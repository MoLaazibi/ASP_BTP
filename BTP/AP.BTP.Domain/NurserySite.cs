namespace AP.BTP.Domain
{
    public class NurserySite
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AddressId { get; set; }
        public int GroundPlanId { get; set; }
        public int UserId { get; set; }
        public bool IsArchived { get; set; }
        public GroundPlan GroundPlan { get; set; }
        public Address Address { get; set; }
        public User User { get; set; }
        public ICollection<Zone> Zones { get; set; }
    }
}
