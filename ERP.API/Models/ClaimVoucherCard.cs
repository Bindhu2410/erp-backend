using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.API.Models
{
    public class ClaimVoucherCard
    {
        public int NewThisWeek { get; set; }
        public decimal ThisWeekTotalAmount { get; set; }
        public int TotalVouchersThisMonth { get; set; }
        public decimal MonthTotalAmount { get; set; }
    }
}
