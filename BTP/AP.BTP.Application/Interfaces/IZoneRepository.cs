using AP.BTP.Domain;

namespace AP.BTP.Application.Interfaces
{
    public interface IZoneRepository : IGenericRepository<Zone>
    {
        Task<Zone?> GetByIdWithNurserySite(int zoneId);

    }
}
