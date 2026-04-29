using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using ERP.API.Models;

namespace ERP.API.Models.CompanySetup
{
    public class CsDefaultAccountMapping
    {
        public int MappingId { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        public string? CompanyName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? TransactionType { get; set; } = string.Empty;
        
        [Required]
        public int DefaultDebitAccountId { get; set; }
        
        [Required]
        public int DefaultCreditAccountId { get; set; }
        
        public string? DebitAccountName { get; set; }
        public string? CreditAccountName { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CsDefaultAccountMappingRequest
    {
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? TransactionType { get; set; } = string.Empty;
        
        [Required]
        public int DefaultDebitAccountId { get; set; }
        
        [Required]
        public int DefaultCreditAccountId { get; set; }
    }

    public class CsDefaultAccountMappingSearchRequest
    {
        [Required]
        public int CompanyId { get; set; }
        
        public string? SearchText { get; set; }
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class CsDefaultAccountMappingResponse : PagedResponse<CsDefaultAccountMapping>
    {
        public CsDefaultAccountMappingResponse() : base() { }
        
        public CsDefaultAccountMappingResponse(IEnumerable<CsDefaultAccountMapping> data, int pageNumber, int pageSize, int totalRecords, int filteredRecords)
            : base(data, pageNumber, pageSize, totalRecords, filteredRecords) { }
    }
}
