using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("claim_items")]
    public class ClaimItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("claim_id")]
        public int ClaimId { get; set; }

        [Column("from_place")]
        [StringLength(255)]
        public string? FromPlace { get; set; }

        [Column("to_place")]
        [StringLength(255)]
        public string? ToPlace { get; set; }

        [Column("mode_of_travel")]
        [StringLength(100)]
        public string? ModeOfTravel { get; set; }

        [Column("expense_type")]
        [StringLength(255)]
        public string? ExpenseType { get; set; }

        [Column("amount")]
        public decimal? Amount { get; set; }

        [Column("actual_km")]
        public decimal? ActualKm { get; set; }

        [Column("comments")]
        [StringLength(255)]
        public string? Comments { get; set; }

        [Column("bill_url")]
        [StringLength(255)]
        public string? BillUrl { get; set; }

        // Navigation back to Claim
        public Claim? Claim { get; set; }
    }
}
