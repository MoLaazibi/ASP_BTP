using AP.BTP.Application.Interfaces;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.NurserySites
{
    public class EditNurserySiteArchivedCommand : IRequest<Result<NurserySiteDTO>>
    {
        public int Id { get; set; }
        public bool IsArchived { get; set; }
    }

    public class EditNurserySiteArchivedCommandValidator : AbstractValidator<EditNurserySiteArchivedCommand>
    {
        private readonly IUnitofWork _uow;

        public EditNurserySiteArchivedCommandValidator(IUnitofWork uow)
        {
            _uow = uow;

            RuleFor(c => c.Id)
                .GreaterThan(0)
                .WithMessage("Id moet groter zijn dan 0.")
                .MustAsync(async (id, ct) =>
                {
                    var item = await _uow.NurserySiteRepository.GetById(id);
                    return item != null;
                })
                .WithMessage("Kweeksite bestaat niet.");
        }
    }

    public class EditNurserySiteArchivedCommandHandler
        : IRequestHandler<EditNurserySiteArchivedCommand, Result<NurserySiteDTO>>
    {
        private readonly IUnitofWork _uow;
        private readonly IMapper _mapper;

        public EditNurserySiteArchivedCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<NurserySiteDTO>> Handle(EditNurserySiteArchivedCommand request, CancellationToken cancellationToken)
        {
            var existing = await _uow.NurserySiteRepository.GetByIdWithDetails(request.Id);
            if (existing == null)
                return Result<NurserySiteDTO>.Failure("Kweeksite niet gevonden.");

            if (request.IsArchived && existing.Zones != null && existing.Zones.Any())
            {
                var zoneIds = existing.Zones.Select(z => z.Id).ToList();
                if (zoneIds.Any())
                {
                    var taskLists = await _uow.TaskListRepository.GetByZoneIdsWithTasks(zoneIds);
                    var hasOpenTasks = taskLists.Any(tl =>
                        tl.Tasks != null &&
                        tl.Tasks.Any(t => t.StopTime == null));

                    if (hasOpenTasks)
                        return Result<NurserySiteDTO>.Failure("Deze kweeksite heeft zones die in huidige taken worden gebruikt en kan niet worden gearchiveerd.");
                }
            }

            existing.IsArchived = request.IsArchived;

            await _uow.NurserySiteRepository.Update(existing);
            await _uow.Commit();

            var dto = _mapper.Map<NurserySiteDTO>(existing);
            return Result<NurserySiteDTO>.Success(dto, "Archivering bijgewerkt.");
        }
    }
}
