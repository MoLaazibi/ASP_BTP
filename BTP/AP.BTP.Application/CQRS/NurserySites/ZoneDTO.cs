namespace AP.BTP.Application.CQRS.NurserySites
{
    public class ZoneDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal Size { get; set; }
        public int NurserySiteId { get; set; }
        public int TreeTypeId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
