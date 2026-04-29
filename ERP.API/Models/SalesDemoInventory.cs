using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("demo_inventory")]
    public class SalesDemoInventory : BaseEntity
    {
        [Column("item_id")]
        public int ItemId { get; set; }

        [Column("status")]
        [MaxLength(50)]
        public string? Status { get; set; }

        [Column("condition")]
        [MaxLength(50)]
        public string? Condition { get; set; }

        [Column("demo_start_date")]
        public DateTime? DemoStartDate { get; set; }

        [Column("demo_expected_end_date")]
        public DateTime? DemoExpectedEndDate { get; set; }

        [Column("demo_actual_end_date")]
        public DateTime? DemoActualEndDate { get; set; }

        [Column("assigned_to_type")]
        [MaxLength(20)]
        public string? AssignedToType { get; set; }

        [Column("notes")]
        [MaxLength(255)]
        public string? Notes { get; set; }

        [Column("original_cost")]
        public decimal? OriginalCost { get; set; }

        [Column("current_value")]
        public decimal? CurrentValue { get; set; }

        [Column("last_inspection_date")]
        public DateTime? LastInspectionDate { get; set; }

        [Column("last_maintenance_date")]
        public DateTime? LastMaintenanceDate { get; set; }

        [NotMapped]
        public string? Product { get; set; }
    }
}
