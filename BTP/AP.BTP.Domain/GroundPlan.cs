namespace AP.BTP.Domain
{
    public class GroundPlan
    {
        public int Id { get; set; }
        public string FileUrl { get; set; }
        public DateTime UploadTime { get; set; }
        public NurserySite NurserySite { get; set; }
    }
}
