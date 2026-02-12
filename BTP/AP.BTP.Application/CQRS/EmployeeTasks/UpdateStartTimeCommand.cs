using AP.BTP.Application.Interfaces;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.EmployeeTasks
{
    public class UpdateStartTimeCommand : IRequest<EmployeeTaskDTO>
    {
        public int Id { get; set; }
    }
    public class UpdateStartTimeCommandValidator : AbstractValidator<UpdateStartTimeCommand>
    {
        private readonly IUnitofWork _uow;

        public UpdateStartTimeCommandValidator(IUnitofWork uow)
        {
            _uow = uow;

            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Task ID moet groter zijn dan 0.");

			RuleFor(x => x)
	            .MustAsync(NoOtherTaskActive)
	            .When(x => x.Id > 0)
	            .WithMessage("Er is al een taak bezig. Werk deze eerst af.");

			RuleFor(x => x)
				.MustAsync(PreviousTasksMustBeDone)
				.When(x => x.Id > 0)
				.WithMessage("Je moet eerst de vorige taken afwerken voordat je deze taak kan starten.");
		}

        private async Task<bool> NoOtherTaskActive(UpdateStartTimeCommand command, CancellationToken ct)
        {
            var task = await _uow.EmployeeTaskRepository.GetById(command.Id);
            var taskListId = task.TaskListId ?? 0;
            Console.WriteLine("tasklistId: " + taskListId);
            var taskList = await _uow.TaskListRepository.GetById(taskListId);
            foreach (var employeeTask in taskList.Tasks)
            {
                Console.WriteLine(employeeTask.Description);
            }

            return !taskList.Tasks.Any(t => t.Id != command.Id &&
                                   t.StartTime != null &&
                                   t.StopTime == null);
        }

        private async Task<bool> PreviousTasksMustBeDone(UpdateStartTimeCommand command, CancellationToken ct)
        {
            var task = await _uow.EmployeeTaskRepository.GetById(command.Id);
            var taskListId = task.TaskListId ?? 0;

            var taskList = await _uow.TaskListRepository.GetById(taskListId);

            return !taskList.Tasks.Any(t => t.Order < task.Order &&
                                   t.StopTime == null);
        }
    }

    public class UpdateStartTimeCommandHandler : IRequestHandler<UpdateStartTimeCommand, EmployeeTaskDTO>
    {
        private readonly IUnitofWork _uow;
        private readonly IMapper _mapper;

        public UpdateStartTimeCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<EmployeeTaskDTO> Handle(UpdateStartTimeCommand request, CancellationToken cancellationToken)
        {
            var task = await _uow.EmployeeTaskRepository.GetById(request.Id);
            task.StartTime = DateTime.Now;

            await _uow.EmployeeTaskRepository.Update(task);
            await _uow.Commit();
            return _mapper.Map<EmployeeTaskDTO>(task);
        }
    }

}
