using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.Users
{
    public class GetAllUserQuery : IRequest<IEnumerable<UserDTO>>
    {
        public int PageNr { get; set; }
        public int PageSize { get; set; }
    }
    public class GetAllUserQueryHandler : IRequestHandler<GetAllUserQuery, IEnumerable<UserDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;

        public GetAllUserQueryHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }
        public async Task<IEnumerable<UserDTO>> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
        {
            return mapper.Map<IEnumerable<UserDTO>>(await uow.UserRepository.GetAll(request.PageNr, request.PageSize));
        }
    }
}