namespace ERP.API.Models.DTOs
{
    public class QuotationGridDto
    {
        public int Id { get; set; }
        public string QuotationId { get; set; }
        public string CustomerName { get; set; }
        public DateTime DateCreated { get; set; }
        public string Status { get; set; }
        // Add other fields as needed
    }

    public class QuotationGridSearchRequest
    {
        // Add filter properties as needed
    }
}
