using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.Users
{

    public class GetUserByAuthIdQuery : IRequest<UserDTO>
    {
        public string AuthId { get; set; }
    }

    public class GetUserByAuthIdQueryHandler : IRequestHandler<GetUserByAuthIdQuery, UserDTO>
    {
        private readonly IUnitofWork _uow;
        private readonly IMapper _mapper;

        public GetUserByAuthIdQueryHandler(IUnitofWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<UserDTO> Handle(GetUserByAuthIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _uow.UserRepository.GetByAuthId(request.AuthId);
            return _mapper.Map<UserDTO>(user);
        }
    }
}