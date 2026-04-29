using System;
using System.Collections.Generic;

namespace ERP.API.Models
{
    public class DeliveryItem
    {
        public int ItemId { get; set; }
        public int UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public int UserUpdated { get; set; }
        public DateTime DateUpdated { get; set; }
        public int Qty { get; set; }
        public decimal Amount { get; set; }
        public bool IsActive { get; set; }
        public decimal UnitPrice { get; set; }
        public int[]? IncludedChildItemIds { get; set; }
        public int[]? AccessoriesIds { get; set; }

        // Navigation properties for full child and accessory item details
        public List<DeliveryItem>? IncludedChildItems { get; set; }
        public List<DeliveryItem>? AccessoriesItems { get; set; }
    }
}
