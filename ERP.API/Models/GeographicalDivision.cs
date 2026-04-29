using System;

namespace ERP.API.Models
{
    public class GeographicalDivision
    {
        public long DivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public string DivisionType { get; set; } = string.Empty;
        public long? ParentDivisionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
    }
}
