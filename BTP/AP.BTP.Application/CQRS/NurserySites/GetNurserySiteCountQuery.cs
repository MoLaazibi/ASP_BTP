using AP.BTP.Application.Interfaces;
using MediatR;

namespace AP.BTP.Application.CQRS.NurserySites
{
    public class GetNurserySiteCountQuery : IRequest<int>
    {
    }

    public class GetNurserySiteCountQueryHandler : IRequestHandler<GetNurserySiteCountQuery, int>
    {
        private readonly IUnitofWork _unitofWork;

        public GetNurserySiteCountQueryHandler(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }

        public async Task<int> Handle(GetNurserySiteCountQuery request, CancellationToken cancellationToken)
        {
            return await _unitofWork.NurserySiteRepository.CountAsync();
        }
    }
}