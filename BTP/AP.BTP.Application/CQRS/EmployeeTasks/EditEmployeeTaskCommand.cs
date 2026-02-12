using AP.BTP.Application.Interfaces;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.EmployeeTasks
{
    public class EditEmployeeTaskCommand : IRequest<Result<EmployeeTaskDTO>>
    {
        public required EmployeeTaskDTO EmployeeTask { get; set; }
    }

    public class EditEmployeeTaskCommandValidator : AbstractValidator<EditEmployeeTaskCommand>
    {
        private readonly IUnitofWork _uow;

        public EditEmployeeTaskCommandValidator(IUnitofWork uow)
        {
            _uow = uow;

            RuleFor(c => c.EmployeeTask)
                .NotNull().WithMessage("Taak mag niet null zijn.");

            RuleFor(c => c.EmployeeTask.Id)
                .GreaterThan(0).WithMessage("Taak-ID moet groter zijn dan 0.")
                .MustAsync(async (id, cancellation) =>
                {
                    var existing = await _uow.EmployeeTaskRepository.GetById(id);
                    return existing != null;
                })
                .WithMessage("De opgegeven taak bestaat niet.");

            RuleFor(c => c.EmployeeTask.Description)
                .NotEmpty().WithMessage("Omschrijving is verplicht.")
                .MaximumLength(255).WithMessage("Omschrijving mag maximaal 255 karakters lang zijn.");

            RuleFor(c => c.EmployeeTask.PlannedDuration)
                .GreaterThan(0).WithMessage("De geplande duur moet groter zijn dan 0.");

            RuleFor(c => c.EmployeeTask.PlannedStartTime)
                .NotEmpty().WithMessage("De geplande starttijd is verplicht.");

            RuleFor(c => c.EmployeeTask.StartTime)
                .LessThanOrEqualTo(c => c.EmployeeTask.StopTime)
                .When(c => c.EmployeeTask.StartTime != null && c.EmployeeTask.StopTime != null)
                .WithMessage("De starttijd moet vóór de stoptijd liggen.");

            RuleFor(c => c.EmployeeTask.TaskListId)
                .GreaterThan(0).WithMessage("Elke taak moet gekoppeld zijn aan een geldige takenlijst.");

            RuleFor(c => c.EmployeeTask.CategoryId)
                .NotNull().WithMessage("De categorie is verplicht.")
                .GreaterThan(0).WithMessage("De categorie is verplicht.");
        }
    }

    public class EditEmployeeTaskCommandHandler : IRequestHandler<EditEmployeeTaskCommand, Result<EmployeeTaskDTO>>
    {
        private readonly IUnitofWork _uow;
        private readonly IMapper _mapper;

        public EditEmployeeTaskCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<EmployeeTaskDTO>> Handle(EditEmployeeTaskCommand request, CancellationToken cancellationToken)
        {
            var dto = request.EmployeeTask;
            var existingTask = await _uow.EmployeeTaskRepository.GetById(dto.Id);
            if (existingTask == null)
                return Result<EmployeeTaskDTO>.Failure($"Taak met ID {dto.Id} niet gevonden.");

            existingTask.Description = dto.Description;
            existingTask.PlannedDuration = dto.PlannedDuration;
            existingTask.PlannedStartTime = dto.PlannedStartTime;
            existingTask.Order = dto.Order;
            existingTask.CategoryId = dto.CategoryId;

            await _uow.EmployeeTaskRepository.Update(existingTask);
            await _uow.Commit();

            var updatedDto = _mapper.Map<EmployeeTaskDTO>(existingTask);
            return Result<EmployeeTaskDTO>.Success(updatedDto, "Taak succesvol bijgewerkt.");
        }
    }
}
