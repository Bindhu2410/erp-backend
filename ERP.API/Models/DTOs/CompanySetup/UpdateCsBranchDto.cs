using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class UpdateCsBranchDto
    {
        [Required(ErrorMessage = "Branch ID is required")]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Branch name is required")]
        [StringLength(255)]
        public string BranchName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branch code is required")]
        [StringLength(50)]
        public string BranchCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address line 1 is required")]
        [StringLength(225)]
        public string AddressLine1 { get; set; } = string.Empty;

        [StringLength(225)]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pincode is required")]
        [StringLength(10)]
        public string Pincode { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [StringLength(15)]
        public string? Gstin { get; set; }

        public bool IsHeadOffice { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }
}
