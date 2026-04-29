namespace ERP.API.Models.DTOs
{
    public class ClaimItemDto
    {
        public string? FromPlace { get; set; }
        public string? ToPlace { get; set; }
        public string? ModeOfTravel { get; set; }
        public string? ExpenseType { get; set; }
        public decimal? Amount { get; set; }
        public decimal? ActualKm { get; set; }
        public string? Comments { get; set; }
        public string? BillUrl { get; set; }
    }
}
