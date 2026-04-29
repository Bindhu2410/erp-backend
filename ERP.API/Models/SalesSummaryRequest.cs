using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models
{
    public class SalesSummaryRequest
    {
        public required string Stage { get; set; }
        public required string StageItemId { get; set; }
    }
}