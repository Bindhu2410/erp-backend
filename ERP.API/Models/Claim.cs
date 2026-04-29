using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("claims")]
    public class Claim
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_created")]
        public int? UserCreated { get; set; }

        [Column("date_created")]
        public DateTime? DateCreated { get; set; }

        [Column("user_updated")]
        public int? UserUpdated { get; set; }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }

        [Required]
        [Column("claim_no")]
        [StringLength(100)]
        public string ClaimNo { get; set; }

        [Required]
        [Column("claim_date")]
        public DateTime ClaimDate { get; set; }

        [Column("user_name")]
        [StringLength(255)]
        public string? UserName { get; set; }

        [Column("claim_type")]
        [StringLength(255)]
        public string? ClaimType { get; set; }

        [Column("mode_of_travel")]
        [StringLength(100)]
        public string? ModeOfTravel { get; set; }

        // NOTE: moved per-item properties (from_place, to_place, expense_type, amount, actual_km, comments, bill_url)
        // into a new ClaimItem entity to support multiple item rows per claim.
        public ICollection<ClaimItem>? Items { get; set; }
    }
}