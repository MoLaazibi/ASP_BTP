using AP.BTP.Application.Interfaces;
using MediatR;

namespace AP.BTP.Application.CQRS.TaskLists
{
    public class GetTaskListCountByUserWeekQuery : IRequest<int>
    {
        public int UserId { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }
    }
    public class GetTaskListCountByUserWeekHandler : IRequestHandler<GetTaskListCountByUserWeekQuery, int>
    {
        private readonly IUnitofWork _uow;

        public GetTaskListCountByUserWeekHandler(IUnitofWork uow)
        {
            _uow = uow;
        }

        public async Task<int> Handle(GetTaskListCountByUserWeekQuery request, CancellationToken cancellationToken)
        {
            return await _uow.TaskListRepository.GetCountByUserAndWeek(
                request.UserId, request.Year, request.Week
            );
        }
    }

}
