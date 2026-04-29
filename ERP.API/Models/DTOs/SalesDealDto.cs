using System;
using System.Collections.Generic;

namespace ERP.API.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}

namespace ERP.API.Models.DTOs
{
    public class SalesDealDto
    {
        public int Id { get; set; }
        public int DealId { get; set; }
        public string? ClinicHospitalIndividual { get; set; }
        public string? LeadId { get; set; }
        public double? Amount { get; set; }
        public string? Status { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string? Territory { get; set; }
        public string? ContactName { get; set; }
        public string? PaymentStatus { get; set; }
        public double? ExpectedRevenue { get; set; }
        public string? DealAge { get; set; }
        public string? ContactPhone { get; set; }
    }    public class DealFilterCriteria
    {
        public int? TerritoryId { get; set; }
        public int? ZoneId { get; set; }
        public int? DivisionId { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        public string? Score { get; set; }
        public string? LeadType { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
    }

    public class DealSummary
    {
        public int CurrentDeals { get; set; }
        public int WonDeals { get; set; }
        public int LostDeals { get; set; }
        public int OnHoldDeals { get; set; }    }
}