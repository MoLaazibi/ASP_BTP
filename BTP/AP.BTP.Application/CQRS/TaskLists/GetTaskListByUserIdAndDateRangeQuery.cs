using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.TaskLists
{
    public class GetTaskListByUserIdAndDateRangeQuery : IRequest<IEnumerable<TaskListDTO>>
    {
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class GetTaskListByUserIdAndDateRangeQueryHandler : IRequestHandler<GetTaskListByUserIdAndDateRangeQuery, IEnumerable<TaskListDTO>>
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IMapper _mapper;

        public GetTaskListByUserIdAndDateRangeQueryHandler(IUnitofWork unitofWork, IMapper mapper)
        {
            _unitofWork = unitofWork;
            _mapper = mapper;
        }
        public async Task<IEnumerable<TaskListDTO>> Handle(GetTaskListByUserIdAndDateRangeQuery request, CancellationToken cancellationToken)
        {
            var taskLists = await _unitofWork.TaskListRepository.GetByUserIdAndDateRange(request.UserId, request.StartDate, request.EndDate);
            return _mapper.Map<IEnumerable<TaskListDTO>>(taskLists);
        }
    }
}
