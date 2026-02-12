using AP.BTP.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace AP.BTP.Application.CQRS.TreeTypes
{
    public class UpdateTreeTypeQrCodeCommand : IRequest<Result<TreeTypeDTO>>
    {
        public required int TreeTypeId { get; set; }
        public required string QrCodeUrl { get; set; }
    }

    public class UpdateTreeTypeQrCodeCommandHandler : IRequestHandler<UpdateTreeTypeQrCodeCommand, Result<TreeTypeDTO>>
    {
        private readonly IUnitofWork uow;
        private readonly IMapper mapper;

        public UpdateTreeTypeQrCodeCommandHandler(IUnitofWork uow, IMapper mapper)
        {
            this.uow = uow;
            this.mapper = mapper;
        }

        public async Task<Result<TreeTypeDTO>> Handle(UpdateTreeTypeQrCodeCommand request, CancellationToken cancellationToken)
        {
            var entity = await uow.TreeTypeRepository.GetById(request.TreeTypeId);

            entity.QrCodeUrl = request.QrCodeUrl;

            await uow.Commit();

            var dto = mapper.Map<TreeTypeDTO>(entity);
            return Result<TreeTypeDTO>.Success(dto, "QR code succesvol bijgewerkt");
        }
    }
}
