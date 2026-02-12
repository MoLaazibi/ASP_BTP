using AP.BTP.Application.CQRS.EmployeeTasks;
using AP.BTP.Application.Interfaces;
using MediatR;

namespace AP.BTP.Application.CQRS.TaskLists
{
    public class DeleteEmployeeTaskCommand : IRequest<Result<EmployeeTaskDTO>>
    {
        public EmployeeTaskDTO EmployeeTask { get; set; }
    }

    public class DeleteEmployeeTaskCommandHandler : IRequestHandler<DeleteEmployeeTaskCommand, Result<EmployeeTaskDTO>>
    {
        private readonly IUnitofWork uow;

        public DeleteEmployeeTaskCommandHandler(IUnitofWork uow)
        {
            this.uow = uow;
        }

        public async Task<Result<EmployeeTaskDTO>> Handle(DeleteEmployeeTaskCommand request, CancellationToken cancellationToken)
        {
            var existingEmployeeTask = await uow.EmployeeTaskRepository.GetById(request.EmployeeTask.Id);

            if (existingEmployeeTask == null)
                return Result<EmployeeTaskDTO>.Failure($"EmployeeTask met ID {request.EmployeeTask.Id} niet gevonden.");

            await uow.EmployeeTaskRepository.Delete(existingEmployeeTask);
            await uow.Commit();

            return Result<EmployeeTaskDTO>.Success(request.EmployeeTask, "EmployeeTask succesvol verwijderd");
        }
    }
}
