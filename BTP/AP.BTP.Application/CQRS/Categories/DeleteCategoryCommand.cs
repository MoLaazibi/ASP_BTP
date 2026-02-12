using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.Categories
{
    public class DeleteCategoryCommand : IRequest<CategoryDTO>
    {
        public int Id { get; set; }
    }
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, CategoryDTO>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;
        public DeleteCategoryCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        public async Task<CategoryDTO> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var excistingCategory = await uow.CategoryRepository.GetById(request.Id);

			if (excistingCategory == null)
				throw new InvalidOperationException($"Categorie met ID {request.Id} niet gevonden.");

			await uow.CategoryRepository.Delete(excistingCategory);
            await uow.Commit();

            return mapper.Map<CategoryDTO>(excistingCategory);
        }
    }

}
