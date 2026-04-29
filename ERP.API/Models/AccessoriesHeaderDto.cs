/*
Sample Request Body:
{
    "accessoriesHeader": {
        "date": "2026-04-15T05:43:25.158Z",
        "itemId": 1,
        "itemDescription": "Sample Item",
        "accessoriesDetails": [
            {
                "name": "Accessory 1",
                "type": "TypeA",
                "qty": 10.5
            }
        ]
    }
}
*/

namespace ERP.API.Models
{
    public class AccessoriesDetailDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal? Qty { get; set; }
    }

    public class AccessoriesHeaderDto
    {
        public int Id { get; set; }
        public string? AccesoryId { get; set; }
        public DateTime? Date { get; set; }
        public int ItemId { get; set; }
        public string ItemDescription { get; set; }
        public List<AccessoriesDetailDto> AccessoriesDetails { get; set; }
    }

    public class AccessoriesHeaderRequest
    {
        public AccessoriesHeaderDto AccessoriesHeader { get; set; }
    }
}
