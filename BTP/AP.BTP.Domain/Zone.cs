namespace AP.BTP.Domain
{
    public class Zone
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal Size { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int NurserySiteId { get; set; }
        public int TreeTypeId { get; set; }
        public NurserySite NurserySite { get; set; }
        public TreeType TreeType { get; set; }
        public ICollection<TaskList>? TaskLists { get; set; }
    }
}
