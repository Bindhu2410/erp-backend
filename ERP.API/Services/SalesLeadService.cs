using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using Dapper;
using System.Data;
using ERP.API.Helpers;
using Microsoft.Extensions.Logging;

namespace ERP.API.Services
{
    public class SalesLeadService : BaseDataService<SalesLead>
    {
        private readonly ILogger<SalesLeadService> _logger;
        public SalesLeadService(string connectionString, ILogger<SalesLeadService> logger)
            : base(connectionString, "sales_lead")  // Fixed table name to match database
        {
            _logger = logger;
        }

        protected override string GenerateInsertQuery()
        {
            return @"
                INSERT INTO sales_lead (
                    user_created, date_created, user_updated, date_updated,
                    customer_name, lead_source, referral_source_name, hospital_of_referral,
                    department_of_referral, social_media, event_date,
                    event_name, lead_id, status, score, isactive, comments, lead_type,
                    contact_name, salutation, contact_mobile_no, land_line_no, email,
                    fax, door_no, street, landmark, website, territory, area, city,
                    district, state, pincode, assigned_to)
                VALUES (
                    @UserCreated, @DateCreated, @UserUpdated, @DateUpdated,
                    @CustomerName, @LeadSource, @ReferralSourceName, @HospitalOfReferral,
                    @DepartmentOfReferral, @SocialMedia, @EventDate,
                    @EventName, @LeadId, @Status, @Score, @IsActive, @Comments, @LeadType,
                    @ContactName, @Salutation, @ContactMobileNo, @LandLineNo, @Email,
                    @Fax, @DoorNo, @Street, @Landmark, @Website, @Territory, @Area, @City,
                    @District, @State, @Pincode, @AssignedTo)
                RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {            return @"
                UPDATE sales_lead SET
                    user_updated = @UserUpdated,
                    date_updated = CURRENT_TIMESTAMP,
                    customer_name = @CustomerName,
                    lead_source = @LeadSource,
                    referral_source_name = @ReferralSourceName,
                    hospital_of_referral = @HospitalOfReferral,
                    department_of_referral = @DepartmentOfReferral,
                    social_media = @SocialMedia,
                    event_date = @EventDate,
                    event_name = @EventName,
                    lead_id = @LeadId,
                    status = @Status,
                    score = @Score,
                    isactive = @IsActive,
                    comments = @Comments,
                    lead_type = @LeadType,
                    contact_name = @ContactName,
                    salutation = @Salutation,
                    contact_mobile_no = @ContactMobileNo,
                    land_line_no = @LandLineNo,
                    email = @Email,
                    fax = @Fax,
                    door_no = @DoorNo,
                    street = @Street,
                    landmark = @Landmark,
                    website = @Website,
                    territory = @Territory,
                    area = @Area,
                    city = @City,
                    district = @District,
                    state = @State,
                    pincode = @Pincode,
                    assigned_to = @AssignedTo
                WHERE id = @Id";
        }        public async Task<(IEnumerable<SalesLeadGridResult> Results, int TotalRecords)> GetSalesLeadsGridAsync(
            string? searchText = null,
            string[]? zones = null,
            string[]? customerNames = null,
            string[]? territories = null,
            string[]? statuses = null,
            string[]? scores = null,
            string[]? leadTypes = null,
            int? pageNumber = 1,
            int? pageSize = 10,
            string? orderBy = "id",
            string? orderDirection = "DESC",
            string[]? selectedLeadIds = null)
        {
            using var connection = CreateConnection();

            var parameters = new DynamicParameters();
            
            // If search text is provided, only apply the search text filter and ignore other filters
            if (!string.IsNullOrWhiteSpace(searchText) && searchText != "string")
            {
                parameters.Add("p_search_text", searchText);
                // Set other filters to null when search text is provided
                // parameters.Add("p_zones", null, DbType.Object); // removed
                parameters.Add("p_customer_names", null, DbType.Object);
                // parameters.Add("p_territories", null, DbType.Object); // removed
                parameters.Add("p_statuses", null, DbType.Object);
                parameters.Add("p_scores", null, DbType.Object);
                parameters.Add("p_lead_types", null, DbType.Object);
                parameters.Add("p_selected_lead_ids", null, DbType.Object);
            }
            else
            {
                // When no search text, apply the other filters
                parameters.Add("p_search_text", null);
                
                // Helper function to clean array parameters
                string[]? CleanArray(string[]? arr) => 
                    arr?.Length > 0 && !(arr.Length == 1 && arr[0] == "string") ? arr : null;

                // parameters.Add("p_zones", CleanArray(zones), DbType.Object); // removed
                parameters.Add("p_customer_names", CleanArray(customerNames), DbType.Object);
                // parameters.Add("p_territories", CleanArray(territories), DbType.Object); // removed
                parameters.Add("p_statuses", CleanArray(statuses), DbType.Object);
                parameters.Add("p_scores", CleanArray(scores), DbType.Object);
                parameters.Add("p_lead_types", CleanArray(leadTypes), DbType.Object);
                parameters.Add("p_selected_lead_ids", CleanArray(selectedLeadIds), DbType.Object);
            }

            parameters.Add("p_page_number", pageNumber);
            parameters.Add("p_page_size", pageSize);
            parameters.Add("p_order_by", string.IsNullOrEmpty(orderBy) ? "id" : orderBy);
            parameters.Add("p_order_direction", string.IsNullOrEmpty(orderDirection) ? "DESC" : orderDirection.ToUpper());

            _logger.LogInformation("Fetching leads with search text: {SearchText}, PageNumber: {PageNumber}, PageSize: {PageSize}",
                searchText, pageNumber, pageSize);

            var sql = @"SELECT * FROM sales_lead_grid(
                @p_search_text, 
                @p_customer_names, 
                @p_statuses, 
                @p_scores, 
                @p_lead_types, 
                @p_selected_lead_ids, 
                @p_page_number, 
                @p_page_size, 
                @p_order_by, 
                @p_order_direction
            )";

            var results = await connection.QueryAsync<SalesLeadGridResult>(sql, parameters);

            var resultsList = results.ToList();
            var totalRecords = resultsList.Any() ? resultsList.First().TotalRecords : 0;

            return (resultsList, totalRecords);
        }

