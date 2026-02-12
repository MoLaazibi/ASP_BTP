using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.NurserySites
{
    public class GetNurserySiteByIdQuery : IRequest<NurserySiteDTO>
    {
        public int Id { get; set; }
    }

    public class GetNurserySiteByIdQueryHandler : IRequestHandler<GetNurserySiteByIdQuery, NurserySiteDTO>
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IMapper _mapper;

        public GetNurserySiteByIdQueryHandler(IUnitofWork unitofWork, IMapper mapper)
        {
            _unitofWork = unitofWork;
            _mapper = mapper;
        }

        public async Task<NurserySiteDTO> Handle(GetNurserySiteByIdQuery request, CancellationToken cancellationToken)
        {
            var nurserySite = await _unitofWork.NurserySiteRepository.GetByIdWithDetails(request.Id);

            if (nurserySite == null)
                return null;

            return _mapper.Map<NurserySiteDTO>(nurserySite);
        }
    }
}
