using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.Users
{
    public class GetByEmailQuery : IRequest<UserDTO>
    {
        public string Email { get; set; }
    }
    public class GetByEmailQueryHandler : IRequestHandler<GetByEmailQuery, UserDTO>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;

        public GetByEmailQueryHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        public async Task<UserDTO> Handle(GetByEmailQuery request, CancellationToken cancellationToken)
        {
            var user = await uow.UserRepository.GetByEmail(request.Email);
            return mapper.Map<UserDTO>(user);
        }
    }

}