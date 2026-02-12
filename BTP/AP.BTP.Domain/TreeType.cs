namespace AP.BTP.Domain
{
    public class TreeType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsArchived { get; set; }
        public string? QrCodeUrl { get; set; }
        public ICollection<TreeImage> TreeImages { get; set; }
        public ICollection<Instruction> Instructions { get; set; }
        public ICollection<Zone> Zones { get; set; }
    }
}
