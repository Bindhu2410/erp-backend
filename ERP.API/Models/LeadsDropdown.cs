using System.ComponentModel;

namespace ERP.API.Models
{    public class LeadsDropdownRequest
    {
        [DefaultValue(null)]
        public string? SearchText { get; set; }

        [DefaultValue(1)]
        public int? PageNumber { get; set; } = 1;

        [DefaultValue(10)]
        public int? PageSize { get; set; } = 10;
    }

    public class LeadsDropdownResult
    {
        public int TotalRecords { get; set; }
        public int Id { get; set; }
        public string? LeadId { get; set; }
        public string? CustomerName { get; set; }
    }
}
