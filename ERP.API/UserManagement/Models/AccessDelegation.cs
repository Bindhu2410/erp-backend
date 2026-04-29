using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.UserManagement.Models
{
    /// <summary>
    /// Access delegation entity representing delegation of user access rights
    /// </summary>
    [Table("AccessDelegations", Schema = "public")]
    public class AccessDelegation
    {
        /// <summary>
        /// Primary key for the access delegation
        /// </summary>
        [Key]
        [Column("DelegationId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DelegationId { get; set; }

        /// <summary>
        /// User ID who is delegating access
        /// </summary>
        [Column("FromUserId")]
        [Required]
        public int FromUserId { get; set; }

        /// <summary>
        /// User ID who is receiving the delegated access
        /// </summary>
        [Column("ToUserId")]
        [Required]
        public int ToUserId { get; set; }

        /// <summary>
        /// Start date and time of the delegation
        /// </summary>
        [Column("StartDate")]
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date and time of the delegation
        /// </summary>
        [Column("EndDate")]
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Reason for the delegation
        /// </summary>
        [Column("Reason")]
        [StringLength(1000)]
        public string? Reason { get; set; }

        /// <summary>
        /// Indicates if the delegation is currently active
        /// </summary>
        [Column("IsActive")]
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// User ID who created the delegation
        /// </summary>
        [Column("CreatedBy")]
        [Required]
        public int CreatedBy { get; set; }

        /// <summary>
        /// Date and time when the delegation was created
        /// </summary>
        [Column("DateCreated")]
        [Required]
        public DateTime DateCreated { get; set; }

        #region Computed Properties

        /// <summary>
        /// Indicates if the delegation is currently valid (active and within date range)
        /// </summary>
        [NotMapped]
        public bool IsCurrentlyValid => IsActive && 
                                       DateTime.UtcNow >= StartDate && 
                                       DateTime.UtcNow <= EndDate;

        /// <summary>
        /// Indicates if the delegation has expired
        /// </summary>
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow > EndDate;

        /// <summary>
        /// Indicates if the delegation is scheduled for future
        /// </summary>
        [NotMapped]
        public bool IsScheduledFuture => DateTime.UtcNow < StartDate;

        /// <summary>
        /// Gets the duration of the delegation in days
        /// </summary>
        [NotMapped]
        public int DurationInDays => (EndDate - StartDate).Days;

        /// <summary>
        /// Gets the remaining days until delegation expires
        /// </summary>
        [NotMapped]
        public int RemainingDays => IsExpired ? 0 : Math.Max(0, (EndDate - DateTime.UtcNow).Days);

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates if the delegation dates are valid
        /// </summary>
        /// <returns>True if dates are valid, false otherwise</returns>
        public bool IsValidDateRange()
        {
            return StartDate < EndDate;
        }

        /// <summary>
        /// Validates if the delegation is not self-referential
        /// </summary>
        /// <returns>True if not self-referential, false otherwise</returns>
        public bool IsValidUserDelegation()
        {
            return FromUserId != ToUserId;
        }

        /// <summary>
        /// Validates if the delegation can be extended to a new end date
        /// </summary>
        /// <param name="newEndDate">New end date</param>
        /// <returns>True if can be extended, false otherwise</returns>
        public bool CanExtendTo(DateTime newEndDate)
        {
            return IsActive && 
                   !IsExpired && 
                   newEndDate > EndDate && 
                   newEndDate > DateTime.UtcNow;
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Returns a string representation of the delegation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"Delegation {DelegationId}: User {FromUserId} -> User {ToUserId} " +
                   $"({StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}) " +
                   $"[{(IsActive ? "Active" : "Inactive")}]";
        }

        /// <summary>
        /// Determines equality based on DelegationId
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object? obj)
        {
            if (obj is AccessDelegation other)
            {
                return DelegationId == other.DelegationId;
            }
            return false;
        }

        /// <summary>
        /// Gets hash code based on DelegationId
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return DelegationId.GetHashCode();
        }

        #endregion
    }
}
