using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsWarehouseDto
    {
        public int WarehouseId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string WarehouseAddressLine1 { get; set; } = string.Empty;
        public string? WarehouseAddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public int? DefaultInventoryLocationId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        
        // From joins
        public string? CompanyName { get; set; }
        public string? BranchName { get; set; }
        public string? BranchCode { get; set; }
    }

    public class CreateCsWarehouseDto
    {
        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        [StringLength(50)]
        public string WarehouseCode { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string WarehouseName { get; set; } = string.Empty;

        [Required]
        [StringLength(225)]
        public string WarehouseAddressLine1 { get; set; } = string.Empty;

        [StringLength(225)]
        public string? WarehouseAddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Pincode { get; set; } = string.Empty;

        public int? DefaultInventoryLocationId { get; set; }
    }

    public class UpdateCsWarehouseDto
    {
        [Required]
        public int WarehouseId { get; set; }

        [Required]
        [StringLength(255)]
        public string WarehouseName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string WarehouseCode { get; set; } = string.Empty;

        [Required]
        [StringLength(225)]
        public string WarehouseAddressLine1 { get; set; } = string.Empty;

        [StringLength(225)]
        public string? WarehouseAddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Pincode { get; set; } = string.Empty;

        public int? DefaultInventoryLocationId { get; set; }
    }

    public class CsWarehouseDropdownDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
    }

    public class CsWarehouseCreateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? WarehouseId { get; set; }
        public string? WarehouseCode { get; set; }
    }
}
