using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.TreeTypes
{
    public class GetAllTreeTypesByArchiveStatusQuery : IRequest<IEnumerable<TreeTypeDTO>>
    {
        public int PageNr { get; set; }
        public int PageSize { get; set; }
        public bool IsArchived { get; set; }
    }

    public class GetAllTreeTypesByArchiveStatusQueryHandler
        : IRequestHandler<GetAllTreeTypesByArchiveStatusQuery, IEnumerable<TreeTypeDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;

        public GetAllTreeTypesByArchiveStatusQueryHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<TreeTypeDTO>> Handle(GetAllTreeTypesByArchiveStatusQuery request, CancellationToken cancellationToken)
        {
            var treeTypes = await uow.TreeTypeRepository
                .GetByArchiveStatus(request.IsArchived, request.PageNr, request.PageSize);

            return mapper.Map<IEnumerable<TreeTypeDTO>>(treeTypes);
        }
    }
}
