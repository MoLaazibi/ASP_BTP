using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.TreeTypes
{
    public class GetAllTreeTypesQuery : IRequest<IEnumerable<TreeTypeDTO>>
    {
        public int PageNr { get; set; }
        public int PageSize { get; set; }
    }

    public class GetAllTreeTypesQueryHandler : IRequestHandler<GetAllTreeTypesQuery, IEnumerable<TreeTypeDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;

        public GetAllTreeTypesQueryHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<TreeTypeDTO>> Handle(GetAllTreeTypesQuery request, CancellationToken cancellationToken)
        {
            var treeTypes = await uow.TreeTypeRepository.GetAll(request.PageNr, request.PageSize);
            return mapper.Map<IEnumerable<TreeTypeDTO>>(treeTypes);
        }
    }
}

