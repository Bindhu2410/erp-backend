using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ERP.API.Services.Implementation
{
    public class EWayBillService : IEWayBillService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EWayBillService> _logger;

        private const string TokenCacheKey = "GspAccessToken";

        public EWayBillService(
            HttpClient httpClient,
            AppDbContext context,
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<EWayBillService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (_cache.TryGetValue(TokenCacheKey, out string token))
            {
                return token;
            }

            var clientId = _configuration["EWayBill:ClientId"];
            var clientSecret = _configuration["EWayBill:ClientSecret"];
            var baseUrl = _configuration["EWayBill:BaseUrl"];

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/authenticate");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["grant_type"] = "client_credentials"
            });

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GSP Authentication failed: {Status}", response.StatusCode);
                throw new Exception("GSP Authentication failed");
            }

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GspTokenResponse>(body);

            if (result == null || string.IsNullOrEmpty(result.AccessToken))
            {
                throw new Exception("Invalid GSP token response");
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(result.ExpiresIn - 60)); // Buffer of 60s

            _cache.Set(TokenCacheKey, result.AccessToken, cacheOptions);

            return result.AccessToken;
        }

        public async Task<EWayBillResponseDto> GenerateEWayBillAsync(int issueId)
        {
            var issue = await _context.Issues
                .Include(i => i.IssueItems)
                .FirstOrDefaultAsync(i => i.Id == issueId);

            if (issue == null)
            {
                return new EWayBillResponseDto { Success = false, ErrorDetails = "Issue not found" };
            }

            try
            {
                var requestBody = MapToEWayRequest(issue);
                var token = await GetAccessTokenAsync();
                
                var baseUrl = _configuration["EWayBill:BaseUrl"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/ewaybill/generate", requestBody);
                var resultBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<EWayBillResponseDto>(resultBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result != null && result.Success)
                {
                    issue.EwayBillNo = result.EwayBillNo;
                    issue.EwayBillDate = DateTime.TryParse(result.EwayBillDate, out var date) ? date : DateTime.UtcNow;
                    issue.EwayBillStatus = "GENERATED";
                    await _context.SaveChangesAsync();
                }

                return result ?? new EWayBillResponseDto { Success = false, ErrorDetails = "Null response from GSP" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating e-Way Bill for Issue {IssueId}", issueId);
                return new EWayBillResponseDto { Success = false, ErrorDetails = ex.Message };
            }
        }

        public async Task<EWayBillResponseDto> GetEWayBillAsync(string ewayBillNo)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                var baseUrl = _configuration["EWayBill:BaseUrl"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{baseUrl}/ewaybill/{ewayBillNo}");
                var body = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<EWayBillResponseDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching e-Way Bill {EwayBillNo}", ewayBillNo);
                return new EWayBillResponseDto { Success = false, ErrorDetails = ex.Message };
            }
        }

        public async Task<EWayBillResponseDto> CancelEWayBillAsync(EWayBillCancelRequestDto request)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                var baseUrl = _configuration["EWayBill:BaseUrl"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/ewaybill/cancel", request);
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<EWayBillResponseDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result != null && result.Success)
                {
                    var issue = await _context.Issues.FirstOrDefaultAsync(i => i.EwayBillNo == request.EwayBillNo.ToString());
                    if (issue != null)
                    {
                        issue.EwayBillStatus = "CANCELLED";
                        await _context.SaveChangesAsync();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling e-Way Bill {EwayBillNo}", request.EwayBillNo);
                return new EWayBillResponseDto { Success = false, ErrorDetails = ex.Message };
            }
        }

        public async Task<EWayBillResponseDto> UpdateVehicleAsync(EWayBillUpdateVehicleRequestDto request)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                var baseUrl = _configuration["EWayBill:BaseUrl"];
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/ewaybill/updateVehicle", request);
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<EWayBillResponseDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result != null && result.Success)
                {
                    var issue = await _context.Issues.FirstOrDefaultAsync(i => i.EwayBillNo == request.EwayBillNo.ToString());
                    if (issue != null)
                    {
                        issue.VehicleNo = request.VehicleNo;
                        await _context.SaveChangesAsync();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle for e-Way Bill {EwayBillNo}", request.EwayBillNo);
                return new EWayBillResponseDto { Success = false, ErrorDetails = ex.Message };
            }
        }

        private EWayBillRequestDto MapToEWayRequest(Issue issue)
        {
            // This is a mapping logic based on ERP data
            // In a real scenario, you'd pull addresses from Company and Customer tables
            return new EWayBillRequestDto
            {
                DocNo = issue.BillNo ?? issue.DocId,
                DocDate = (issue.BillDate ?? issue.IssueDate ?? DateTime.UtcNow).ToString("dd/MM/yyyy"),
                FromGstin = issue.FromGstin,
                ToGstin = issue.ToGstin,
                Distance = issue.Distance ?? 0,
                TransporterId = issue.TransporterId,
                VehicleNo = issue.VehicleNo,
                SupplyType = issue.SupplyType ?? "O",
                SubType = issue.SubType ?? "1",
                DocType = issue.DocType ?? "INV",
                TotInvValue = issue.Gross ?? 0,
                TotalValue = issue.Gross ?? 0,
                CgstValue = issue.IssueItems?.Sum(x => x.CgstAmount ?? 0) ?? 0,
                SgstValue = issue.IssueItems?.Sum(x => x.SgstAmount ?? 0) ?? 0,
                IgstValue = issue.IssueItems?.Sum(x => x.IgstAmount ?? 0) ?? 0,
                ItemList = issue.IssueItems?.Select(item => new EWayItemDto
                {
                    ProductName = item.Product ?? item.Item,
                    ProductDesc = $"{item.Make} {item.Model}",
                    HsnCode = item.HsnCode,
                    Quantity = item.Qty ?? 0,
                    QtyUnit = item.Unit ?? "NOS",
                    TaxableAmount = item.Amount ?? 0,
                    CgstRate = item.CgstRate ?? 0,
                    SgstRate = item.SgstRate ?? 0,
                    IgstRate = item.IgstRate ?? 0
                }).ToList()
            };
        }
    }
}
