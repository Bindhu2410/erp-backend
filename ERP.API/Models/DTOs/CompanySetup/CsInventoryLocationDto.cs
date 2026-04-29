using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsInventoryLocationDto
    {
        public int LocationId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        [StringLength(50)]
        public string LocationCode { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string LocationName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? LocationCategory { get; set; }

        public decimal? CapacityWeight { get; set; }

        [StringLength(10)]
        public string? CapacityWeightUom { get; set; }

        public decimal? CapacityVolume { get; set; }

        [StringLength(10)]
        public string? CapacityVolumeUom { get; set; }

        public int? CapacityItemCount { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }

        public int TotalRecords { get; set; }
    }
}
