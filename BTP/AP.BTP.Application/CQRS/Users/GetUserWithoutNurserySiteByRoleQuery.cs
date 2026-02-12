using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.Users
{
    public class GetUserWithoutNurserySiteByRoleQuery : IRequest<List<UserDTO>>
    {
        public int PageNr { get; set; }
        public int PageSize { get; set; }
    }

    public class GetAllUserWithoutNurserySiteQueryHandler : IRequestHandler<GetUserWithoutNurserySiteByRoleQuery, List<UserDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;

        public GetAllUserWithoutNurserySiteQueryHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }
        public async Task<List<UserDTO>> Handle(GetUserWithoutNurserySiteByRoleQuery request, CancellationToken cancellationToken)
        {
            return mapper.Map<List<UserDTO>>(await uow.UserRepository.GetWithoutNurserySiteByRole(request.PageNr, request.PageSize));
        }
    }
}
