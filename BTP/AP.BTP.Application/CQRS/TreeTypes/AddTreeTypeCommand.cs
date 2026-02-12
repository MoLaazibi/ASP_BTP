using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.TreeTypes
{
    public class AddTreeTypeCommand : IRequest<Result<TreeTypeDTO>>
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public List<InstructionInput> Instructions { get; set; } = new();
        public string? QrCodeUrl { get; set; }

        public class InstructionInput
        {
            public string Season { get; set; } = string.Empty;
            public string FileUrl { get; set; } = string.Empty;
        }
    }

    public class AddTreeTypeCommandValidator : AbstractValidator<AddTreeTypeCommand>
    {
        public AddTreeTypeCommandValidator(IUnitofWork uow)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Naam is verplicht")
                .MaximumLength(50).WithMessage("Naam mag maximaal 50 tekens bevatten")
                .MustAsync(async (name, ct) => await uow.TreeTypeRepository.GetByName(name) == null)
                .WithMessage("Deze boomsoort bestaat al");

            RuleFor(x => x.Description)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleForEach(x => x.Instructions)
                .ChildRules(instr =>
                {
                    instr.RuleFor(i => i.Season).NotEmpty().WithMessage("Season is verplicht");
                    instr.RuleFor(i => i.FileUrl).NotEmpty().WithMessage("File URL is verplicht");
                });
        }
    }

    public class AddTreeTypeCommandHandler : IRequestHandler<AddTreeTypeCommand, Result<TreeTypeDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;

        public AddTreeTypeCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        public async Task<Result<TreeTypeDTO>> Handle(AddTreeTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = new TreeType
            {
                Name = request.Name,
                Description = request.Description ?? "",
                IsArchived = false,
                TreeImages = request.ImageUrls
                    .Select(url => new TreeImage { FileUrl = url, UploadTime = DateTime.UtcNow })
                    .ToList(),
                Instructions = request.Instructions
                    .Select(i => new Instruction { Season = i.Season, FileUrl = i.FileUrl, UploadTime = DateTime.UtcNow })
                    .ToList(),
                QrCodeUrl = request.QrCodeUrl
            };

            await uow.TreeTypeRepository.Create(entity);
            await uow.Commit();

            var dto = mapper.Map<TreeTypeDTO>(entity);
            return Result<TreeTypeDTO>.Success(dto, "Boomsoort succesvol toegevoegd");
        }
    }
}
