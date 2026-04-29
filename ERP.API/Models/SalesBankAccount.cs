using System;

namespace ERP.API.Models
{
    public class SalesBankAccount
    {
        public int Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? Branch { get; set; }
        public string? RegisteredCompany { get; set; }
        public string? NameOfTheBank { get; set; }
        public string? AccountNo { get; set; }
        public string? IFSCCode { get; set; }
        public string? AccountHolderName { get; set; }
    }
}
