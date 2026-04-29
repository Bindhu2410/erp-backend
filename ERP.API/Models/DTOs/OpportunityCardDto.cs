namespace ERP.API.Models.DTOs
{
    public class OpportunityCardDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
    }
}
