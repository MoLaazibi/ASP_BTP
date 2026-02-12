using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.NurserySites
{
    public class AddNurserySiteCommand : IRequest<Result<NurserySiteDTO>>
    {
        public required NurserySiteDTO NurserySite { get; set; }
        public required AddressDTO Address { get; set; }
        public required GroundPlanDTO GroundPlan { get; set; }
        public int UserId { get; set; }
        public List<ZoneDTO> Zones { get; set; } = new();
    }
    public class AddNurserySiteCommandValidator : AbstractValidator<AddNurserySiteCommand>
    {
        private readonly IUnitofWork _uow;
        public AddNurserySiteCommandValidator(IUnitofWork uow)
        {
            this._uow = uow;

            RuleFor(s => s.NurserySite)
           .NotNull().WithMessage("Kweeksite mag niet null zijn.");

            RuleFor(s => s.NurserySite.Name)
                .NotEmpty().WithMessage("Naam is verplicht.")
                .MaximumLength(100)
                .WithMessage("De naam van een kweeksite mag maximaal 100 characters lang zijn");

            RuleFor(s => s.NurserySite.AddressId)
                .GreaterThan(-1)
                .WithMessage("AddressId mag niet negatief zijn.");

            RuleFor(s => s.Address)
                .NotNull().WithMessage("Een kweeksite moet een adres hebben.")
                .ChildRules(address =>
                {
                    address.RuleFor(a => a.Id)
                        .GreaterThan(-1)
                        .WithMessage("Adres-Id mag niet negatief zijn.");

                    address.RuleFor(a => a.StreetName)
                        .NotEmpty().WithMessage("Straatnaam is verplicht.")
                        .MaximumLength(100)
                        .WithMessage("Een straatnaam mag maximaal 100 characters lang zijn");

                    address.RuleFor(a => a.HouseNumber)
                        .NotEmpty().WithMessage("Huisnummer is verplicht.")
                        .MaximumLength(10)
                        .WithMessage("Een huisnummer mag maximaal 10 characters lang zijn");

                    address.RuleFor(a => a.PostalCode)
                        .NotEmpty().WithMessage("Postcode is verplicht.")
                        .MinimumLength(4)
                        .MaximumLength(10)
                        .WithMessage("Een postcode moet tussen 4 en 10 characters lang zijn");
                });

            RuleFor(s => s.GroundPlan)
                .NotNull().WithMessage("Een kweeksite moet een plattegrond hebben.")
                .ChildRules(gp =>
                {
                    gp.RuleFor(g => g.Id)
                        .GreaterThan(-1)
                        .WithMessage("Plattegrond-Id mag niet negatief zijn.");

                    gp.RuleFor(g => g.FileUrl)
                        .NotEmpty().WithMessage("Plattegrond-bestand is verplicht.");
                });

            RuleForEach(s => s.Zones).ChildRules(zone =>
            {
                zone.RuleFor(z => z.Id)
                    .GreaterThan(-1)
                    .WithMessage("Zone-Id mag niet negatief zijn.");

                zone.RuleFor(z => z.Code)
                    .NotEmpty().WithMessage("Elke zone moet een code hebben.")
                    .MaximumLength(10).WithMessage("Code mag maximaal 10 karakters lang zijn.");

                zone.RuleFor(z => z.Size)
                    .GreaterThan(0).WithMessage("Grootte van een zone moet groter zijn dan 0.");

                zone.RuleFor(z => z.TreeTypeId)
                    .GreaterThan(0).WithMessage("Elke zone moet een boomsoort hebben.");
            });

            RuleFor(c => c.UserId)
                .MustAsync(async (userId, cancellation) =>
                {
                    var existing = await _uow.NurserySiteRepository
                        .FindAsync(ns => ns.UserId == userId);

                    return existing == null;
                })
                .WithMessage("Deze verantwoordelijke is al gekoppeld aan een andere kweeksite.");

            RuleFor(s => s)
                .MustAsync(async (command, cancellation) =>
                {
                    var existingAddress = await _uow.AddressRepository.FindAsync(a =>
                        a.StreetName == command.Address.StreetName &&
                        a.HouseNumber == command.Address.HouseNumber &&
                        a.PostalCode == command.Address.PostalCode);

                    if (existingAddress == null)
                        return true;

                    var site = await _uow.NurserySiteRepository.GetById(command.NurserySite.Id);
                    return site != null && site.AddressId == existingAddress.Id;
                })
                .WithMessage("Er bestaat al een andere kweeksite met hetzelfde adres.");
        }
    }
    public class AddNurserySiteCommandHandler : IRequestHandler<AddNurserySiteCommand, Result<NurserySiteDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;
        private readonly IMediator mediator;

        public AddNurserySiteCommandHandler(IUnitofWork uow, IMapper mapper, IMediator mediator)
        {
            this.uow = uow;
            this.mapper = mapper;
            this.mediator = mediator;
        }

        public async Task<Result<NurserySiteDTO>> Handle(AddNurserySiteCommand request, CancellationToken cancellationToken)
        {
            var newSite = new NurserySite
            {
                Name = request.NurserySite.Name,
                GroundPlanId = request.NurserySite.GroundPlan.Id,
                Address = new Address
                {
                    StreetName = request.Address.StreetName,
                    PostalCode = request.Address.PostalCode,
                    HouseNumber = request.Address.HouseNumber,

                },
                GroundPlan = new GroundPlan
                {
                    FileUrl = request.GroundPlan.FileUrl,
                    UploadTime = request.GroundPlan.UploadTime,
                },
                UserId = request.UserId,
                Zones = new List<Zone>()
            };

            await uow.NurserySiteRepository.Create(newSite);
            await uow.Commit();

            foreach (var zoneDTO in request.Zones)
            {
                zoneDTO.NurserySiteId = newSite.Id;
                await mediator.Send(new AddZoneCommand { Zone = zoneDTO });
            }

            var dto = mapper.Map<NurserySiteDTO>(newSite);

            return Result<NurserySiteDTO>.Success(dto, "Kweeksite succesvol aangemaakt");
        }
    }
}
