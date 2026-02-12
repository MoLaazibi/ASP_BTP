using AP.BTP.Application.Interfaces;
using AP.BTP.Domain;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.Users
{
    public class GetUserByRoleQuery : IRequest<IEnumerable<UserDTO>>
    {
        public Role Role { get; set; }
        public int PageNr { get; set; }
        public int PageSize { get; set; }
    }
    public class GetUserByRoleQueryHandler : IRequestHandler<GetUserByRoleQuery, IEnumerable<UserDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;

        public GetUserByRoleQueryHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }
        public async Task<IEnumerable<UserDTO>> Handle(GetUserByRoleQuery request, CancellationToken cancellationToken)
        {
            var users = await uow.UserRepository.GetByRole(request.Role, request.PageNr, request.PageSize);
            var dto = mapper.Map<IEnumerable<UserDTO>>(users);
            return dto;
        }
    }
}
