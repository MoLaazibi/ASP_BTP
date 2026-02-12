using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.NurserySites
{
    public class EditZoneCommand : IRequest<Result<Zone>>
    {
        public ZoneDTO Zone { get; set; }
    }

    public class EditZoneCommandHandler : IRequestHandler<EditZoneCommand, Result<Zone>>
    {
        private readonly IUnitofWork uow;

        public EditZoneCommandHandler(IUnitofWork uow)
        {
            this.uow = uow;
        }

        public class EditZoneCommandValidator : AbstractValidator<EditZoneCommand>
        {
            private readonly IUnitofWork _uow;

            public EditZoneCommandValidator(IUnitofWork uow)
            {
                _uow = uow;

				RuleFor(z => z.Zone)
	                .NotNull().WithMessage("Zone mag niet null zijn.");

				RuleFor(z => z.Zone.Id)
					.GreaterThan(0).WithMessage("Zone-ID moet groter zijn dan 0.")
					.MustAsync(async (id, cancellation) =>
					{
						var zone = await _uow.ZoneRepository.GetById(id);
						return zone != null;
					})
					.WithMessage("De opgegeven zone bestaat niet.")
					.When(z => z.Zone != null);

				RuleFor(z => z.Zone.Code)
					.NotEmpty().WithMessage("Code is verplicht.")
					.MaximumLength(10).WithMessage("Code mag maximaal 10 karakters lang zijn.")
					.When(z => z.Zone != null);

				RuleFor(z => z.Zone.Size)
					.GreaterThan(0).WithMessage("Grootte van de zone moet groter zijn dan 0.")
					.When(z => z.Zone != null);

				RuleFor(z => z.Zone.TreeTypeId)
					.GreaterThan(0).WithMessage("Boomsoort is verplicht.")
					.When(z => z.Zone != null);

				RuleFor(z => z.Zone.NurserySiteId)
					.GreaterThan(0).WithMessage("NurserySiteId is verplicht en moet groter zijn dan 0.")
					.When(z => z.Zone != null);
			}
        }

        public async Task<Result<Zone>> Handle(EditZoneCommand request, CancellationToken cancellationToken)
        {
            var zoneDTO = request.Zone;
            var existingZone = await uow.ZoneRepository.GetById(zoneDTO.Id);

            if (existingZone == null)
                return Result<Zone>.Failure($"Zone met ID {zoneDTO.Id} niet gevonden.");

            var treeType = await uow.TreeTypeRepository.GetById(zoneDTO.TreeTypeId);
            if (treeType == null)
                return Result<Zone>.Failure($"Boomsoort '{zoneDTO.TreeTypeId}' niet gevonden.");

            existingZone.Code = zoneDTO.Code;
            existingZone.Size = zoneDTO.Size;
            existingZone.TreeTypeId = treeType.Id;

            await uow.ZoneRepository.Update(existingZone);
            await uow.Commit();

            return Result<Zone>.Success(existingZone, "Zone succesvol bijgewerkt");
        }
    }
}
