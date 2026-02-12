using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.TaskLists
{
    public class GetTaskListByIdQuery : IRequest<TaskListDTO>
    {
        public int Id { get; set; }
    }

    public class GetTaskListByIdQueryHandler : IRequestHandler<GetTaskListByIdQuery, TaskListDTO>
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IMapper _mapper;

        public GetTaskListByIdQueryHandler(IUnitofWork unitofWork, IMapper mapper)
        {
            _unitofWork = unitofWork;
            _mapper = mapper;
        }

        public async Task<TaskListDTO> Handle(GetTaskListByIdQuery request, CancellationToken cancellationToken)
        {
            var taskList = await _unitofWork.TaskListRepository.GetByIdWithDetails(request.Id);

            if (taskList == null)
                return null;

            return _mapper.Map<TaskListDTO>(taskList);
        }
    }
}