        public async Task<(IEnumerable<SalesLeadGridResult> Results, int TotalRecords)> GetSalesLeadsGridByUserAsync(
            int currentUserId,
            string? searchText = null,
            string[]? zones = null,
            string[]? customerNames = null,
            string[]? territories = null,
            string[]? statuses = null,
            string[]? scores = null,
            string[]? leadTypes = null,
            int? pageNumber = 1,
            int? pageSize = 10,
            string? orderBy = "id",
            string? orderDirection = "DESC",
            string[]? selectedLeadIds = null)
        {
            using var connection = CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("p_current_user_id", currentUserId);
            
            // If search text is provided, only apply the search text filter and ignore other filters
            if (!string.IsNullOrWhiteSpace(searchText) && searchText != "string")
            {
                parameters.Add("p_search_text", searchText);
                // Set other filters to null when search text is provided
                parameters.Add("p_customer_names", null, DbType.Object);
                parameters.Add("p_statuses", null, DbType.Object);
                parameters.Add("p_scores", null, DbType.Object);
                parameters.Add("p_lead_types", null, DbType.Object);
                parameters.Add("p_selected_lead_ids", null, DbType.Object);
            }
            else
            {
                // When no search text, apply the other filters
                parameters.Add("p_search_text", null);
                
                // Helper function to clean array parameters
                string[]? CleanArray(string[]? arr) => 
                    arr?.Length > 0 && !(arr.Length == 1 && arr[0] == "string") ? arr : null;

                parameters.Add("p_customer_names", CleanArray(customerNames), DbType.Object);
                parameters.Add("p_statuses", CleanArray(statuses), DbType.Object);
                parameters.Add("p_scores", CleanArray(scores), DbType.Object);
                parameters.Add("p_lead_types", CleanArray(leadTypes), DbType.Object);
                parameters.Add("p_selected_lead_ids", CleanArray(selectedLeadIds), DbType.Object);
            }

            parameters.Add("p_page_number", pageNumber);
            parameters.Add("p_page_size", pageSize);
            parameters.Add("p_order_by", string.IsNullOrEmpty(orderBy) ? "id" : orderBy);
            parameters.Add("p_order_direction", string.IsNullOrEmpty(orderDirection) ? "DESC" : orderDirection.ToUpper());

            _logger.LogInformation("Fetching user-based leads for UserId: {UserId}, SearchText: {SearchText}, PageNumber: {PageNumber}, PageSize: {PageSize}",
                currentUserId, searchText, pageNumber, pageSize);

            var sql = @"SELECT * FROM sales_lead_grid(
                @p_current_user_id,
                @p_search_text, 
                @p_customer_names, 
                @p_statuses, 
                @p_scores, 
                @p_lead_types, 
                @p_selected_lead_ids, 
                @p_page_number, 
                @p_page_size, 
                @p_order_by, 
                @p_order_direction
            )";

