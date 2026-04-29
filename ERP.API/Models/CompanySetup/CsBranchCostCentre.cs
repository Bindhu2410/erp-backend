namespace ERP.API.Models.CompanySetup
{
    public class CsBranchCostCentre
    {
        public int BranchId { get; set; }
        public int CostCentreId { get; set; }
    }

    public class CsBranchCostCentreDetail
    {
        public int BranchId { get; set; }
        public int CostCentreId { get; set; }
        public string CostCentreName { get; set; }
        public string CostCentreCode { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CsBranchDetail
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CsBranchCostCentrePagedResponse
    {
        public List<CsBranchDetail> Items { get; set; }
        public long TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class CsBranchCostCentreDropdownItem
    {
        public int CostCentreId { get; set; }
        public string CostCentreCode { get; set; }
        public string Name { get; set; }
        public int? ParentCostCentreId { get; set; }
        public string Path { get; set; }
    }
}
