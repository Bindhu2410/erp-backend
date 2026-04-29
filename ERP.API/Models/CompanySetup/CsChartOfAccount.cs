using System;

namespace ERP.API.Models.CompanySetup
{
    public class CsChartOfAccount
    {
        public int AccountId { get; set; }
        public int CompanyId { get; set; }
        public int? ParentAccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public bool IsActive { get; set; }
        public bool CostCentreAllocationRequired { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CsChartOfAccountDto : CsChartOfAccount
    {
        public string Path { get; set; }
        public int Level { get; set; }
    }

    public class CsChartOfAccountSearchRequest
    {
        public int CompanyId { get; set; }
        public string? SearchText { get; set; }
        public string? AccountType { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class CsChartOfAccountPagedResponse
    {
        public IEnumerable<CsChartOfAccount> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class CsChartOfAccountDropdownItem
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public int? ParentAccountId { get; set; }
        public string Path { get; set; }
    }
}
