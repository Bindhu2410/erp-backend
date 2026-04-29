using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models.CompanySetup
{
    [Table("cs_warehouses")]
    public class CsWarehouse
    {
        [Key]
        [Column("warehouse_id")]
        public int WarehouseId { get; set; }

        [Required]
        [Column("company_id")]
        public int CompanyId { get; set; }

        [Required]
        [Column("branch_id")]
        public int BranchId { get; set; }

        [Required]
        [Column("warehouse_code")]
        [StringLength(50)]
        public string WarehouseCode { get; set; } = string.Empty;

        [Required]
        [Column("warehouse_name")]
        [StringLength(255)]
        public string WarehouseName { get; set; } = string.Empty;

        [Required]
        [Column("warehouse_address_line1")]
        [StringLength(225)]
        public string WarehouseAddressLine1 { get; set; } = string.Empty;

        [Column("warehouse_address_line2")]
        [StringLength(225)]
        public string? WarehouseAddressLine2 { get; set; }

        [Required]
        [Column("city")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [Column("state")]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [Column("pincode")]
        [StringLength(10)]
        public string Pincode { get; set; } = string.Empty;

        [Column("default_inventory_location_id")]
        public int? DefaultInventoryLocationId { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual CsCompany? Company { get; set; }

        [ForeignKey("BranchId")]
        public virtual CsBranch? Branch { get; set; }
    }
}
