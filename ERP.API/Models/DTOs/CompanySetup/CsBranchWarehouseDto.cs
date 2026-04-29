using System;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class CsBranchWarehouseDto
    {
        public int WarehouseId { get; set; }
        public int BranchId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
