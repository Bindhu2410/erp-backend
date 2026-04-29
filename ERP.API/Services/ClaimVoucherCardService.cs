using Microsoft.EntityFrameworkCore;
using ERP.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.API.Services
{
    public interface IClaimVoucherCardService
    {
        Task<ClaimVoucherCard> GetClaimVoucherCardsAsync();
    }

    public class ClaimVoucherCardService : IClaimVoucherCardService
    {
        private readonly AppDbContext _context;

        public ClaimVoucherCardService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get claim voucher card statistics for this week and this month
        /// </summary>
        /// <returns>ClaimVoucherCard containing statistics</returns>
        public async Task<ClaimVoucherCard> GetClaimVoucherCardsAsync()
        {
            var now = DateTime.UtcNow;
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // Fetch all vouchers into memory to avoid DateTime Kind issues
            var vouchers = await _context.ClaimVouchers
                .Select(cv => new { cv.DateCreated, cv.TotalAmount })
                .ToListAsync();

            // Count new vouchers this week
            var newThisWeek = vouchers
                .Count(cv => cv.DateCreated.HasValue 
                    && cv.DateCreated.Value.ToUniversalTime() >= startOfWeek 
                    && cv.DateCreated.Value.ToUniversalTime() < now.AddDays(1));

            // Sum total amount this week
            var thisWeekTotal = vouchers
                .Where(cv => cv.DateCreated.HasValue 
                    && cv.DateCreated.Value.ToUniversalTime() >= startOfWeek 
                    && cv.DateCreated.Value.ToUniversalTime() < now.AddDays(1))
                .Sum(cv => cv.TotalAmount ?? 0);

            // Count total vouchers this month
            var totalThisMonth = vouchers
                .Count(cv => cv.DateCreated.HasValue 
                    && cv.DateCreated.Value.ToUniversalTime() >= startOfMonth 
                    && cv.DateCreated.Value.ToUniversalTime() < startOfMonth.AddMonths(1));

            // Sum total amount this month
            var monthTotal = vouchers
                .Where(cv => cv.DateCreated.HasValue 
                    && cv.DateCreated.Value.ToUniversalTime() >= startOfMonth 
                    && cv.DateCreated.Value.ToUniversalTime() < startOfMonth.AddMonths(1))
                .Sum(cv => cv.TotalAmount ?? 0);

            return new ClaimVoucherCard
            {
                NewThisWeek = newThisWeek,
                ThisWeekTotalAmount = thisWeekTotal,
                TotalVouchersThisMonth = totalThisMonth,
                MonthTotalAmount = monthTotal
            };
        }
    }
}
