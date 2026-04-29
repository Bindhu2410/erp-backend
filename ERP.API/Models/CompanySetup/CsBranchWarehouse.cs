using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models.CompanySetup
{
    [Table("cs_branch_warehouses")]
    public class CsBranchWarehouse
    {
        [Key]
        [Column("warehouse_id")]
        public int WarehouseId { get; set; }

        [Column("branch_id")]
        public int BranchId { get; set; }

        [Required]
        [Column("warehouse_code")]
        public string WarehouseCode { get; set; } = string.Empty;

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("address_line1")]
        public string AddressLine1 { get; set; } = string.Empty;

        [Column("address_line2")]
        public string? AddressLine2 { get; set; }

        [Required]
        [Column("city")]
        public string City { get; set; } = string.Empty;

        [Required]
        [Column("state")]
        public string State { get; set; } = string.Empty;

        [Required]
        [Column("pincode")]
        public string Pincode { get; set; } = string.Empty;

        [Column("contact_person")]
        public string? ContactPerson { get; set; }

        [Column("contact_number")]
        public string? ContactNumber { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
