using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("receipt")]
    public class Receipt
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

        [Column("received_from")]
        public string? ReceivedFrom { get; set; }

        [Column("customer_name")]
        public string CustomerName { get; set; }

        [Column("sales_representative")]
        public string? SalesRepresentative { get; set; }

        [Column("salesman")]
        public string? Salesman { get; set; }

        [Column("hospital_name")]
        public string? HospitalName { get; set; }

        [Column("doc_id")]
        public string? DocId { get; set; }

        [Column("receipt_date")]
        public DateTime? ReceiptDate { get; set; }

        [Column("doc_date")]
        public DateTime? DocDate { get; set; }

        [Column("ref_no")]
        public string? RefNo { get; set; }

        [Column("ref_date")]
        public DateTime? RefDate { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("comments")]
        public string? Comments { get; set; }

        [Column("issue_id")]
        public string? IssueId { get; set; }

        // Footer fields
        [Column("gross")]
        public decimal? Gross { get; set; }

        [Column("total_qty")]
        public decimal? TotalQty { get; set; }

        [Column("amount_in_words")]
        public string? AmountInWords { get; set; }

        [Column("narration")]
        public string? Narration { get; set; }

        // Child Collections
        public virtual ICollection<ReceiptItem>? Items { get; set; }
        public virtual ICollection<ReceiptOptionalItem>? OptionalItems { get; set; }
        public virtual ICollection<ReceiptAccessory>? Accessories { get; set; }
    }
}

