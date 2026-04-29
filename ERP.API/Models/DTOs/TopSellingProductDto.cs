using System;
using System.Collections.Generic;

namespace ERP.API.Models.DTOs
{
    public class TopSellingProductDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string? ImageUrl { get; set; }
        public int UnitsSold { get; set; }
        public DateTime? LastSoldDate { get; set; }
    }
}
