using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.NurserySites
{
    public class GetAllNurserySiteQuery : IRequest<IEnumerable<NurserySiteDTO>>
    {
        public int PageNr { get; set; } = 1;
        public int PageSize { get; set; } = 6;
        public bool? IsArchived { get; set; }
    }

    public class GetAllNurserySiteQueryHandler : IRequestHandler<GetAllNurserySiteQuery, IEnumerable<NurserySiteDTO>>
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IMapper _mapper;

        public GetAllNurserySiteQueryHandler(IUnitofWork unitofWork, IMapper mapper)
        {
            _unitofWork = unitofWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NurserySiteDTO>> Handle(GetAllNurserySiteQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<Domain.NurserySite> nurserySites;
            if (request.IsArchived.HasValue)
            {
                nurserySites = await _unitofWork.NurserySiteRepository.GetByArchiveStatus(request.IsArchived.Value, request.PageNr, request.PageSize);
            }
            else
            {
                nurserySites = await _unitofWork.NurserySiteRepository.GetAll(request.PageNr, request.PageSize);
            }
            return _mapper.Map<IEnumerable<NurserySiteDTO>>(nurserySites);
        }
    }
}
