using AP.BTP.Application.Interfaces;
using MediatR;

namespace AP.BTP.Application.CQRS.Instructions
{
    public record GetCurrentInstructionForTreeTypeQuery(int TreeTypeId) : IRequest<Result<InstructionDTO>>;

    public class GetCurrentInstructionForTreeTypeQueryHandler : IRequestHandler<GetCurrentInstructionForTreeTypeQuery, Result<InstructionDTO>>
    {
        private readonly IUnitofWork uow;
        public GetCurrentInstructionForTreeTypeQueryHandler(IUnitofWork uow)
        {
            this.uow = uow;
        }

        public async Task<Result<InstructionDTO>> Handle(GetCurrentInstructionForTreeTypeQuery request, CancellationToken cancellationToken)
        {
            var month = DateTime.UtcNow.Month;
            var season = month switch
            {
                >= 3 and <= 5 => "Lente",
                >= 6 and <= 8 => "Zomer",
                >= 9 and <= 11 => "Herfst",
                _ => "Winter"
            };

            var instr = await uow.InstructionRepository.GetLatestByTreeTypeAndSeason(request.TreeTypeId, season);
            if (instr == null)
            {
                return Result<InstructionDTO>.Failure("Geen instructie voor huidig seizoen.");
            }

            var dto = new InstructionDTO
            {
                Id = instr.Id,
                Season = instr.Season,
                FileUrl = instr.FileUrl,
                UploadTime = instr.UploadTime
            };

            return Result<InstructionDTO>.Success(dto, "Huidige seizoen instructie");
        }
    }
}

