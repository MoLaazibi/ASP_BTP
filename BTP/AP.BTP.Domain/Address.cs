namespace AP.BTP.Domain
{
    public class Address
    {
        public int Id { get; set; }
        public string StreetName { get; set; }
        public string HouseNumber { get; set; }
        public string PostalCode { get; set; }
        public NurserySite NurserySite { get; set; }
    }
}
