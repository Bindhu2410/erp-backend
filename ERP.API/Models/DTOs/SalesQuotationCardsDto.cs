namespace ERP.API.Models.DTOs
{
    public class SalesQuotationCardsDto
    {
        public long Draft { get; set; }
        public long Approved { get; set; }
        public long FinalQuotation { get; set; }
        public long Submitted { get; set; }
        public long Negotiation { get; set; }
        public long Cancelled { get; set; }
    }
}
