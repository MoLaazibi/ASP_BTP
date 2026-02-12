namespace AP.BTP.Domain
{
    public class Instruction
    {
        public int Id { get; set; }
        public int TreeTypeId { get; set; }
        public TreeType TreeType { get; set; }
        public string Season { get; set; }
        public string FileUrl { get; set; }
        public DateTime UploadTime { get; set; }
    }
}

