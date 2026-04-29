
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    public class Issue
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("user_created")]
        public int? UserCreated { get; set; }
        [Column("date_created")]
        public DateTime? DateCreated { get; set; }
        [Column("user_updated")]
        public int? UserUpdated { get; set; }
        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }
        [Column("location_id")]
        public string? LocationId { get; set; }
        [Column("bom_id")]
        public string[]? BomIds { get; set; }
        [Column("iss_to")]
        public string? IssTo { get; set; }
        [Column("issue_to")]
        public string? IssueTo { get; set; }
        [Column("customer_name")]
        public string? CustomerName { get; set; }
        [Column("party_branch")]
        public string? PartyBranch { get; set; }
        [Column("status")]
        public string? Status { get; set; }
        [Column("sales_representative")]
        public string? SalesRepresentative { get; set; }
        [Column("goods_consign_from")]
        public string? GoodsConsignFrom { get; set; }
        [Column("goods_consign_to")]
        public string? GoodsConsignTo { get; set; }
        [Column("delivered_by")]
        public string? DeliveredBy { get; set; }
        [Column("booking_address")]
        public string? BookingAddress { get; set; }
        [Column("booking_qty")]
        public int? BookingQty { get; set; }
        [Column("app_value")]
        public decimal? AppValue { get; set; }
        [Column("received_on")]
        public DateTime? ReceivedOn { get; set; }
        [Column("bom_name")]
        public string? BomName { get; set; }
        [Column("demo_from")]
        public string? DemoFrom { get; set; }
        [Column("demo_report")]
        public string? DemoReport { get; set; }
        [Column("demo_request")]
        public string? DemoRequest { get; set; }
        [Column("demo_remarks")]
        public string? DemoRemarks { get; set; }
        [Column("doc_id")]
        public string? DocId { get; set; } // Output only
        [Column("issue_date")]
        public DateTime? IssueDate { get; set; }
        [Column("ref_no")]
        public string? RefNo { get; set; }
        [Column("ref_date")]
        public DateTime? RefDate { get; set; }
        [Column("comments")]
        public string? Comments { get; set; }
        [Column("narration")]
        public string? Narration { get; set; }
        [Column("receipt_id")]
        public string? ReceiptId { get; set; }

        // Billing Section
        [Column("generate_invoice")]
        public string? GenerateInvoice { get; set; } // YES/NO
        [Column("bill_no")]
        public string? BillNo { get; set; }
        [Column("bill_date")]
        public DateTime? BillDate { get; set; }
        [Column("doctor_name")]
        public string? DoctorName { get; set; }
        [Column("billing_description")]
        public string? BillingDescription { get; set; }
        [Column("billing_amount")]
        public decimal? BillingAmount { get; set; }

        // Footer fields
        [Column("gross")]
        public decimal? Gross { get; set; }
        [Column("total_qty")]
        public decimal? TotalQty { get; set; }
        [Column("amount_in_words")]
        public string? AmountInWords { get; set; }

        // Eway Bill Section
        [Column("eway_bill_no")]
        public string? EwayBillNo { get; set; }
        [Column("eway_bill_date")]
        public DateTime? EwayBillDate { get; set; }
        [Column("transporter")]
        public string? Transporter { get; set; }
        [Column("vehicle_no")]
        public string? VehicleNo { get; set; }
        
        [Column("from_gstin")]
        public string? FromGstin { get; set; }
        
        [Column("to_gstin")]
        public string? ToGstin { get; set; }
        
        [Column("distance")]
        public int? Distance { get; set; }
        
        [Column("transporter_id")]
        public string? TransporterId { get; set; }
        
        [Column("eway_bill_status")]
        public string? EwayBillStatus { get; set; }
        
        [Column("supply_type")]
        public string? SupplyType { get; set; } = "O"; // Default: Outward
        
        [Column("sub_type")]
        public string? SubType { get; set; } = "1"; // Default: Supply
        
        [Column("doc_type")]
        public string? DocType { get; set; } = "INV"; // Default: Tax Invoice

        public System.Collections.Generic.ICollection<IssueOptionalItem>? OptionalItems { get; set; }
        public System.Collections.Generic.ICollection<IssueItem>? IssueItems { get; set; }
    }
}
