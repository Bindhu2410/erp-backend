using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models.DTOs
{
    public class BomDropdownListDto
    {
    public string? BomId { get; set; }
    public string? BomName { get; set; }
    public string? BomType { get; set; }
    public List<ItemDropdownDto>? ChildItems { get; set; }
    }
}
