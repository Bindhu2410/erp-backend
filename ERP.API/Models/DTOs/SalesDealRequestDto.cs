using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ERP.API.Models.DTOs
{
    public class SalesDealRequestDto
    {
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int? Id { get; set; }

        [MaxLength(255)]
        public string? DealName { get; set; }        public double? Amount { get; set; }
        public double? ExpectedRevenue { get; set; }
        public DateTime? StartDate { get; set; }

        [MaxLength(255)]
        public string? DealFor { get; set; }
        public DateTime? CloseDate { get; set; }

        [MaxLength(255)]
        public string? Status { get; set; }
        public bool? IsActive { get; set; }
        public string? Comments { get; set; }
        public int? OpportunityId { get; set; }
        public int? CustomerId { get; set; }
        public int? SalesRepresentativeId { get; set; }
    }
}
