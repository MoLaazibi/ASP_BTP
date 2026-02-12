using AP.BTP.Application.Interfaces;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.Categories
{
    public class EditCategoryCommand : IRequest<Result<CategoryDTO>>
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Color { get; set; }
    }

    public class EditCategoryCommandValidator : AbstractValidator<EditCategoryCommand>
    {
        public EditCategoryCommandValidator()
        {
            RuleFor(c => c.Id)
                .GreaterThan(0)
                .WithMessage("Id moet groter zijn dan 0.");

            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(50);

            RuleFor(c => c.Color)
                .NotEmpty().WithMessage("Color is required.")
                .Matches("^#[0-9A-Fa-f]{6}$")
                .WithMessage("Color must be a valid hex color in the format #xxxxxx.");
        }
    }

    public class EditCategoryCommandHandler : IRequestHandler<EditCategoryCommand, Result<CategoryDTO>>
    {
        private readonly IUnitofWork _uow;
        private readonly IMapper _mapper;

        public EditCategoryCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<CategoryDTO>> Handle(EditCategoryCommand request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var name = request.Name;
            var color = request.Color;

            var existing = await _uow.CategoryRepository.GetById(id);
            if (existing == null)
                return Result<CategoryDTO>.Failure($"Taak met ID {id} niet gevonden.");

            existing.Name = name;
            existing.Color = color;

            await _uow.CategoryRepository.Update(existing);
            await _uow.Commit();

            var updatedDto = _mapper.Map<CategoryDTO>(existing);
            return Result<CategoryDTO>.Success(updatedDto, "Taak succesvol bijgewerkt.");
        }
    }
}
