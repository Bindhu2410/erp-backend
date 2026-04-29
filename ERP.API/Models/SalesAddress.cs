using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.API.Models
{
    [Table("sales_addresses")]    
    public class SalesAddress : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public override int? Id { get; set; }

        [Column("contact_name")]
        public string? ContactName { get; set; }

        [Column("type")]
        public string? Type { get; set; }

        [Column("city")]
        public string? City { get; set; }

        [Column("state")]
        public string? State { get; set; }

        [Column("pincode")]
        public int? Pincode { get; set; }

        [Column("isactive")]
        public bool? IsActive { get; set; }

        [Column("block")]
        public string? Block { get; set; }

        [Column("department")]
        public string? Department { get; set; }

        [Column("area")]
        public string? Area { get; set; }

        [Column("opportunity_id")]
        public string? OpportunityId { get; set; }

        [Column("door_no")]
        public string? DoorNo { get; set; }

        [Column("street")]
        public string? Street { get; set; }

        [Column("land_mark")]
        public string? Landmark { get; set; }

        [Column("is_default")]
        public bool? IsDefault { get; set; }

        [Column("sales_lead_id")]
        public int? SalesLeadId { get; set; }

        // These properties are inherited from BaseEntity
        [Column("user_created")]
        public override int? UserCreated { get; set; }

        [Column("date_created")]
        public override DateTime? DateCreated { get; set; }

        [Column("user_updated")]
        public override int? UserUpdated { get; set; }

        [Column("date_updated")]
        public override DateTime? DateUpdated { get; set; }
    }    public class SalesAddressDetails : SalesAddress {
        public string? Country { get; set; }
        public new string? State { get; set; }
        public new string? City { get; set; }
        public new string? Area { get; set; }
        public new string? Pincode { get; set; }
        public string? District { get; set; }
        public string? Territory { get; set; }
    }
}