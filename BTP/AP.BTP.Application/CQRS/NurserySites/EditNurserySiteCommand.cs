using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.NurserySites
{
    public class EditNurserySiteCommand : IRequest<Result<NurserySiteDTO>>
    {
        public required NurserySiteDTO NurserySite { get; set; }
        public required AddressDTO Address { get; set; }
        public required GroundPlanDTO GroundPlan { get; set; }
        public required int UserId { get; set; }
        public required List<ZoneDTO> Zones { get; set; }
    }

    public class EditNurserySiteCommandValidator : AbstractValidator<EditNurserySiteCommand>
    {
        private readonly IUnitofWork _uow;

        public EditNurserySiteCommandValidator(IUnitofWork uow)
        {
            _uow = uow;

            RuleFor(s => s.NurserySite)
                .NotNull().WithMessage("Kweeksite mag niet null zijn.");

            RuleFor(s => s.NurserySite.Id)
                .GreaterThan(0).WithMessage("Kweeksite-ID moet groter zijn dan 0.")
                .MustAsync(async (id, cancellation) =>
                {
                    var site = await _uow.NurserySiteRepository.GetById(id);
                    return site != null;
                })
                .WithMessage("De opgegeven kweeksite bestaat niet.");

            RuleFor(s => s.NurserySite.Name)
                .NotEmpty().WithMessage("Naam is verplicht.")
                .MaximumLength(100)
                .WithMessage("De naam van een kweeksite mag maximaal 100 characters lang zijn");

            RuleFor(s => s.Address)
                .NotNull().WithMessage("Een kweeksite moet een adres hebben.")
                .ChildRules(address =>
                {
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
                    gp.RuleFor(g => g.FileUrl)
                        .NotEmpty().WithMessage("Plattegrond-bestand is verplicht.");
                });

            RuleForEach(s => s.Zones).ChildRules(zone =>
            {
                zone.RuleFor(z => z.Code)
                    .NotEmpty().WithMessage("Elke zone moet een code hebben.")
                    .MaximumLength(10).WithMessage("Code mag maximaal 10 karakters lang zijn.");

                zone.RuleFor(z => z.Size)
                    .GreaterThan(0).WithMessage("Grootte van een zone moet groter zijn dan 0.");

                zone.RuleFor(z => z.TreeTypeId)
                    .GreaterThan(0).WithMessage("Elke zone moet een boomsoort hebben.");
            });
            RuleFor(s => s.UserId)
                .MustAsync(async (command, userId, cancellation) =>
                {
                    var existing = await _uow.NurserySiteRepository
                        .FindAsync(ns => ns.UserId == userId);

                    if (existing == null)
                        return true;

                    return existing.Id == command.NurserySite.Id;
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

    public class EditNurserySiteCommandHandler : IRequestHandler<EditNurserySiteCommand, Result<NurserySiteDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;
        private readonly IMediator mediator;

        public EditNurserySiteCommandHandler(IUnitofWork uow, IMapper mapper, IMediator mediator)
        {
            this.uow = uow;
            this.mapper = mapper;
            this.mediator = mediator;
        }

        public async Task<Result<NurserySiteDTO>> Handle(EditNurserySiteCommand request, CancellationToken cancellationToken)
        {
            var dto = request.NurserySite;

            var existingSite = await uow.NurserySiteRepository.GetByIdWithDetails(dto.Id);
            if (existingSite == null)
                return Result<NurserySiteDTO>.Failure($"Kweeksite met ID {dto.Id} niet gevonden.");

            UpdateBasicFields(dto, existingSite, request);
            UpdateAddress(existingSite, request);
            UpdateGroundPlan(existingSite, request);

            await uow.NurserySiteRepository.Update(existingSite);

            await AddNewZones(request, existingSite);
            await EditExistingZones(request);
            await DeleteRemovedZones(request);

            await uow.Commit();

            var updatedDto = mapper.Map<NurserySiteDTO>(existingSite);
            return Result<NurserySiteDTO>.Success(updatedDto, "Kweeksite succesvol bijgewerkt");
        }
        #region Helper

        private static void UpdateBasicFields(NurserySiteDTO dto, NurserySite existingSite, EditNurserySiteCommand request)
        {
            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != existingSite.Name)
                existingSite.Name = dto.Name;

            existingSite.UserId = request.UserId;
        }

        private void UpdateAddress(NurserySite existingSite, EditNurserySiteCommand request)
        {
            if (request.Address == null)
                return;

            if (existingSite.Address == null)
            {
                existingSite.Address = mapper.Map<Address>(request.Address);
                return;
            }

            if (!string.IsNullOrWhiteSpace(request.Address.StreetName))
                existingSite.Address.StreetName = request.Address.StreetName;

            if (!string.IsNullOrWhiteSpace(request.Address.HouseNumber))
                existingSite.Address.HouseNumber = request.Address.HouseNumber;

            if (!string.IsNullOrWhiteSpace(request.Address.PostalCode))
                existingSite.Address.PostalCode = request.Address.PostalCode;
        }

        private void UpdateGroundPlan(NurserySite existingSite, EditNurserySiteCommand request)
        {
            if (request.GroundPlan == null)
                return;

            if (existingSite.GroundPlan == null)
            {
                existingSite.GroundPlan = new GroundPlan
                {
                    FileUrl = request.GroundPlan.FileUrl,
                    UploadTime = request.GroundPlan.UploadTime == default
                        ? DateTime.UtcNow
                        : request.GroundPlan.UploadTime
                };
                return;
            }

            if (!string.IsNullOrWhiteSpace(request.GroundPlan.FileUrl))
            {
                existingSite.GroundPlan.FileUrl = request.GroundPlan.FileUrl;
                existingSite.GroundPlan.UploadTime = request.GroundPlan.UploadTime == default
                    ? DateTime.UtcNow
                    : request.GroundPlan.UploadTime;
            }
        }

        private async Task AddNewZones(EditNurserySiteCommand request, NurserySite existingSite)
        {
            foreach (var zoneDTO in request.Zones.Where(z => z.Id == 0))
            {
                zoneDTO.NurserySiteId = existingSite.Id;
                await mediator.Send(new AddZoneCommand { Zone = zoneDTO });
            }
        }

        private async Task EditExistingZones(EditNurserySiteCommand request)
        {
            foreach (var zoneDTO in request.Zones.Where(z => z.Id > 0))
            {
                await mediator.Send(new EditZoneCommand { Zone = zoneDTO });
            }
        }

        private async Task DeleteRemovedZones(EditNurserySiteCommand request)
        {
            foreach (var zone in request.Zones.Where(z => z.IsDeleted))
            {
                var zoneDTO = mapper.Map<ZoneDTO>(zone);
                await mediator.Send(new DeleteZoneCommand { Zone = zoneDTO });
            }
        }


        #endregion
    }

    public class DeleteNurserySiteCommand : IRequest<Result<NurserySiteDTO>>
    {
        public int Id { get; set; }
    }

    public class DeleteNurserySiteCommandHandler : IRequestHandler<DeleteNurserySiteCommand, Result<NurserySiteDTO>>
    {
        private readonly IUnitofWork uow;

        public DeleteNurserySiteCommandHandler(IUnitofWork uow)
        {
            this.uow = uow;
        }

        public async Task<Result<NurserySiteDTO>> Handle(DeleteNurserySiteCommand request, CancellationToken cancellationToken)
        {
            var existing = await uow.NurserySiteRepository.GetByIdWithDetails(request.Id);
            if (existing == null)
                return Result<NurserySiteDTO>.Failure("Kweeksite niet gevonden.");

            if (!existing.IsArchived)
                return Result<NurserySiteDTO>.Failure("Verwijderen kan alleen voor gearchiveerde kweeksites.");

            if (existing.Zones != null && existing.Zones.Any())
            {
                var zoneIds = existing.Zones.Select(z => z.Id).ToList();
                var taskLists = await uow.TaskListRepository.GetByZoneIdsWithTasks(zoneIds);
                if (taskLists.Any())
                    return Result<NurserySiteDTO>.Failure("Verwijderen niet mogelijk: zones van deze kweeksite worden gebruikt in takenlijsten.");

                foreach (var zone in existing.Zones.ToList())
                    await uow.ZoneRepository.Delete(zone);
            }

            await uow.NurserySiteRepository.Delete(existing);
            await uow.Commit();

            return Result<NurserySiteDTO>.Success(new NurserySiteDTO { Id = existing.Id }, "Kweeksite verwijderd.");
        }
    }
}
