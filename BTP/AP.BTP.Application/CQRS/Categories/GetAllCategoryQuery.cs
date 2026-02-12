using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.Categories
{
    public class GetAllCategoryQuery : IRequest<IEnumerable<CategoryDTO>>
    {
        public int PageNr { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool? IsArchived { get; set; }
    }

    public class GetAllCategoryQueryHandler : IRequestHandler<GetAllCategoryQuery, IEnumerable<CategoryDTO>>
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IMapper _mapper;

        public GetAllCategoryQueryHandler(IUnitofWork unitofWork, IMapper mapper)
        {
            _unitofWork = unitofWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDTO>> Handle(GetAllCategoryQuery request, CancellationToken cancellationToken)
        {
            var categories = request.IsArchived.HasValue
                ? await _unitofWork.CategoryRepository.GetByArchiveStatus(request.IsArchived.Value, request.PageNr, request.PageSize)
                : await _unitofWork.CategoryRepository.GetAll(request.PageNr, request.PageSize);
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }
    }
}
