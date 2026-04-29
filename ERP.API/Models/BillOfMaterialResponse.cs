namespace ERP.API.Models
{
    public class BillOfMaterialChildItemResponse
    {
        public int Id { get; set; }
        public int ChildItemId { get; set; }
        public string? ChildItemName { get; set; }
        public string? ChildItemCode { get; set; }
        public decimal Quantity { get; set; }
    }

    public class BillOfMaterialResponse
    {
        public int Id { get; set; }
        public string? BomId { get; set; }
        public string? BomName { get; set; }
        public string? BomType { get; set; }
        public string? ParentItemName { get; set; }
        public string? ParentItemCode { get; set; }
        public List<BillOfMaterialChildItemResponse> ChildItems { get; set; } = new List<BillOfMaterialChildItemResponse>();
    }
}
