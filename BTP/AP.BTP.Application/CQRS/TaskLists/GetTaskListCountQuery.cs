using AP.BTP.Application.Interfaces;
using MediatR;

namespace AP.BTP.Application.CQRS.TaskLists
{
    public class GetTaskListCountQuery : IRequest<int>
    {
        public bool? IsArchived { get; set; }
    }

    public class GetTaskListCountQueryHandler : IRequestHandler<GetTaskListCountQuery, int>
    {
        private readonly IUnitofWork _unitofWork;

        public GetTaskListCountQueryHandler(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }

        public async Task<int> Handle(GetTaskListCountQuery request, CancellationToken cancellationToken)
        {
            if (request.IsArchived.HasValue)
                return await _unitofWork.TaskListRepository.CountByArchiveStatus(request.IsArchived.Value);

            return await _unitofWork.TaskListRepository.CountAsync();
        }
    }
}
