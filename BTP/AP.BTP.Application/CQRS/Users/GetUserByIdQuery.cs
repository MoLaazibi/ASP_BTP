using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.Users
{
    public class GetUserByIdQuery : IRequest<UserDTO>
    {
        public int Id { get; set; }
    }

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDTO>
    {
        private readonly IUnitofWork _uow;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(IUnitofWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<UserDTO> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _uow.UserRepository.GetById(request.Id);
            return _mapper.Map<UserDTO>(user);
        }
    }
}
