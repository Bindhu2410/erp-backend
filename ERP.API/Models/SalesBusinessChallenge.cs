using ERP.API.Models;

public class SalesBusinessChallenge:BaseEntity
{
    public override int? Id { get; set; }
    public string? Solution { get; set; }
    public string? Challenges { get; set; }
    public List<SalesProduct>? Products { get; set; } = new();
}
