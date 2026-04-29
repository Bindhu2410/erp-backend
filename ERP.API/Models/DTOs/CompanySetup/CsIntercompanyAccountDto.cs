namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsIntercompanyAccountDto
    {
        public int IntercompanyAccountId { get; set; }
        public int RelationshipId { get; set; }
        public string TransactionType { get; set; }
        public int Company1ReceivableAccountId { get; set; }
        public int Company2PayableAccountId { get; set; }
        public string? Company1TaxTreatmentRule { get; set; }
        public string? Company2TaxTreatmentRule { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CsIntercompanyAccountSearchDto
    {
        public int RelationshipId { get; set; }
        public string? SearchText { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }
}
