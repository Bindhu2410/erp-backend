using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    [Table("sales_customers", Schema = "public")]
    public class SalesCustomer
    {
        [Key]
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

        [Column("customer_id")]
        [MaxLength(255)]
        public string? CustomerId { get; set; }

        [Column("first_deal_id")]
        [MaxLength(255)]
        public string? FirstDealId { get; set; }

        [Column("primary_contact")]
        [MaxLength(255)]
        public string? PrimaryContact { get; set; }

        [Column("customer_type")]
        [MaxLength(255)]
        public string? CustomerType { get; set; }

        [Column("source")]
        [MaxLength(255)]
        public string? Source { get; set; }

        [Column("social_media")]
        [MaxLength(255)]
        public string? SocialMedia { get; set; }

        [Column("referral_source_name")]
        [MaxLength(255)]
        public string? ReferralSourceName { get; set; }

        [Column("hospital_of_referral")]
        [MaxLength(255)]
        public string? HospitalOfReferral { get; set; }

        [Column("department_of_referral")]
        [MaxLength(255)]
        public string? DepartmentOfReferral { get; set; }

        [Column("city_of_referral")]
        [MaxLength(255)]
        public string? CityOfReferral { get; set; }

        [Column("event_name")]
        [MaxLength(255)]
        public string? EventName { get; set; }

        [Column("event_date")]
        public DateTime? EventDate { get; set; }

        [Column("status")]
        [MaxLength(255)]
        public string? Status { get; set; }

        [Column("territory_id")]
        [MaxLength(255)]
        public string? TerritoryId { get; set; }

        [Column("city_id")]
        [MaxLength(255)]
        public string? CityId { get; set; }

        [Column("customer_internal_id")]
        [MaxLength(255)]
        public string? CustomerInternalId { get; set; }

        [Column("sales_person_id")]
        public int? SalesPersonId { get; set; }

        [Column("email_opt_out")]
        public bool? EmailOptOut { get; set; }

        [Column("rating")]
        [MaxLength(255)]
        public string? Rating { get; set; }

        [Column("phone_no")]
        [MaxLength(255)]
        public string? PhoneNo { get; set; }

        [Column("phone_ext")]
        [MaxLength(255)]
        public string? PhoneExt { get; set; }

        [Column("ticker_symbol")]
        [MaxLength(255)]
        public string? TickerSymbol { get; set; }

        [Column("employees")]
        public int? Employees { get; set; }

        [Column("annual_revenue")]
        public double? AnnualRevenue { get; set; }

        [Column("nic_code")]
        [MaxLength(255)]
        public string? NicCode { get; set; }

        [Column("parent_customer")]
        [MaxLength(255)]
        public string? ParentCustomer { get; set; }

        [Column("ownership")]
        [MaxLength(255)]
        public string? Ownership { get; set; }

        [Column("website")]
        [MaxLength(255)]
        public string? Website { get; set; }

        [Column("fax")]
        [MaxLength(255)]
        public string? Fax { get; set; }

        [Column("payment_term")]
        [MaxLength(255)]
        public string? PaymentTerm { get; set; }

        [Column("credit_limit")]
        [MaxLength(255)]
        public string? CreditLimit { get; set; }

        [Column("loyalty_program")]
        [MaxLength(255)]
        public string? LoyaltyProgram { get; set; }

        [Column("sales_partner")]
        [MaxLength(255)]
        public string? SalesPartner { get; set; }

        [Column("coommission_rate")]
        public double? CoommissionRate { get; set; }

        [Column("total_receivable")]
        public double? TotalReceivable { get; set; }

        [Column("outstanding")]
        public double? Outstanding { get; set; }

        [Column("account_balance")]
        public double? AccountBalance { get; set; }

        [Column("name")]
        [MaxLength(255)]
        public string? Name { get; set; }

        [Column("territory")]
        [MaxLength(255)]
        public string? Territory { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = false;
    }
}