            var results = await connection.QueryAsync<SalesLeadGridResult>(sql, parameters);

            var resultsList = results.ToList();
            var totalRecords = resultsList.Any() ? resultsList.First().TotalRecords : 0;

            return (resultsList, totalRecords);
        }        public override async Task<SalesLead?> GetByIdAsync(int? id)
        {
            if (!id.HasValue)
                return null;

            using var connection = CreateConnection();
            
            var query = "SELECT * FROM sales_lead WHERE id = @id";
            return await connection.QueryFirstOrDefaultAsync<SalesLead>(query, new { id = id.Value });
        }
         public async Task<SalesLeadDetails?> GetLeadDetailsByIdAsync(int id)
        {
            using var connection = CreateConnection();

            var query = @"
                SELECT 
    sl.*, 
    country.division_name AS Country,
    state.division_name AS State,
    district.division_name AS District,
    territory.division_name AS Territory,
    city.division_name AS City,
    area.division_name AS Area,
    pincode.division_name AS Pincode
FROM sales_lead sl

-- Join geographical divisions by type and ID
LEFT JOIN geographical_divisions country 
    ON country.division_name = sl.country 

LEFT JOIN geographical_divisions state 
    ON state.division_name = sl.state 

LEFT JOIN geographical_divisions district 
    ON district.division_name = sl.district 

LEFT JOIN geographical_divisions territory 
    ON sl.territory = territory.division_name 

LEFT JOIN geographical_divisions city 
    ON city.division_name = sl.city 

LEFT JOIN geographical_divisions area 
    ON area.division_name = sl.area 

LEFT JOIN geographical_divisions pincode 
    ON pincode.division_name = sl.pincode 

WHERE sl.id = @Id AND sl.status IS DISTINCT FROM 'Converted'";

            var lead = await connection.QueryFirstOrDefaultAsync<SalesLeadDetails>(query, new { Id = id });

            if (lead == null)
                return null;

            // Initialize collections to empty lists
            lead.Addresses = new List<SalesAddressDetails>();
            lead.Contacts = new List<SalesContact>();
            lead.BusinessChallenges = new List<SalesLeadsBusinessChallenge>();
            lead.Products = new List<SalesProducts>();

            // Get addresses
            var addressQuery = @"SELECT 
    sa.*,                    sc.name AS Country,
    ss.name AS State,
    sd.name AS District,
    st.name AS Territory,
    sct.name AS City,
    sa_area.name AS Area,
    p.pincode AS Pincode
FROM sales_addresses sa
LEFT JOIN sales_states ss ON sa.state = ss.name
LEFT JOIN sales_districts sd ON sa.district = sd.name
LEFT JOIN sales_countries sc ON sa.country = sc.name
LEFT JOIN sales_territories st ON sa.territory = st.name
LEFT JOIN sales_cities sct ON sa.city = sct.name
LEFT JOIN sales_areas sa_area ON sa.area = sa_area.name
LEFT JOIN pincodes p ON sa.pincode = p.pincode
WHERE sa.sales_leads_id = @id";
            var addresses = await connection.QueryAsync<SalesAddressDetails>(addressQuery, new { id });
            if (addresses.Any())
            {
                lead.Addresses = addresses.ToList();
            }

            // Get contacts
            var contactQuery = "SELECT * FROM sales_contacts WHERE sales_leads_id = @id";
            var contacts = await connection.QueryAsync<SalesContact>(contactQuery, new { id });
            if (contacts.Any())
            {
                lead.Contacts = contacts.ToList();
            }

            // Get business challenges
            var challengeQuery = "SELECT * FROM sales_leads_business_challenges WHERE sales_leads_id = @id";
            var challenges = await connection.QueryAsync<SalesLeadsBusinessChallenge>(challengeQuery, new { id });
            if (challenges.Any())
            {
                lead.BusinessChallenges = challenges.ToList();
            }

            // Get interest products
            var productQuery = @"SELECT 
    sp.*, 
    m.id AS MakeId,
    m.name AS MakeName,
    c.id AS CategoryId,
    c.name AS CategoryName,
    md.id AS ModelId,
    md.name AS ModelName,
    p.id AS ProductId,
    p.name AS ProductName,
    ii.item_code AS ItemCode,
    ii.item_name AS ItemName
FROM sales_products sp
LEFT JOIN inventory_items ii ON sp.inventory_items_id = ii.id
LEFT JOIN makes m ON ii.make_id = m.id
LEFT JOIN models md ON ii.model_id = md.id
LEFT JOIN categories c ON ii.category_id = c.id
LEFT JOIN products p ON ii.product_id = p.id
WHERE sp.stage_item_id = @id";
            var products = await connection.QueryAsync<SalesProducts>(productQuery, new { id });
            if (products.Any())
            {
                lead.Products = products.ToList();
            }

            return lead;
        }        public async Task<(IEnumerable<LeadsDropdownResult> Results, int TotalRecords)> GetLeadsDropdownAsync(
            string? searchText = null,
            int? pageNumber = 1,
            int? pageSize = 10)
        {
            using var connection = CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("p_search_text", string.IsNullOrWhiteSpace(searchText) ? null : searchText);
            parameters.Add("p_page_number", pageNumber ?? 1);
            parameters.Add("p_page_size", pageSize ?? 10);

            var results = await connection.QueryAsync<LeadsDropdownResult>(
                @"SELECT * FROM get_leads_dropdown(
                    p_search_text := @p_search_text,
                    p_page_number := @p_page_number,
                    p_page_size := @p_page_size)",
                parameters
            );

