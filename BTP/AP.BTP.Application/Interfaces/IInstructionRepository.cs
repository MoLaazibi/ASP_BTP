using AP.BTP.Domain;

namespace AP.BTP.Application.Interfaces
{
    public interface IInstructionRepository : IGenericRepository<Instruction>
    {
        Task<Instruction?> GetLatestByTreeTypeAndSeason(int treeTypeId, string season);
    }
}
