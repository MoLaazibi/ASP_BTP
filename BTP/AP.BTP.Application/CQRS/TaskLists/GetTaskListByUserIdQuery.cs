using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.TaskLists
{
    public class GetTaskListByUserIdQuery : IRequest<IEnumerable<TaskListDTO>>
    {
        public int UserId { get; set; }
    }
    public class GetTaskListByUserIdQueryHandler : IRequestHandler<GetTaskListByUserIdQuery, IEnumerable<TaskListDTO>>
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IMapper _mapper;

        public GetTaskListByUserIdQueryHandler(IUnitofWork unitofWork, IMapper mapper)
        {
            _unitofWork = unitofWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TaskListDTO>> Handle(GetTaskListByUserIdQuery request, CancellationToken cancellationToken)
        {
            var taskLists = await _unitofWork.TaskListRepository.GetByUserId(request.UserId);
            return _mapper.Map<IEnumerable<TaskListDTO>>(taskLists);
        }
    }
}
