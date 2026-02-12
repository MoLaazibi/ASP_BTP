using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.TaskLists
{
    public class GetAllTaskListQuery : IRequest<IEnumerable<TaskListDTO>>
    {
        public int PageNr { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool? IsArchived { get; set; }
    }

    public class GetAllTaskListQueryHandler : IRequestHandler<GetAllTaskListQuery, IEnumerable<TaskListDTO>>
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IMapper _mapper;

        public GetAllTaskListQueryHandler(IUnitofWork unitofWork, IMapper mapper)
        {
            _unitofWork = unitofWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TaskListDTO>> Handle(GetAllTaskListQuery request, CancellationToken cancellationToken)
        {
            var taskLists = request.IsArchived.HasValue
                ? await _unitofWork.TaskListRepository.GetByArchiveStatus(request.IsArchived.Value, request.PageNr, request.PageSize)
                : await _unitofWork.TaskListRepository.GetAll(request.PageNr, request.PageSize);
            return _mapper.Map<IEnumerable<TaskListDTO>>(taskLists);
        }
    }
}
