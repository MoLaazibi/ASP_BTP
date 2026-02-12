using AP.BTP.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace AP.BTP.Application.CQRS.Categories
{
    public class ArchiveCategoryCommand : IRequest<Result<CategoryDTO>>
    {
        public int CategoryId { get; set; }
    }
    public class ArchiveCategoryCommandValidator : AbstractValidator<ArchiveCategoryCommand>
    {
        public ArchiveCategoryCommandValidator(IUnitofWork uow)
        {
            RuleFor(c => c.CategoryId)
                .GreaterThan(0)
                .WithMessage("CategoryId moet groter zijn dan 0.");

            RuleFor(c => c.CategoryId)
                .MustAsync(async (id, ct) =>
                {
                    var category = await uow.CategoryRepository.GetById(id);
                    return category != null;
                })
                .WithMessage(c => $"Categorie met ID {c.CategoryId} bestaat niet.");
        }
    }

    public class ArchiveCategoryCommandHandler : IRequestHandler<ArchiveCategoryCommand, Result<CategoryDTO>>
    {
        private readonly IUnitofWork uow;

        public ArchiveCategoryCommandHandler(IUnitofWork uow)
        {
            this.uow = uow;
        }

        public async Task<Result<CategoryDTO>> Handle(ArchiveCategoryCommand request, CancellationToken cancellationToken)
        {
            var existingCategory = await uow.CategoryRepository.GetById(request.CategoryId);

            existingCategory.IsArchived = !existingCategory.IsArchived;

            await uow.CategoryRepository.Update(existingCategory);
            await uow.Commit();

            return Result<CategoryDTO>.Success(new CategoryDTO { Id = existingCategory.Id }, "Categorie gearchiveerd.");
        }
    }
}
