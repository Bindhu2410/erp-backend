using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    public class PurchaseOrderItemDto
    {
        public int Id { get; set; }
        [Column("purchase_order_id")]
        public int PurchaseOrderId { get; set; }
        [Column("item_id")]
        public int ItemId { get; set; }
        [Column("supplier_id")]
        public int? SupplierId { get; set; }
        public int Quantity { get; set; }
    }
}
