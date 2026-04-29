namespace ERP.API.Models
{
    public class PurchaseOrderFullDetailsDto
    {
    public PurchaseOrderDto PurchaseOrder { get; set; }
    public InvoiceDto Invoice { get; set; }
    public QuotationDto Quotation { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; }
    public object TermsAndConditions { get; set; }
    }
}
