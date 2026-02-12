using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.Categories
{
    public class AddCategoryCommand : IRequest<Result<CategoryDTO>>
    {
        public required string Name { get; set; }
        public required string Color { get; set; }
    }

    public class AddCategoryCommandValidator : AbstractValidator<AddCategoryCommand>
    {
        public AddCategoryCommandValidator(IUnitofWork uow)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(50);

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("Color is required.")
                .Matches("^#[0-9A-Fa-f]{6}$")
                .WithMessage("Color must be a valid hex color in the format #xxxxxx.");
        }
    }

	public class AddCategoryCommandHandler : IRequestHandler<AddCategoryCommand, Result<CategoryDTO>>
	{
		private readonly IUnitofWork uow;
		private readonly IMapper mapper;

		public AddCategoryCommandHandler(IUnitofWork uow, IMapper mapper)
		{
			this.uow = uow;
			this.mapper = mapper;
		}

		public async Task<Result<CategoryDTO>> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
		{
			var newCategory = new Category
			{
				Name = request.Name,
				Color = request.Color,
			};

			await uow.CategoryRepository.Create(newCategory);
			await uow.Commit();
			var dto = mapper.Map<CategoryDTO>(newCategory);
			return Result<CategoryDTO>.Success(dto, "Category successfully created");
		}
	}
}
