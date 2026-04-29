using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_deals")]
    public class SalesDeal : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int? Id { get; set; }

        [Column("deal_name")]
        [MaxLength(255)]
        public string? DealName { get; set; }

        [Column("amount")] 
        public double? Amount { get; set; }

        [Column("expected_revenue")]
        public double? ExpectedRevenue { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("deal_for")]
        [MaxLength(255)]
        public string? DealFor { get; set; }

        [Column("close_date")]
        public DateTime? CloseDate { get; set; }

        [Column("status")]
        [MaxLength(255)]
        public string? Status { get; set; }

        [Column("isactive")]
        public bool? IsActive { get; set; }

        [Column("comments")]
        public string? Comments { get; set; }

        [Column("opportunity_id")]
        public int? OpportunityId { get; set; }

        [Column("customer_id")]
        public int? CustomerId { get; set; }

        [Column("sales_representative_id")]
        public int? SalesRepresentativeId { get; set; }

        // Navigation properties for foreign keys
        [ForeignKey("OpportunityId")]
        public virtual SalesOpportunity? Opportunity { get; set; }

        [ForeignKey("CustomerId")]
        public virtual SalesCustomer? Customer { get; set; }

        [ForeignKey("SalesRepresentativeId")]
        public virtual SalesEmployee? SalesRepresentative { get; set; }

        [ForeignKey("UserCreated")]
        public virtual SalesEmployee? UserCreatedNavigation { get; set; }

        [ForeignKey("UserUpdated")]
        public virtual SalesEmployee? UserUpdatedNavigation { get; set; }
    }

    public class SalesDealDetails : SalesDeal
    {
        public string? OpportunityName { get; set; }
        public string? CustomerName { get; set; }
        public string? SalesRepresentativeName { get; set; }
    }
}