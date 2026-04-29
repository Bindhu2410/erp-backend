using System;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models
{
    public class QuotationGridRequest
    {
        public string? SearchText { get; set; }

        public string[]? CustomerNames { get; set; }

        public string[]? Statuses { get; set; }

        public string[]? QuotationTypes { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string OrderBy { get; set; } = "date_created";

        [RegularExpression("^(ASC|DESC)$", ErrorMessage = "Order direction must be either 'ASC' or 'DESC'")]
        public string OrderDirection { get; set; } = "DESC";
    }
}
