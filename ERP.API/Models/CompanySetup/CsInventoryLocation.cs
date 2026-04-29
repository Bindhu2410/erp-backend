using System;

namespace ERP.API.Models.CompanySetup
{
    public class CsInventoryLocation
    {
        public int LocationId { get; set; }
        public int WarehouseId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string? LocationCategory { get; set; }
        public decimal? CapacityWeight { get; set; }
        public string? CapacityWeightUom { get; set; }
        public decimal? CapacityVolume { get; set; }
        public string? CapacityVolumeUom { get; set; }
        public int? CapacityItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
