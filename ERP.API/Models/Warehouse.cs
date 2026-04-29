using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("warehouse", Schema = "public")]
    public class Warehouse
    {
        [Key]
    [Column("id")]
    public int Id { get; set; }
    [Required]
    [MaxLength(255)]
    [Column("warehouse_name")]
    public string WarehouseName { get; set; }
    [MaxLength(50)]
    [Column("warehouse_type")]
    public string WarehouseType { get; set; }
    [MaxLength(255)]
    [Column("address")]
    public string Address { get; set; }
    [MaxLength(100)]
    [Column("city")]
    public string City { get; set; }
    [MaxLength(100)]
    [Column("state")]
    public string State { get; set; }
    [MaxLength(100)]
    [Column("country")]
    public string Country { get; set; }
    [MaxLength(20)]
    [Column("pincode")]
    public string Pincode { get; set; }
    [MaxLength(100)]
    [Column("contact_person")]
    public string ContactPerson { get; set; }
    [MaxLength(50)]
    [Column("phone")]
    public string Phone { get; set; }
    [MaxLength(100)]
    [Column("email")]
    public string Email { get; set; }
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    [Column("parent_warehouse_id")]
    public int? ParentWarehouseId { get; set; }
    [ForeignKey("ParentWarehouseId")]
    public Warehouse? ParentWarehouse { get; set; }
        [Column("user_created")]
        public int? UserCreated { get; set; }

        [Column("date_created")]
        public DateTime? DateCreated { get; set; }

        [Column("user_updated")]
        public int? UserUpdated { get; set; }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }
    }
}
