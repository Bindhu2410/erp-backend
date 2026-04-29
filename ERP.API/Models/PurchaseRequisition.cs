using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("purchase_requisitions", Schema = "public")]
    public class PurchaseRequisition
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("purchase_requisition_id")]
        public string PurchaseRequisitionId { get; set; }

        [MaxLength(250)]
        [Column("requester_name")]
        public string RequesterName { get; set; }

        [Required]
        [Column("description")]
        public string Description { get; set; }

        [Column("delivery_date")]
        public DateTime? DeliveryDate { get; set; }

        [Column("budget_amount")]
        public decimal? BudgetAmount { get; set; }

        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; }

        [Column("user_created")]
        public int? UserCreated { get; set; }

        [Column("user_updated")]
        public int? UserUpdated { get; set; }

        [Column("date_created")]
        public DateTime DateCreated { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        [Column("date_updated")]
        public DateTime DateUpdated { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; }

        [NotMapped]
        public string VendorName => Supplier?.VendorName;

        // Ensure UTC for all DateTime assignments
        public void SetDateCreated(DateTime date)
        {
            DateCreated = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }
        public void SetDateUpdated(DateTime date)
        {
            DateUpdated = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }
    }
}
