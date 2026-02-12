using AP.BTP.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP.BTP.Application.CQRS.TaskLists
{
    public class ArchiveTaskListCommand : IRequest<Result<TaskListDTO>>
    {
        public int TaskListId { get; set; }
    }

    public class ArchiveTaskListCommandHandler : IRequestHandler<ArchiveTaskListCommand, Result<TaskListDTO>>
    {
        private readonly IUnitofWork uow;

        public ArchiveTaskListCommandHandler(IUnitofWork uow)
        {
            this.uow = uow;
        }

        public async Task<Result<TaskListDTO>> Handle(ArchiveTaskListCommand request, CancellationToken cancellationToken)
        {
            var existingTaskList = await uow.TaskListRepository.GetByIdWithDetails(request.TaskListId);

            if (existingTaskList == null)
                return Result<TaskListDTO>.Failure($"Takenlijst met ID {request.TaskListId} niet gevonden.");

            var hasOpenTasks = existingTaskList.Tasks != null && existingTaskList.Tasks.Any(t => t.StopTime == null);
            if (hasOpenTasks)
                return Result<TaskListDTO>.Failure("Archiveren niet mogelijk: er zijn nog onafgewerkte taken.");

            existingTaskList.IsArchived = true;

            await uow.TaskListRepository.Update(existingTaskList);
            await uow.Commit();

            return Result<TaskListDTO>.Success(new TaskListDTO { Id = existingTaskList.Id }, "Takenlijst gearchiveerd.");
        }
    }
}
