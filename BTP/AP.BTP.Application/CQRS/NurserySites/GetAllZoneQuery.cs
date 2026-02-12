using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.NurserySites
{
    public class GetAllZoneQuery : IRequest<IEnumerable<ZoneDTO>>
    {
        public int PageNr { get; set; } = 1;
        public int PageSize { get; set; } = 6;
    }
    public class GetAllZoneQueryHandler : IRequestHandler<GetAllZoneQuery, IEnumerable<ZoneDTO>>
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IMapper _mapper;
        public GetAllZoneQueryHandler(IUnitofWork unitofWork, IMapper mapper)
        {
            _unitofWork = unitofWork;
            _mapper = mapper;
        }
        public async Task<IEnumerable<ZoneDTO>> Handle(GetAllZoneQuery request, CancellationToken cancellationToken)
        {
            var zones = await _unitofWork.ZoneRepository.GetAll(request.PageNr, request.PageSize);
            return _mapper.Map<IEnumerable<ZoneDTO>>(zones);
        }
    }
}
