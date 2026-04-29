using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ERP.API.Models
{
    public class SalesOrder
    {
        public int Id { get; set; }
          [StringLength(20)]
        public string? OrderId { get; set; }
         public string CustomerName { get; set; }

         public string MobileNum { get; set; } // NEW: for Opportunity Contact Mobile Number

        //[Required]
    public int? CustomerId { get; set; }
        
        [Required]        public DateTimeOffset OrderDate { get; set; }
        
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }
        
        [Required]
        [StringLength(50)]
        public string? Status { get; set; }
        
        public int? QuotationId { get; set; }
        
        [StringLength(50)]
        public string? PoId { get; set; }
        
        public DateTimeOffset? AcceptanceDate { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        public decimal TaxAmount { get; set; }
        
        public decimal GrandTotal { get; set; }

        /// <summary>
        /// Freight charge for the order. Maps to 'freight_charge' column in DB.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.Column("freight_charge")]
        public decimal? FreightCharge { get; set; }
        
        public string? Notes { get; set; }
        
        public int? UserCreated { get; set; }
          public DateTimeOffset? DateCreated { get; set; }
        
        public int? UserUpdated { get; set; }
        
        public DateTimeOffset? DateUpdated { get; set; }
    }

    public class SalesOrderGrid
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string OrderId { get; set; }
        public string MobileNum { get; set; } // Added for API response
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Status { get; set; }
        public string? PoId { get; set; }
        public decimal GrandTotal { get; set; }

        /// <summary>
        /// QuotationId for the order. Maps to 'quotation_id' column in DB.
        /// </summary>
        public int? QuotationId { get; set; }

        /// <summary>
        /// Freight charge for the order. Maps to 'freight_charge' column in DB.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.Column("freight_charge")]
        public decimal? FreightCharge { get; set; }
        public object QuotationInfo { get; set; }
        public List<SalesProduct>? Items { get; set; } // Change from List<object> to List<SalesProduct>
        // Added for API response
    }

    public class SalesOrderDetailsDto
    {
        public SalesOrder SalesOrder { get; set; }
            public string CustomerName { get; set; }
        public SalesQuotation Quotation { get; set; }
        public List<SalesProduct> Items { get; set; }
     // Added for API response
    }

    // ...existing code...
    // (removed duplicate/erroneous class definition and misplaced curly braces)
}
