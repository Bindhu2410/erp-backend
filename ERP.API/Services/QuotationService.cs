using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Dapper;
using Npgsql;

namespace ERP.API.Services
{
    public class QuotationService
    {
        private readonly SalesLeadService _salesLeadService;
        private readonly IWebHostEnvironment _environment;        private readonly string _connectionString;

        public QuotationService(
            SalesLeadService salesLeadService, 
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _salesLeadService = salesLeadService;
            _environment = environment;
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentException("DefaultConnection string is not configured");
            
            // Enable case-insensitive column mapping for Dapper
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }        public async Task<QuotationResponseDto> CreateQuotationAsync(CreateQuotationRequestDto request)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Insert the quotation first
                const string quotationSql = @"SELECT * FROM create_quotation(
                    p_user_created := @UserCreated,
                    p_user_updated := @UserUpdated,
                    p_version := @Version,
                    p_terms := @Terms,
                    p_valid_till := @ValidTill,
                    p_quotation_for := @QuotationFor,
                    p_status := @Status,
                    p_lost_reason := @LostReason,
                    p_customer_id := @CustomerId,
                    p_quotation_type := @QuotationType,
                    p_quotation_date := @QuotationDate,
                    p_order_type := @OrderType,
                    p_comments := @Comments,
                    p_delivery_within := @DeliveryWithin,
                    p_delivery_after := @DeliveryAfter,
                    p_is_active := @IsActive,
                    p_quotation_id := @QuotationId,
                    p_opportunity_id := @OpportunityId,
                    p_lead_id := @LeadId,
                    p_customer_name := @CustomerName,
                    p_taxes := @Taxes,
                    p_delivery := @Delivery,
                    p_payment := @Payment,
                    p_warranty := @Warranty,
                    p_freight_charge := @FreightCharge,
                    p_is_current := @IsCurrent,
                    p_parent_sales_quotations_id := @ParentSalesQuotationsId)";

                var result = await connection.QueryFirstOrDefaultAsync<QuotationResponseDto>(quotationSql, request);
                  if (result == null)
                    throw new InvalidOperationException("Failed to create quotation");

                await transaction.CommitAsync();

                // Fetch the complete quotation
                return await GetQuotationByIdAsync(result.Id);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }        public async Task<QuotationResponseDto> UpdateQuotationAsync(UpdateQuotationRequestDto request)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                const string quotationSql = @"
                    UPDATE public.sales_quotations 
                    SET 
                        user_updated = @UserUpdated,
                        date_updated = CURRENT_TIMESTAMP,
                        version = @Version,
                        terms = @Terms,
                        valid_till = @ValidTill,
                        quotation_for = @QuotationFor,
                        status = @Status,
                        lost_reason = @LostReason,
                        customer_id = @CustomerId,
                        quotation_type = @QuotationType,
                        quotation_date = @QuotationDate,
                        order_type = @OrderType,
                        comments = @Comments,
                        delivery_within = @DeliveryWithin,
                        delivery_after = @DeliveryAfter,
                        is_active = @IsActive,
                        quotation_id = @QuotationId,
                        opportunity_id = @OpportunityId,
                        lead_id = @LeadId,
                        customer_name = @CustomerName,
                        taxes = @Taxes,
                        delivery = @Delivery,
                        payment = @Payment,
                        warranty = @Warranty,
                        freight_charge = @FreightCharge,
                        is_current = @IsCurrent,
                        parent_sales_quotations_id = @ParentSalesQuotationsId
                    WHERE id = @Id
                    RETURNING *;";

                var result = await connection.QueryFirstOrDefaultAsync<QuotationResponseDto>(quotationSql, request);
                
                if (result == null)
                    throw new InvalidOperationException($"Failed to update quotation {request.Id}");

                await transaction.CommitAsync();

                return await GetQuotationByIdAsync(result.Id);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }        public async Task<QuotationResponseDto> GetQuotationByIdAsync(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string quotationSql = @"
                SELECT * FROM public.sales_quotations 
                WHERE id = @Id;";

            var result = await connection.QueryFirstAsync<QuotationResponseDto>(quotationSql, new { Id = id });
            if (result == null)
                throw new KeyNotFoundException($"Quotation with ID {id} not found");

            return result;
        }

        public async Task<string?> GenerateQuotationHtmlAsync(int leadId)
        {
            // Fetch lead details
            var leadDetails = await _salesLeadService.GetLeadDetailsByIdAsync(leadId);
            if (leadDetails == null)
            {
                return null;
            }

            // Load HTML template
            var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "QuotationTemplate.html");
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("Quotation template not found.", templatePath);
            }
            var template = await File.ReadAllTextAsync(templatePath);

            // Generate quotation reference and date
            string quotationRef = $"QA/24-25/{leadId:D4}";
            string quotationDate = DateTime.UtcNow.ToString("dd.MM.yyyy");

            // Calculate total price - using Amount instead of Price            // Build product table rows - no products since they were removed
            var productRows = new StringBuilder();
            productRows.AppendLine(@"
                <tr>
                    <td style='border: 1px solid #ddd; padding: 8px; text-align: center;' colspan='3'>No products in this quotation</td>
                </tr>");

            // Set default values for price fields
            string formattedTotal = "0";
            string finalOfferPrice = "0";

            // Replace placeholders in the template
            var htmlContent = template
                .Replace("{{QuotationRef}}", quotationRef)
                .Replace("{{QuotationDate}}", quotationDate)
                .Replace("{{CustomerName}}", System.Web.HttpUtility.HtmlEncode(leadDetails.CustomerName ?? "N/A"))
                .Replace("{{CustomerAddress}}", System.Web.HttpUtility.HtmlEncode($"{leadDetails.Street ?? ""}, {leadDetails.City ?? ""}, {leadDetails.State ?? ""}, {leadDetails.Pincode ?? ""}"))
                .Replace("{{ProductRows}}", productRows.ToString())
                .Replace("{{TotalPrice}}", formattedTotal)
                .Replace("{{FinalOfferPrice}}", finalOfferPrice)
                .Replace("{{ContactMobile}}", System.Web.HttpUtility.HtmlEncode(leadDetails.ContactMobileNo ?? "N/A"))
                .Replace("{{ContactEmail}}", System.Web.HttpUtility.HtmlEncode(leadDetails.Email ?? "N/A"));

            return htmlContent;
        }
    }
}