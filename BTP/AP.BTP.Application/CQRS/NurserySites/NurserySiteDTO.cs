namespace AP.BTP.Application.CQRS.NurserySites
{
    public class NurserySiteDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AddressId { get; set; }
        public int UserId { get; set; }
        public bool IsArchived { get; set; }
        public GroundPlanDTO GroundPlan { get; set; }
        public AddressDTO Address { get; set; }
        public ICollection<ZoneDTO> Zones { get; set; }
    }
}
