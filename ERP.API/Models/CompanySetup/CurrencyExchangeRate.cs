using System;

namespace ERP.API.Models.CompanySetup
{
    public class CurrencyExchangeRate
    {
        public int ExchangeRateId { get; set; }
        public int CompanyId { get; set; }
        public int FromCurrencyId { get; set; }
        public int ToCurrencyId { get; set; }
        public DateTime RateDate { get; set; }
        public decimal ExchangeRate { get; set; }
        public string? RateType { get; set; }
        public string? RateSource { get; set; }
        public bool IsActive { get; set; }
        public DateTime EffectiveFromDate { get; set; }
        public DateTime? EffectiveToDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
