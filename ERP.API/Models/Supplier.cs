using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace ERP.API.Models
{
        [Table("suppliers")]
        public class Supplier
        {
            [Column("id")]
            public int Id { get; set; }
            [Column("vendor_code")]
            public string? VendorCode { get; set; }
            [Column("vendor_name")]
            public string VendorName { get; set; }
            [Column("phone")]
            public List<string> Phone { get; set; } = new List<string>();
            [Column("email")]
            public List<string> Email { get; set; } = new List<string>();
    [Column("door_no")]
    public string? DoorNo { get; set; }
    [Column("street")]
    public string? Street { get; set; }
    [Column("area")]
    public string? Area { get; set; }
    [Column("city")]
    public string? City { get; set; }
    [Column("state")]
    public string? State { get; set; }
    [Column("country")]
    public string? Country { get; set; }
    [Column("pincode")]
    public string? Pincode { get; set; }
    [Column("address")]
    public string? Address { get; set; }
    [Column("gst_number")]
    public string? GstNumber { get; set; }
    [Column("pan_number")]
    public string? PanNumber { get; set; }
    [Column("is_registered")]
    public bool IsRegistered { get; set; } = false;
    [Column("bank_name")]
    public string? BankName { get; set; }
    [Column("account_holder_name")]
    public string? AccountHolderName { get; set; }
    [Column("bank_account_number")]
    public string? BankAccountNumber { get; set; }
    [Column("ifsc_code")]
    public string? IfscCode { get; set; }
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
