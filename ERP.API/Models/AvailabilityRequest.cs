namespace ERP.API.Models
{
    public class AvailabilityRequest
    {
        public int ItemId { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int? ToDay { get; set; }
        public int? ToMonth { get; set; }
        public int? ToYear { get; set; }
    }
}