            var list = results.ToList();
            return (list, list.FirstOrDefault()?.TotalRecords ?? 0);
        }

        public async Task GenerateAndUpdateLeadIdAsync(SalesLead lead)
        {
            using var connection = CreateConnection();
            var sql = @"            SELECT lead_id 
            FROM sales_lead
            WHERE lead_id SIMILAR TO 'LEAD-[0-9]{3}'
            ORDER BY lead_id DESC 
            LIMIT 1";

            var lastId = await connection.QueryFirstOrDefaultAsync<string>(sql);
            
            int nextNumber;
            if (lastId == null)
            {
                nextNumber = 1;
            }
            else
            {
                // Extract the number part and increment
                if (int.TryParse(lastId.Substring(5), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
                else
                {
                    nextNumber = 1;
                }
            }

            // Format: LEAD- + 3 digits padded with zeros
            lead.LeadId = $"LEAD-{nextNumber.ToString().PadLeft(3, '0')}";
            await UpdateAsync(lead);
        }

        public async Task<SalesLeadDetails?> GetLeadDetailsByLeadIdAsync(string leadId)
        {
            using var connection = CreateConnection();
            var query = @"
                SELECT 
                    sl.*, 
                    sc.name AS Country,
                    ss.name AS State,
                    sd.name AS District,
                    sl.territory AS Territory,  -- Direct text field
                    sct.name AS City,
                    sa.name AS Area,
                    p.pincode AS Pincode                FROM sales_lead sl
                LEFT JOIN sales_states ss ON sl.state = ss.name
                LEFT JOIN sales_districts sd ON sl.district = sd.name
                LEFT JOIN sales_countries sc ON sl.country = sc.name
                LEFT JOIN sales_cities sct ON sl.city = sct.name
                LEFT JOIN sales_areas sa ON sl.area = sa.name
                LEFT JOIN pincodes p ON sl.pincode = p.pincode
                WHERE sl.lead_id = @LeadId AND sl.status IS DISTINCT FROM 'Converted'";

            var lead = await connection.QueryFirstOrDefaultAsync<SalesLeadDetails>(query, new { LeadId = leadId });

            if (lead == null)
                return null;

            // Initialize collections to empty lists
            lead.Addresses = new List<SalesAddressDetails>();
            lead.Contacts = new List<SalesContact>();
            lead.BusinessChallenges = new List<SalesLeadsBusinessChallenge>();
            lead.Products = new List<SalesProducts>();

            // Get addresses
            var addressQuery = @"SELECT 
                sa.*, 
                sc.name AS Country,
                ss.name AS State,                sd.name AS District,
                st.name AS Territory,
                sct.name AS City,
                sa_area.name AS Area,
                p.pincode AS Pincode
            FROM sales_addresses sa
            LEFT JOIN sales_states ss ON sa.state = ss.name
            LEFT JOIN sales_districts sd ON sa.district = sd.name
            LEFT JOIN sales_countries sc ON sa.country = sc.name
            LEFT JOIN sales_territories st ON sa.territory = st.name
            LEFT JOIN sales_cities sct ON sa.city = sct.name
            LEFT JOIN sales_areas sa_area ON sa.area = sa_area.name
            LEFT JOIN pincodes p ON sa.pincode = p.pincode
            WHERE sa.sales_leads_id = @id";
            var addresses = await connection.QueryAsync<SalesAddressDetails>(addressQuery, new { id = lead.Id });
            if (addresses.Any())
            {
                lead.Addresses = addresses.ToList();
            }

            // Get contacts
            var contactQuery = "SELECT * FROM sales_contacts WHERE sales_leads_id = @id";
            var contacts = await connection.QueryAsync<SalesContact>(contactQuery, new { id = lead.Id });
            if (contacts.Any())
            {
                lead.Contacts = contacts.ToList();
            }

            // Get business challenges
            var challengeQuery = "SELECT * FROM sales_leads_business_challenges WHERE sales_leads_id = @id";
            var challenges = await connection.QueryAsync<SalesLeadsBusinessChallenge>(challengeQuery, new { id = lead.Id });
            if (challenges.Any())
            {
                lead.BusinessChallenges = challenges.ToList();
            }

            // Get interest products
            var productQuery = @"
            SELECT
                sp.*,
                m.id AS MakeId,
                m.name AS MakeName,
                c.id AS CategoryId,
                c.name AS CategoryName,
                md.id AS ModelId,
                md.name AS ModelName,
                p.id AS ProductId,
                p.name AS ProductName,
                ii.item_code AS ItemCode,
                ii.item_name AS ItemName
            FROM sales_products sp
            LEFT JOIN inventory_items ii ON sp.inventory_items_id = ii.id
            LEFT JOIN makes m ON ii.make_id = m.id
            LEFT JOIN models md ON ii.model_id = md.id
            LEFT JOIN categories c ON ii.category_id = c.id
            LEFT JOIN products p ON ii.product_id = p.id
            WHERE sp.stage_item_id = @id";
        var products = await connection.QueryAsync<SalesProducts>(productQuery, new { id = lead.Id });
        if (products.Any())
        {
            lead.Products = products.ToList();
        }

        return lead;
    }

    public async Task<SalesLeadCardsDto> GetSalesLeadCardsAsync()
    {
        using var connection = CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<SalesLeadCardsDto>(
            "SELECT * FROM sp_get_lead_cards_count()"
        );
        return result ?? new SalesLeadCardsDto();
    }
        public async Task<LeadCardsDto> GetLeadCardsAsync()
        {
            using var connection = CreateConnection();
            var sql = @"SELECT 
                COUNT(*) AS ""TotalLeads"", 
                COUNT(*) FILTER (WHERE date_created >= date_trunc('week', CURRENT_DATE)) AS ""NewThisWeek"", 
                COUNT(*) FILTER (WHERE status = 'Qualified') AS ""QualifiedLeads"" 
            FROM public.sales_lead
            WHERE isactive = true;";
            var result = await connection.QueryFirstOrDefaultAsync<LeadCardsDto>(sql);
            return result ?? new LeadCardsDto();
        }

        public async Task<LeadCardsDto> GetLeadCardsByUserAsync(int userId)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT
                    COUNT(*)::bigint                                                                             AS ""TotalLeads"",
                    COUNT(*) FILTER (WHERE date_created >= date_trunc('week', CURRENT_DATE))::bigint            AS ""NewThisWeek"",
                    COUNT(*) FILTER (WHERE status = 'Qualified')::bigint                                        AS ""QualifiedLeads"",
                    COUNT(*) FILTER (WHERE status = 'Converted')::bigint                                        AS ""Converted"",
                    COUNT(*) FILTER (WHERE status = 'Lost')::bigint                                             AS ""Lost""
                FROM sales_lead
                WHERE isactive = true;";
            var result = await connection.QueryFirstOrDefaultAsync<LeadCardsDto>(sql);
            return result ?? new LeadCardsDto();
        }

        public async Task<bool> IsDuplicateLeadAsync(string contactMobileNo, string email)
        {
            using var connection = CreateConnection();
            try
            {
                // Use explicit column names without table alias to avoid ambiguity
                var query = @"SELECT COUNT(1) FROM sales_lead WHERE 
                    (@ContactMobileNo IS NOT NULL AND contact_mobile_no = @ContactMobileNo) OR 
                    (@Email IS NOT NULL AND email = @Email)";
                var count = await connection.ExecuteScalarAsync<int>(query, new { ContactMobileNo = contactMobileNo, Email = email });
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for duplicate lead with mobile: {Mobile}, email: {Email}", contactMobileNo, email);
                // Fallback to function-based approach if direct query fails
                try
                {
                    var result = await connection.QuerySingleAsync<bool>(
                        "SELECT check_duplicate_lead(@Mobile, @Email)",
                        new { Mobile = contactMobileNo, Email = email });
                    return result;
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Fallback duplicate check also failed");
                    throw;
                }
            }
        }

        public async Task<SalesLead?> GetByLeadIdAsync(string leadId)
        {
            using var connection = CreateConnection();
            var query = "SELECT * FROM sales_lead WHERE lead_id = @LeadId AND status IS DISTINCT FROM 'Converted' LIMIT 1";
            return await connection.QueryFirstOrDefaultAsync<SalesLead>(query, new { LeadId = leadId });
        }

        public async Task<SalesLead?> GetByQuotationIdAsync(string quotationId)
        {
            // The correct column is likely LeadId, not QuotationId
            using var connection = CreateConnection();
            var query = "SELECT * FROM sales_lead WHERE lead_id = @QuotationId AND status IS DISTINCT FROM 'Converted' LIMIT 1";
            return await connection.QueryFirstOrDefaultAsync<SalesLead>(query, new { QuotationId = quotationId });
        }

        public async Task<SalesLead?> GetByLatestQuotationIdAsync(string quotationId)
        {
            using var connection = CreateConnection();
            // Find the latest quotation by QuotationId
            var quotation = await connection.QueryFirstOrDefaultAsync<SalesQuotation>(
                "SELECT * FROM sales_quotations WHERE quotationid = @QuotationId ORDER BY date_created DESC LIMIT 1",
                new { QuotationId = quotationId });
            if (quotation == null || string.IsNullOrEmpty(quotation.LeadId))
                return null;
            // Now fetch the lead by LeadId
            var lead = await connection.QueryFirstOrDefaultAsync<SalesLead>(
                "SELECT * FROM sales_lead WHERE lead_id = @LeadId AND status IS DISTINCT FROM 'Converted' LIMIT 1",
                new { quotation.LeadId });
            return lead;
        }

        public override async Task<int> CreateAsync(SalesLead lead)
        {
            return await base.CreateAsync(lead);
        }
    }
}