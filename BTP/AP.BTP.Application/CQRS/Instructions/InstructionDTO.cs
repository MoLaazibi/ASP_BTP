namespace AP.BTP.Application.CQRS.Instructions
{
    public class InstructionDTO
    {
        public int Id { get; set; }
        public string Season { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime UploadTime { get; set; }
    }
}

