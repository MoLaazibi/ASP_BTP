using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.TaskLists
{
    public class GetTaskListByUserIdAndDateQuery : IRequest<TaskListDTO>
    {
        public int UserId { get; set; }
        public DateTime Date { get; set; }
    }
    public class GetTaskListByUserIdAndDateQueryHandler : IRequestHandler<GetTaskListByUserIdAndDateQuery, TaskListDTO>
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IMapper _mapper;

        public GetTaskListByUserIdAndDateQueryHandler(IUnitofWork unitofWork, IMapper mapper)
        {
            _unitofWork = unitofWork;
            _mapper = mapper;
        }

        public async Task<TaskListDTO> Handle(GetTaskListByUserIdAndDateQuery request, CancellationToken cancellationToken)
        {
            var taskLists = await _unitofWork.TaskListRepository.GetByUserIdAndDate(request.UserId, request.Date);
            return _mapper.Map<TaskListDTO>(taskLists);
        }
    }
}
