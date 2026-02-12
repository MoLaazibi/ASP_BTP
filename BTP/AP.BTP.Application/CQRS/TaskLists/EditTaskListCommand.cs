using AP.BTP.Application.CQRS.EmployeeTasks;
using AP.BTP.Application.Interfaces;
using AutoMapper;
using FluentValidation;
using MediatR;
using System.Globalization;

namespace AP.BTP.Application.CQRS.TaskLists
{
    public class EditTaskListCommand : IRequest<Result<TaskListDTO>>
    {
        public required TaskListDTO TaskList { get; set; }
        public required List<EmployeeTaskDTO> Tasks { get; set; }
    }

    public class EditTaskListCommandValidator : AbstractValidator<EditTaskListCommand>
    {
        private readonly IUnitofWork _uow;

        public EditTaskListCommandValidator(IUnitofWork uow)
        {
            _uow = uow;

            RuleFor(c => c.TaskList)
                .NotNull().WithMessage("Takenlijst mag niet null zijn.");

            RuleFor(c => c.TaskList.Id)
                .GreaterThan(0).WithMessage("Takenlijst-ID moet groter zijn dan 0.")
                .MustAsync(async (id, cancellation) =>
                {
                    var existing = await _uow.TaskListRepository.GetById(id);
                    return existing != null;
                })
                .WithMessage("De opgegeven takenlijst bestaat niet.")
                .When(c => c.TaskList != null);

            RuleFor(c => c.TaskList.ZoneId)
                .GreaterThan(0).When(c => c.TaskList != null && c.TaskList.ZoneId.HasValue)
                .WithMessage("Een zone is verplicht.");

            RuleForEach(c => c.Tasks).ChildRules(task =>
            {
                task.RuleFor(t => t.Description)
                    .NotEmpty().WithMessage("Elke taak moet een beschrijving hebben.")
                    .MaximumLength(255).WithMessage("Omschrijving mag maximaal 255 karakters lang zijn.");

                task.RuleFor(t => t.PlannedDuration)
                    .GreaterThan(0).WithMessage("De geplande duur moet groter zijn dan 0.");

                task.RuleFor(t => t.PlannedStartTime)
                    .NotEmpty().WithMessage("De geplande starttijd is verplicht.");

                task.RuleFor(t => t.CategoryId)
                        .NotNull().WithMessage("De categorie is verplicht.")
                        .GreaterThan(0).WithMessage("De categorie is verplicht.");
            });

            RuleFor(c => c)
                .Must(NoOverlappingTasks)
                .WithMessage("Er zijn overlappende taken. Controleer de geplande tijden.");

            RuleFor(s => s.TaskList.UserId)
                .MustAsync(UserHasLessThanFiveInWeek)
                .WithMessage("Deze medewerker heeft al 5 takenlijsten toegewezen gekregen deze week.")
                .When(s => s.TaskList != null && s.TaskList.UserId.HasValue);

            RuleFor(s => s.TaskList.UserId)
                .MustAsync(UserHasTaskListInOtherNurserySiteInSameWeek)
                .WithMessage("Deze medewerker kan alleen takenlijsten toegewezen krijgen binnen dezelfde kweeksite in dezelfde week.")
                .When(s => s.TaskList != null && s.TaskList.UserId.HasValue);
        }

        private static bool NoOverlappingTasks(EditTaskListCommand command)
        {
            if (command.Tasks == null || command.Tasks.Count <= 1)
                return true;

            var sorted = command.Tasks.OrderBy(t => t.PlannedStartTime.TimeOfDay).ToList();

            for (int i = 0; i < sorted.Count - 1; i++)
            {
                var current = sorted[i];
                var next = sorted[i + 1];

                int currentStartMinutes = (int)current.PlannedStartTime.TimeOfDay.TotalMinutes;
                int currentEndMinutes = currentStartMinutes + current.PlannedDuration * 60;
                int nextStartMinutes = (int)next.PlannedStartTime.TimeOfDay.TotalMinutes;

                if (nextStartMinutes < currentEndMinutes)
                    return false;
            }

            return true;
        }

        private async Task<bool> UserHasLessThanFiveInWeek(EditTaskListCommand command, int? userId, CancellationToken ct)
        {
            if (!userId.HasValue)
                return true;

            var date = command.TaskList.Date;

            var culture = CultureInfo.CurrentCulture;
            int week = culture.Calendar.GetWeekOfYear(
                date,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
            int year = date.Year;

            var count = await _uow.TaskListRepository
                .GetCountByUserAndWeek(userId.Value, year, week);

            return count < 5;
        }

        private async Task<bool> UserHasTaskListInOtherNurserySiteInSameWeek(EditTaskListCommand command, int? userId, CancellationToken ct)
        {
            if (!userId.HasValue || command.TaskList == null || !command.TaskList.ZoneId.HasValue)
                return true;

            var currentZone = await _uow.ZoneRepository.GetByIdWithNurserySite(command.TaskList.ZoneId.Value);
            if (currentZone == null || currentZone.NurserySite == null)
                return true;

            var date = command.TaskList.Date;
            var culture = CultureInfo.CurrentCulture;
            int week = culture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            int year = date.Year;

            var userTaskListsInWeek = await _uow.TaskListRepository.GetByUserAndWeekWithZoneAndNurserySite(userId.Value, year, week);

            bool hasDifferentNurserySite = userTaskListsInWeek
                .Where(tl => tl.Id != command.TaskList.Id)
                .Any(tl => tl.Zone?.NurserySite?.Id != currentZone.NurserySite.Id);

            return !hasDifferentNurserySite;
        }
    }

    public class EditTaskListCommandHandler : IRequestHandler<EditTaskListCommand, Result<TaskListDTO>>
    {
        private readonly IUnitofWork _uow;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public EditTaskListCommandHandler(IUnitofWork uow, IMapper mapper, IMediator mediator)
        {
            _uow = uow;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<Result<TaskListDTO>> Handle(EditTaskListCommand request, CancellationToken cancellationToken)
        {
            var dto = request.TaskList;
            var dtoTasks = request.Tasks;
            var existing = await _uow.TaskListRepository.GetByIdWithDetails(dto.Id);
            if (existing == null)
                return Result<TaskListDTO>.Failure($"Takenlijst met ID {dto.Id} niet gevonden.");

            existing.Date = dto.Date;
            existing.UserId = dto.UserId;
            existing.ZoneId = dto.ZoneId;

            foreach (var taskDto in dtoTasks)
            {
                if (taskDto.Id == 0 && !taskDto.IsDeleted)
                {
                    taskDto.TaskListId = existing.Id;
                    await _mediator.Send(new AddEmployeeTaskCommand { EmployeeTask = taskDto });
                }
                if (taskDto.Id > 0 && !taskDto.IsDeleted)
                {
                    await _mediator.Send(new EditEmployeeTaskCommand { EmployeeTask = taskDto });
                }
                else if (taskDto.IsDeleted)
                {
                    await _mediator.Send(new DeleteEmployeeTaskCommand { EmployeeTask = taskDto });
                }
            }

            await _uow.TaskListRepository.Update(existing);
            await _uow.Commit();

            var updatedDto = _mapper.Map<TaskListDTO>(existing);
            return Result<TaskListDTO>.Success(updatedDto, "Takenlijst succesvol bijgewerkt.");
        }
    }
}
