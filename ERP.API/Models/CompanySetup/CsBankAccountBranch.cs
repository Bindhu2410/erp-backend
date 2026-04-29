using System;

namespace ERP.API.Models.CompanySetup
{
    public class CsBankAccountBranch
    {
        public int BankAccountId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string Address { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
    }

    public class CsBankAccountBranchDetail : CsBankAccount
    {
        public new long TotalCount { get; set; }
    }
}
