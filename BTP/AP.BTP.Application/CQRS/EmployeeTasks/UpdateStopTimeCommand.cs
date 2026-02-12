using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.EmployeeTasks
{
    public class UpdateStopTimeCommand : IRequest<EmployeeTaskDTO>
    {
        public int Id { get; set; }
    }
    public class UpdateStopTimeCommandHandler : IRequestHandler<UpdateStopTimeCommand, EmployeeTaskDTO>
    {
        private readonly IUnitofWork _uow;
        private readonly IMapper _mapper;

        public UpdateStopTimeCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<EmployeeTaskDTO> Handle(UpdateStopTimeCommand request, CancellationToken cancellationToken)
        {
            var task = await _uow.EmployeeTaskRepository.GetById(request.Id);
            task.StopTime = DateTime.Now;

            await _uow.EmployeeTaskRepository.Update(task);
            await _uow.Commit();

            if (task.TaskListId.HasValue)
            {
                var tasksInList = await _uow.EmployeeTaskRepository.GetByTaskListId(task.TaskListId.Value);
                var allCompleted = tasksInList != null && tasksInList.Any() && tasksInList.All(t => t.StopTime != null);
                if (allCompleted)
                {
                    var tl = await _uow.TaskListRepository.GetByIdWithDetails(task.TaskListId.Value);
                    if (tl != null && tl.Date.Date <= DateTime.Today)
                    {
                        tl.IsArchived = true;
                        await _uow.TaskListRepository.Update(tl);
                        await _uow.Commit();
                    }
                }
            }
            return _mapper.Map<EmployeeTaskDTO>(task);
        }
    }

}
