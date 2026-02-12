using AP.BTP.Application.Interfaces;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.TreeTypes
{
    public class EditTreeTypeArchivedCommand : IRequest<Result<TreeTypeDTO>>
    {
        public int Id { get; set; }
        public bool IsArchived { get; set; }
    }

    public class EditTreeTypeIsArchivedCommandValidator : AbstractValidator<EditTreeTypeArchivedCommand>
    {
        private readonly IUnitofWork _uow;

        public EditTreeTypeIsArchivedCommandValidator(IUnitofWork uow)
        {
            _uow = uow;

            RuleFor(c => c.Id)
                .GreaterThan(0)
                .WithMessage("Id moet groter zijn dan 0.")
                .MustAsync(async (id, cancellation) =>
                {
                    var item = await _uow.TreeTypeRepository.GetById(id);
                    return item != null;
                })
                .WithMessage("Boomtype bestaat niet.");
        }
    }

    public class EditTreeTypeIsArchivedCommandHandler
        : IRequestHandler<EditTreeTypeArchivedCommand, Result<TreeTypeDTO>>
    {
        private readonly IUnitofWork _uow;
        private readonly IMapper _mapper;

        public EditTreeTypeIsArchivedCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<TreeTypeDTO>> Handle(EditTreeTypeArchivedCommand request, CancellationToken cancellationToken)
        {
            var existing = await _uow.TreeTypeRepository.GetById(request.Id);
            if (existing == null)
                return Result<TreeTypeDTO>.Failure("Boomtype niet gevonden.");

            if (request.IsArchived)
            {
                var zoneUsingTree = await _uow.ZoneRepository.FindAsync(z => z.TreeTypeId == request.Id);
                if (zoneUsingTree != null)
                    return Result<TreeTypeDTO>.Failure("Kan boomtype niet archiveren omdat het gekoppeld is aan een zone.");
            }

            existing.IsArchived = request.IsArchived;

            await _uow.TreeTypeRepository.Update(existing);
            await _uow.Commit();

            var dto = _mapper.Map<TreeTypeDTO>(existing);
            return Result<TreeTypeDTO>.Success(dto, "Archivering bijgewerkt.");
        }
    }
}
