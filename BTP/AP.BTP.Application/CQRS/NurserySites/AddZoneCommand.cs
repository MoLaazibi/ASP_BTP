using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.NurserySites
{
    public class AddZoneCommand : IRequest<Result<ZoneDTO>>
    {
        public ZoneDTO Zone { get; set; }
    }
    public class AddZoneCommandHandler : IRequestHandler<AddZoneCommand, Result<ZoneDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;

        public AddZoneCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }
        public async Task<Result<ZoneDTO>> Handle(AddZoneCommand request, CancellationToken cancellationToken)
        {
            var newZone = new Zone
            {
                Code = request.Zone.Code,
                Size = request.Zone.Size,
                TreeTypeId = request.Zone.TreeTypeId,
                NurserySiteId = request.Zone.NurserySiteId,
            };

            await uow.ZoneRepository.Create(newZone);
            await uow.Commit();

            var dto = mapper.Map<ZoneDTO>(newZone);

            return Result<ZoneDTO>.Success(dto, "Zone succesvol toegevoegd");
        }
    }
}
