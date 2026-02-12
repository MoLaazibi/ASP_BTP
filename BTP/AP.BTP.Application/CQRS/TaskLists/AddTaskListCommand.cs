using AP.BTP.Application.CQRS.EmployeeTasks;
using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using FluentValidation;
using MediatR;
using System.Globalization;

namespace AP.BTP.Application.CQRS.TaskLists
{
    public class AddTaskListCommand : IRequest<Result<TaskListDTO>>
    {
        public required TaskListDTO TaskList { get; set; }
        public required List<EmployeeTaskDTO> Tasks { get; set; }
    }

    public class AddTaskListCommandValidator : AbstractValidator<AddTaskListCommand>
    {
        private readonly IUnitofWork _uow;

        public AddTaskListCommandValidator(IUnitofWork uow)
        {
            _uow = uow;

            RuleFor(s => s.TaskList)
                .NotNull().WithMessage("TaskList object is required");

            RuleFor(s => s.Tasks)
                .NotEmpty()
                .WithMessage("Een takenlijst heeft minstens 1 taak nodig");

            RuleFor(s => s.TaskList.Date)
                .NotEmpty()
                .WithMessage("Een takenlijst heeft een datum nodig")
                .When(s => s.TaskList != null);

            RuleFor(s => s.TaskList.ZoneId)
                .GreaterThan(0).When(s => s.TaskList != null && s.TaskList.ZoneId.HasValue)
                .WithMessage("Een zone is verplicht.");

            RuleFor(s => s.TaskList.UserId)
                .Must(id => id == null || id >= 0)
                .WithMessage("UserId mag niet negatief zijn.")
                .When(s => s.TaskList != null);

            RuleFor(s => s.TaskList.ZoneId)
                .Must(id => id == null || id >= 0)
                .WithMessage("ZoneId mag niet negatief zijn.")
                .When(s => s.TaskList != null);

            RuleForEach(s => s.Tasks)
                .ChildRules(task =>
                {
                    task.RuleFor(t => t.Description)
                        .NotEmpty()
                        .WithMessage("Elke taak moet een beschrijving hebben.");

                    task.RuleFor(t => t.PlannedDuration)
                        .GreaterThanOrEqualTo(0)
                        .WithMessage("PlannedDuration mag niet negatief zijn.");

                    task.RuleFor(t => t.Order)
                        .GreaterThanOrEqualTo(0)
                        .WithMessage("Order mag niet negatief zijn.");

                    task.RuleFor(t => t.TaskListId)
                        .Must(id => id == null || id >= 0)
                        .WithMessage("TaskListId mag niet negatief zijn.");

                    task.RuleFor(t => t.PlannedStartTime)
                        .NotEmpty()
                        .WithMessage("Elke taak moet een geplande starttijd hebben.");

                    task.RuleFor(t => t.CategoryId)
                        .NotNull().WithMessage("De categorie is verplicht.")
                        .GreaterThan(0).WithMessage("De categorie is verplicht.");
                });

            RuleFor(s => s.Tasks)
                .Must(NoOverlappingTasks)
                .WithMessage("Er zijn overlappende taken in de takenlijst. Controleer de starttijden en duur.");

            RuleFor(s => s.Tasks.Count)
                .LessThanOrEqualTo(4)
                .WithMessage("Een takenlijst mag maximaal 4 taken bevatten.");

            RuleFor(s => s.TaskList.UserId)
                .MustAsync(UserHasLessThanFiveInWeek)
                .WithMessage("Deze medewerker heeft al 5 takenlijsten toegewezen gekregen dit week.")
                .When(s => s.TaskList != null && s.TaskList.UserId.HasValue);

            RuleFor(s => s.TaskList.UserId)
                .MustAsync(UserHasTaskListInOtherNurserySiteInSameWeek)
                .WithMessage("Deze medewerker kan alleen takenlijsten toegewezen krijgen binnen dezelfde kweeksite in dezelfde week.")
                .When(s => s.TaskList != null && s.TaskList.UserId.HasValue);
        }

        private static bool NoOverlappingTasks(List<EmployeeTaskDTO> tasks)
        {
            if (tasks == null || tasks.Count < 2)
                return true;

            var ordered = tasks.OrderBy(t => t.PlannedStartTime).ToList();

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                int currStart = (int)ordered[i].PlannedStartTime.TimeOfDay.TotalMinutes;
                int currEnd = currStart + ordered[i].PlannedDuration * 60;
                int nextStart = (int)ordered[i + 1].PlannedStartTime.TimeOfDay.TotalMinutes;

                if (nextStart < currEnd)
                    return false;
            }
            return true;
        }

        private async Task<bool> UserHasLessThanFiveInWeek(AddTaskListCommand command, int? userId, CancellationToken ct)
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

        private async Task<bool> UserHasTaskListInOtherNurserySiteInSameWeek(AddTaskListCommand command, int? userId, CancellationToken ct)
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
                .Any(tl => tl.Zone?.NurserySite?.Id != currentZone.NurserySite.Id);

            return !hasDifferentNurserySite;
        }
    }

    public class AddTaskListCommandHandler : IRequestHandler<AddTaskListCommand, Result<TaskListDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;

        public AddTaskListCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        public async Task<Result<TaskListDTO>> Handle(AddTaskListCommand request, CancellationToken cancellationToken)
        {
            var newTaskList = new TaskList
            {
                Date = request.TaskList.Date,
                Tasks = request.Tasks.Select(taskDto => new EmployeeTask
                {
                    Description = taskDto.Description,
                    PlannedDuration = taskDto.PlannedDuration,
                    PlannedStartTime = taskDto.PlannedStartTime,
                    StopTime = taskDto.StopTime,
                    Order = taskDto.Order,
                    CategoryId = taskDto.CategoryId
                }).ToList(),
                UserId = request.TaskList.UserId,
                ZoneId = request.TaskList.ZoneId,
                IsArchived = false

            };

            await uow.TaskListRepository.Create(newTaskList);
            await uow.Commit();
            var dto = mapper.Map<TaskListDTO>(newTaskList);
            return Result<TaskListDTO>.Success(dto, "TaskList successfully created");
        }
    }
}
