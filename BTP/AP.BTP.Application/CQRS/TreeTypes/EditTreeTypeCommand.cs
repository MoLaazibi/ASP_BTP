using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.TreeTypes
{
    public class EditTreeTypeCommand : IRequest<Result<TreeTypeDTO>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public List<InstructionInput> Instructions { get; set; } = new();

        public class InstructionInput
        {
            public string Season { get; set; } = string.Empty;
            public string FileUrl { get; set; } = string.Empty;
        }
    }

    public class EditTreeTypeCommandValidator : AbstractValidator<EditTreeTypeCommand>
    {
        public EditTreeTypeCommandValidator(IUnitofWork uow)
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Naam is verplicht")
                .MaximumLength(50).WithMessage("Naam mag maximaal 50 tekens bevatten")
                .MustAsync(async (cmd, name, ct) =>
                {
                    var existing = await uow.TreeTypeRepository.GetByName(name);
                    return existing == null || existing.Id == cmd.Id;
                })
                .WithMessage("Deze boomsoort bestaat al");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleForEach(x => x.Instructions)
                .ChildRules(instr =>
                {
                    instr.RuleFor(i => i.Season).NotEmpty().WithMessage("Season is verplicht");
                    instr.RuleFor(i => i.FileUrl).NotEmpty().WithMessage("File URL is verplicht");
                });
        }
    }

    public class EditTreeTypeCommandHandler : IRequestHandler<EditTreeTypeCommand, Result<TreeTypeDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;

        public EditTreeTypeCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        public async Task<Result<TreeTypeDTO>> Handle(EditTreeTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = await uow.TreeTypeRepository.GetByIdWithIncludes(request.Id);
            if (entity == null)
                return Result<TreeTypeDTO>.Failure("Boomsoort niet gevonden.");

            entity.Name = request.Name;
            entity.Description = request.Description ?? string.Empty;

            entity.TreeImages = request.ImageUrls
                .Select(url => new TreeImage { FileUrl = url, UploadTime = System.DateTime.UtcNow })
                .ToList();

            entity.Instructions = request.Instructions
                .Select(i => new Instruction { Season = i.Season, FileUrl = i.FileUrl, UploadTime = System.DateTime.UtcNow })
                .ToList();

            await uow.TreeTypeRepository.Update(entity);
            await uow.Commit();

            var dto = mapper.Map<TreeTypeDTO>(entity);
            return Result<TreeTypeDTO>.Success(dto, "Boomsoort succesvol bijgewerkt");
        }
    }

    public class DeleteTreeTypeCommand : IRequest<Result<TreeTypeDTO>>
    {
        public int Id { get; set; }
    }

    public class DeleteTreeTypeCommandHandler : IRequestHandler<DeleteTreeTypeCommand, Result<TreeTypeDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;

        public DeleteTreeTypeCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        public async Task<Result<TreeTypeDTO>> Handle(DeleteTreeTypeCommand request, CancellationToken cancellationToken)
        {
            var existing = await uow.TreeTypeRepository.GetByIdWithIncludes(request.Id);
            if (existing == null)
                return Result<TreeTypeDTO>.Failure("Boomsoort niet gevonden.");

            if (!existing.IsArchived)
                return Result<TreeTypeDTO>.Failure("Verwijderen kan alleen voor gearchiveerde boomsoorten.");

            var zoneUsingTree = await uow.ZoneRepository.FindAsync(z => z.TreeTypeId == request.Id);
            if (zoneUsingTree != null)
                return Result<TreeTypeDTO>.Failure("Verwijderen niet mogelijk: boomsoort is gekoppeld aan een zone.");

            await uow.TreeTypeRepository.Delete(existing);
            await uow.Commit();

            var dto = mapper.Map<TreeTypeDTO>(existing);
            return Result<TreeTypeDTO>.Success(dto, "Boomsoort verwijderd.");
        }
    }
}
