namespace AP.BTP.Application.CQRS.TreeImages
{
    public class TreeImageDTO
    {
        public int Id { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public DateTime UploadTime { get; set; }
    }
}

