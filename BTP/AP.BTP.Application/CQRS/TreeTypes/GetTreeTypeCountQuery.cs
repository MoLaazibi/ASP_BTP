using AP.BTP.Application.Interfaces;
using MediatR;

namespace AP.BTP.Application.CQRS.TreeTypes
{
    public class GetTreeTypeCountQuery : IRequest<int>
    {
        public bool? IsArchived { get; set; }
    }

    public class GetTreeTypeCountQueryHandler : IRequestHandler<GetTreeTypeCountQuery, int>
    {
        private readonly IUnitofWork _unitofWork;

        public GetTreeTypeCountQueryHandler(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }

        public async Task<int> Handle(GetTreeTypeCountQuery request, CancellationToken cancellationToken)
        {
            if (request.IsArchived.HasValue)
                return await _unitofWork.TreeTypeRepository.CountByArchiveStatus(request.IsArchived.Value);

            return await _unitofWork.TreeTypeRepository.CountAsync();
        }
    }
}
