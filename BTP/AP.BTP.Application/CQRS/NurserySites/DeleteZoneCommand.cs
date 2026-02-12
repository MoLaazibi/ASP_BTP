using AP.BTP.Application.Interfaces;
using MediatR;

namespace AP.BTP.Application.CQRS.NurserySites
{
    public class DeleteZoneCommand : IRequest<Result<ZoneDTO>>
    {
        public ZoneDTO Zone { get; set; }
    }

    public class DeleteZoneCommandHandler : IRequestHandler<DeleteZoneCommand, Result<ZoneDTO>>
    {
        private readonly IUnitofWork uow;

        public DeleteZoneCommandHandler(IUnitofWork uow)
        {
            this.uow = uow;
        }

        public async Task<Result<ZoneDTO>> Handle(DeleteZoneCommand request, CancellationToken cancellationToken)
        {
            var existingZone = await uow.ZoneRepository.GetById(request.Zone.Id);

            if (existingZone == null)
                return Result<ZoneDTO>.Failure($"Zone met ID {request.Zone.Id} niet gevonden.");

            await uow.ZoneRepository.Delete(existingZone);
            await uow.Commit();

            return Result<ZoneDTO>.Success(request.Zone, "Zone succesvol verwijderd");
        }
    }
}
