using System;

namespace ERP.API.Models.CompanySetup
{
    public class CsCostCentre
    {
        public int CostCentreId { get; set; }
        public int CompanyId { get; set; }
        public int? ParentCostCentreId { get; set; }
        public string? CostCentreCode { get; set; }
        public string? CostCentreName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ParentCostCentreName { get; set; }
        public string? CompanyName { get; set; }
        public string? ParentCostCentreCode { get; set; }
    }

    public class CsCostCentreDto
    {
        public int? ParentCostCentreId { get; set; }
        public string? CostCentreCode { get; set; }
        public string? CostCentreName { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CsCostCentreSearchRequest
    {
        public string? SearchText { get; set; }
        public bool? IsActive { get; set; }
        public int? ParentCostCentreId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class CsCostCentreHierarchyItem
    {
        public int CostCentreId { get; set; }
        public int? ParentCostCentreId { get; set; }
        public string? CostCentreCode { get; set; }
        public string? CostCentreName { get; set; }
        public int Level { get; set; }
        public string? Path { get; set; }
    }

    public class CsCostCentrePagedResponse
    {
        public List<CsCostCentre>? Items { get; set; } = new List<CsCostCentre>();
        public long TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class CsCostCentreDropdownItem
    {
        public int Value { get; set; }
        public string? Label { get; set; }
    }
}
