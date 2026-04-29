using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("delivery_challans")]
    public class DeliveryChallan
    {
        [Key]
        public int Id { get; set; }

        [Column("delivery_challan_id")]
        public string? DeliveryChallanId { get; set; }

        [Column("delivery_date")]
        public DateTime DeliveryDate { get; set; }

        [Column("sales_order_id")]
        public int? SalesOrderId { get; set; }

        [Column("salesman_id")]
        public int? SalesmanId { get; set; }

        [Column("party_id")]
        public int? PartyId { get; set; }

        [Column("delivery_status")]
        public string? DeliveryStatus { get; set; }

        [Column("dispatch_address")]
        public string? DispatchAddress { get; set; }

        [Column("priority")]
        public string? Priority { get; set; }

        [Column("transporter_name")]
        public string? TransporterName { get; set; }

        [Column("vehicle_no")]
        public string? VehicleNo { get; set; }

        [Column("driver_name")]
        public string? DriverName { get; set; }

        [Column("driver_contact")]
        public long? DriverContact { get; set; }

        [Column("mode_of_delivery")]
        public string? ModeOfDelivery { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        // New Fields from UI
        [Column("location")]
        public string? Location { get; set; }

        [Column("form_20_sno")]
        public string? Form20SNo { get; set; }

        [Column("form_20_no")]
        public string? Form20No { get; set; }

        [Column("ref_no")]
        public string? RefNo { get; set; }

        [Column("ref_date")]
        public DateTime? RefDate { get; set; }

        [Column("dispatched_by")]
        public string? DispatchedBy { get; set; }

        [Column("delivered_by")]
        public string? DeliveredBy { get; set; }

        [Column("goods_consign_from")]
        public string? GoodsConsignFrom { get; set; }

        [Column("goods_consign_to")]
        public string? GoodsConsignTo { get; set; }

        [Column("booking_address")]
        public string? BookingAddress { get; set; }

        [Column("booking_qty")]
        public decimal? BookingQty { get; set; }

        [Column("app_value")]
        public decimal? AppValue { get; set; }

        [Column("delivery_at")]
        public string? DeliveryAt { get; set; }

        [Column("delivery_add1")]
        public string? DeliveryAdd1 { get; set; }

        [Column("delivery_add2")]
        public string? DeliveryAdd2 { get; set; }

        [Column("document_through")]
        public string? DocumentThrough { get; set; }

        [Column("invoice_no")]
        public string? InvoiceNo { get; set; }

        [Column("invoice_date")]
        public DateTime? InvoiceDate { get; set; }

        // Footer Fields
        [Column("gross_amount")]
        public decimal? GrossAmount { get; set; }

        [Column("net_amount")]
        public decimal? NetAmount { get; set; }

        [Column("total_qty")]
        public decimal? TotalQty { get; set; }

        [Column("amount_in_words")]
        public string? AmountInWords { get; set; }

        [Column("delivery_to")]
        public string? DeliveryTo { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("prepared_by")]
        public string? PreparedBy { get; set; }

        [Column("authorized_by")]
        public string? AuthorizedBy { get; set; }

        [Column("received_by")]
        public string? ReceivedBy { get; set; }

        [Column("user_created")]
        public int? UserCreated { get; set; }

        [Column("date_created")]
        public DateTime? DateCreated { get; set; }

        [Column("user_updated")]
        public int? UserUpdated { get; set; }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }

        public List<DeliveryChallanItem>? Items { get; set; }
    }
}
