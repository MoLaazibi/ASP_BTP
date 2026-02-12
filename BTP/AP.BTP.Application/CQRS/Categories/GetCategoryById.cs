using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.Categories
{
    public class GetCategoryByIdQuery : IRequest<CategoryDTO>
    {
        public int Id { get; set; }
    }

    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDTO>
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IMapper _mapper;

        public GetCategoryByIdQueryHandler(IUnitofWork unitofWork, IMapper mapper)
        {
            _unitofWork = unitofWork;
            _mapper = mapper;
        }

        public async Task<CategoryDTO> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var nurserySite = await _unitofWork.CategoryRepository.GetById(request.Id);

            if (nurserySite == null)
                return null;

            return _mapper.Map<CategoryDTO>(nurserySite);
        }
    }
}
