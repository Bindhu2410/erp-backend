using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsBankAccountDto
    {
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string BankName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string BankBranchName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string IFSCCode { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? SwiftCode { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Purpose { get; set; } = string.Empty;
        
        [Required]
        [StringLength(5)]
        public string Currency { get; set; } = string.Empty;
    }
}
