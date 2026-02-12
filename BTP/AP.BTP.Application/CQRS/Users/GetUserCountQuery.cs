using AP.BTP.Application.Interfaces;
using MediatR;

namespace AP.BTP.Application.CQRS.Users
{
    public class GetUserCountQuery : IRequest<int>
    {
    }

    public class GetUserCountQueryHandler : IRequestHandler<GetUserCountQuery, int>
    {
        private readonly IUnitofWork _unitofWork;

        public GetUserCountQueryHandler(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }

        public async Task<int> Handle(GetUserCountQuery request, CancellationToken cancellationToken)
        {
            return await _unitofWork.UserRepository.CountAsync();
        }
    }
}

