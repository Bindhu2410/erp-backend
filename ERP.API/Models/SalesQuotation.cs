using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_quotations")]
    public class SalesQuotation
    {
        [MaxLength(200)]
    [Column("contact_name")]
    public string? ContactName { get; set; }

    [MaxLength(100)]
    [Column("contact_mobile_no")]
    public string? ContactMobileNo { get; set; }
    
        [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }

    public int? UserCreated { get; set; }
    public DateTime? DateCreated { get; set; }
    public int? UserUpdated { get; set; }
    public DateTime? DateUpdated { get; set; }

    [MaxLength(255)]
    public string? Version { get; set; }

    public string? Terms { get; set; }

    public DateTime? ValidTill { get; set; }

    public string? QuotationFor { get; set; }

    [MaxLength(255)]
    public string? Status { get; set; }

    [MaxLength(255)]
    public string? LostReason { get; set; }

    public int CustomerId { get; set; }

    [MaxLength(255)]
    [Required(ErrorMessage = "CustomerName is required")]
    public string? CustomerName { get; set; }

    [MaxLength(255)]
    public string? QuotationType { get; set; }

    [Required(ErrorMessage = "QuotationDate is required")]
    public DateTime? QuotationDate { get; set; }

    public string? OpportunityId { get; set; }

    [MaxLength(255)]
    [Required(ErrorMessage = "OrderType is required")]
    public string? OrderType { get; set; }

    public string? Comments { get; set; }

    public string? DeliveryWithin { get; set; }

    public string? DeliveryAfter { get; set; }

    [Column("is_active")]
    [Required]
    public bool IsActive { get; set; }

    [MaxLength(255)]
    public string? QuotationId { get; set; }

    public string? LeadId { get; set; }

    public int Taxes { get; set; }

    public string? Delivery { get; set; }

    public string? Payment { get; set; }

    public string? Warranty { get; set; }

    /// <summary>
    /// Freight charge for the quotation. Maps to 'freight_charge' column in DB.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.Column("freight_charge")]
    public decimal? FreightCharge { get; set; }

    public bool? IsCurrent { get; set; }
    public int Discount { get; set; }

    [Column("expire_date")]
    public DateTime? ExpireDate { get; set; }

    [MaxLength(255)]
    public string? Territory { get; set; }

    [Column("expected_completion")]
    public DateTime? ExpectedCompletion { get; set; }

    [Column("delivery_prepare_after")]
    [MaxLength(255)]
    public string? DeliveryPrepareAfter { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(255)]
    public string? Contact { get; set; }

    [MaxLength(255)]
    public string? SelectedTaxes { get; set; }

    [MaxLength(255)]
    public string? SelectedFreightCharges { get; set; }

    public string? SelectedDelivery { get; set; }

    public string? SelectedPayment { get; set; }

    public string? SelectedWarranty { get; set; }

    public bool? LatestQuotation { get; set; }

    public bool? Published { get; set; }

    [MaxLength(255)]
    public string? CurrentModifierName { get; set; }

    // Navigation Properties
    [ForeignKey("UserCreated")]
    public virtual User? Creator { get; set; }

    [ForeignKey("UserUpdated")]
    public virtual User? Updater { get; set; }

    [ForeignKey("OpportunityId")]
    public virtual SalesOpportunity? Opportunity { get; set; }

    [ForeignKey("CustomerId")]
    public virtual SalesCustomer? Customer { get; set; }

    [ForeignKey("ParentSalesQuotationsId")]
    public virtual SalesQuotation? ParentQuotation { get; set; }

    [ForeignKey("LeadId")]
    public virtual SalesLead? Lead { get; set; }

    // Foreign key navigation properties
    [ForeignKey("SalesLead")]
    public int? SalesLeadsId { get; set; }

    [ForeignKey("SalesOpportunity")]
    public int? SalesOpportunitiesId { get; set; }

    [ForeignKey("SalesContact")]
    public int? SalesContactsId { get; set; }

    [ForeignKey("SalesAddress")]
    public int? SalesAddressesId { get; set; }

    [ForeignKey("SalesRepresentative")]
    public int? SalesRepresentativesId { get; set; }

    [ForeignKey("ParentQuotation")]
    public int? ParentSalesQuotationsId { get; set; }

    [ForeignKey("CopyFromQuotation")]
    public int? CopyFromSalesQuotationsId { get; set; }

    [ForeignKey("CurrentModifierEmployee")]
    public int? CurrentModifierEmployeesId { get; set; }
}
}
