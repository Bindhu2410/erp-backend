using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs.CompanySetup
{
    public class AccountingPeriodDto
    {
        public int PeriodId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(100)]
        public string PeriodName { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Open";

        public bool IsCurrentActive { get; set; }
    }

    public class AccountingPeriodResponse
    {
        public int PeriodId { get; set; }
        public int CompanyId { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsCurrentActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AccountingPeriodSearchRequest
    {
        public string? SearchText { get; set; }
        public string? Status { get; set; }
        public DateTime? Date { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AccountingPeriodPagedResponse
    {
        public List<AccountingPeriodResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
