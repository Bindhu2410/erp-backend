namespace ERP.API.Models.DTOs
{
    public class GeographicalHierarchyDto
    {
        public long DivisionId { get; set; }
        public long? ParentDivisionId { get; set; }
        public string DivisionName { get; set; } = string.Empty;
        public string DivisionType { get; set; } = string.Empty;
        public int Level { get; set; }
    }
}
