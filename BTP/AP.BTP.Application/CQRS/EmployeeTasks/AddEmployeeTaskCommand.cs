using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.EmployeeTasks
{
    public class AddEmployeeTaskCommand : IRequest<Result<EmployeeTaskDTO>>
    {
        public EmployeeTaskDTO EmployeeTask { get; set; }
    }

    public class AddEmployeeTaskCommandValidator : AbstractValidator<AddEmployeeTaskCommand>
    {

        public AddEmployeeTaskCommandValidator(IUnitofWork uow)
        {
            RuleFor(c => c.EmployeeTask)
                .NotNull().WithMessage("Taak mag niet null zijn.");

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

    public class AddEmployeeTaskCommandHandler : IRequestHandler<AddEmployeeTaskCommand, Result<EmployeeTaskDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;

        public AddEmployeeTaskCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }
        public async Task<Result<EmployeeTaskDTO>> Handle(AddEmployeeTaskCommand request, CancellationToken cancellationToken)
        {
            var dto = request.EmployeeTask;
            var newEmployeeTask = new EmployeeTask
            {
                Description = dto.Description,
                PlannedDuration = dto.PlannedDuration,
                PlannedStartTime = dto.PlannedStartTime,

                StartTime = dto.StartTime,
                StopTime = dto.StopTime,
                Order = dto.Order,
                TaskListId = dto.TaskListId,
                CategoryId = dto.CategoryId,
            };

            await uow.EmployeeTaskRepository.Create(newEmployeeTask);
            await uow.Commit();

            var newdto = mapper.Map<EmployeeTaskDTO>(newEmployeeTask);
            return Result<EmployeeTaskDTO>.Success(newdto, "EmployeeTask succesvol toegevoegd");
        }
    }
}