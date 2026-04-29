using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Dapper;
using Npgsql;
using ERP.API.Models;
using ERP.API.Models.DTOs;
using System.Linq;

namespace ERP.API.Services
{
    public interface ISalesQuotationService
    {
        Task<SalesQuotation?> GetByIdAsync(int id);
        Task<string?> GetPoIdByQuotationIdAsync(int quotationId);
        Task<string?> GetSalesOrderIdByQuotationIdAsync(int quotationId);
        Task<object?> GetDetailsByQuotationIdAsync(int quotationId);
        Task<object?> GetItemsByQuotationIdAsync(int quotationId);
        Task<object?> GetTermsAndConditionsByQuotationIdAsync(int quotationId);
        Task<object?> GetLeadAddressByQuotationIdAsync(int quotationId);
        Task InitializeDatabaseObjectsAsync(); // Add this for explicit initialization
    }

    public class SalesQuotationService : BaseDataService<SalesQuotation>, ISalesQuotationService
    {
        private bool _databaseObjectsInitialized = false;
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private readonly string _connectionString;

        public SalesQuotationService(string connectionString)
            : base(connectionString, "sales_quotations")
        {
            _connectionString = connectionString;
            // Don't initialize in constructor - use lazy initialization
        }

        /// <summary>
        /// Explicitly initialize database objects if needed
        /// </summary>
        public async Task InitializeDatabaseObjectsAsync()
        {
            await EnsureDatabaseObjectsInitializedAsync();
        }

        private async Task EnsureDatabaseObjectsInitializedAsync()
        {
            if (_databaseObjectsInitialized) return;

            await _initLock.WaitAsync();
            try
            {
                if (_databaseObjectsInitialized) return;

                await ValidateDatabaseObjectsAsync();
                _databaseObjectsInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        /// <summary>
        /// Patch (partial update) a sales quotation and its items.
        /// Only non-null fields in the request will be updated.
        /// </summary>
        public async Task<SalesQuotationWithItemsResponse?> PatchQuotationWithItemsAsync(int id, SalesQuotationWithItemsRequest request)
        {
            if (request == null || request.Quotation == null)
                throw new ArgumentException("Invalid patch request: request or quotation is null");

            await EnsureDatabaseObjectsInitializedAsync();

            using var connection = CreateConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Fetch existing quotation
                var existing = await connection.QueryFirstOrDefaultAsync<QuotationResponseDto>(
                    "SELECT * FROM sales_quotations WHERE id = @Id AND is_active = true", new { Id = id }, transaction);
                if (existing == null)
                    return null;

                // Patch only provided fields
                var patch = request.Quotation;
                var updateFields = new List<string>();
                var parameters = new DynamicParameters();
                parameters.Add("Id", id);

                void PatchField<T>(string field, T value) where T : class
                {
                    if (value != null)
                    {
                        updateFields.Add($"{field} = @{field}");
                        parameters.Add(field, value);
                    }
                }
                void PatchFieldStruct<T>(string field, T? value) where T : struct
                {
                    if (value.HasValue)
                    {
                        updateFields.Add($"{field} = @{field}");
                        parameters.Add(field, value);
                    }
                }

                // Patch fields with correct helper based on type
                PatchFieldStruct<int>("user_updated", patch.UserUpdated);
                PatchField<string>("version", patch.Version);
                PatchField<string>("terms", patch.Terms);
                PatchFieldStruct<DateTime>("valid_till", patch.ValidTill);
                PatchField<string>("quotation_for", patch.QuotationFor);
                PatchField<string>("status", patch.Status);
                PatchField<string>("lost_reason", patch.LostReason);
                PatchFieldStruct<int>("customer_id", patch.CustomerId);
                PatchField<string>("quotation_type", patch.QuotationType);
                PatchFieldStruct<DateTime>("quotation_date", patch.QuotationDate);
                PatchField<string>("order_type", patch.OrderType);
                PatchField<string>("comments", patch.Comments);
                PatchField<string>("delivery_within", patch.DeliveryWithin);
                PatchField<string>("delivery_after", patch.DeliveryAfter);
                PatchField<string>("quotation_id", patch.QuotationId);
                PatchField<string>("opportunity_id", patch.OpportunityId);
                PatchField<string>("lead_id", patch.LeadId);
                PatchField<string>("customer_name", patch.CustomerName);
                PatchFieldStruct<decimal>("taxes", patch.Taxes);
                PatchField<string>("delivery", patch.Delivery);
                PatchField<string>("payment", patch.Payment);
                PatchField<string>("warranty", patch.Warranty);
                PatchFieldStruct<decimal>("freight_charge", patch.FreightCharge);
                PatchFieldStruct<bool>("is_current", patch.IsCurrent);
                PatchFieldStruct<int>("parent_sales_quotations_id", patch.ParentSalesQuotationsId);
                PatchField<string>("contact_name", patch.ContactName);
                PatchField<string>("contact_mobile_no", patch.ContactMobileNo);
                PatchFieldStruct<int>("assigned_to", patch.AssignedTo);

                if (updateFields.Count > 0)
                {
                    var sql = $"UPDATE sales_quotations SET {string.Join(", ", updateFields)}, date_updated = NOW() WHERE id = @Id";
                    await connection.ExecuteAsync(sql, parameters, transaction);
                }

                // Items patching is not implemented (no sales_quotation_items table in current schema)

                await transaction.CommitAsync();

                // Return the updated quotation
                var updated = await GetQuotationWithItemsByIdAsync(id);
                return updated;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task ValidateDatabaseObjectsAsync()
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                // Check if table exists
                var tableExists = await connection.QuerySingleOrDefaultAsync<bool>(
                    @"SELECT EXISTS (
                        SELECT FROM information_schema.tables 
                        WHERE table_schema = 'public' 
                        AND table_name = 'sales_quotations'
                    );");

                if (!tableExists)
                {
                    throw new Exception("Table sales_quotations does not exist");
                }

                // Check if stored procedures exist
                var procedureNames = new[] {
                    "get_all_quotations",
                    "get_quotation_by_id",
                    "create_quotation",
                    "update_quotation",
                    "delete_quotation",
                    "get_quotations_by_opportunity",
                    "get_quotations_by_customer"
                };

                var existingProcedures = await connection.QueryAsync<string>(
                    @"SELECT proname 
                    FROM pg_proc 
                    WHERE proname = ANY(@names);",
                    new { names = procedureNames });

                var missingProcedures = procedureNames.Except(existingProcedures);
                if (missingProcedures.Any())
                {
                    await EnsureStoredProceduresAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating database objects: {ex.Message}", ex);
            }
        }

        private async Task EnsureStoredProceduresAsync()
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                // Use more careful dropping - only drop if they exist
                var dropSql = @"
                    DO $$
                    BEGIN
                        -- Drop functions if they exist
                        IF EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'get_all_quotations') THEN
                            DROP FUNCTION get_all_quotations();
                        END IF;

                        IF EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'get_quotation_by_id') THEN
                            DROP FUNCTION get_quotation_by_id(INT);
                        END IF;

                        IF EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'get_quotations_by_opportunity') THEN
                            DROP FUNCTION get_quotations_by_opportunity(INT);
                        END IF;

                        IF EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'get_quotations_by_customer') THEN
                            DROP FUNCTION get_quotations_by_customer(INT);
                        END IF;

                        IF EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'create_quotation') THEN
                            DROP FUNCTION create_quotation(
                                INT, VARCHAR, VARCHAR, TIMESTAMP WITH TIME ZONE, VARCHAR,
                                VARCHAR, VARCHAR, INT, VARCHAR, TIMESTAMP WITH TIME ZONE,
                                VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, INT, INT,
                                VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, INT);
                        END IF;

                        IF EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'update_quotation') THEN
                            DROP FUNCTION update_quotation(
                                INT, INT, VARCHAR, VARCHAR, TIMESTAMP WITH TIME ZONE, VARCHAR,
                                VARCHAR, VARCHAR, INT, VARCHAR, TIMESTAMP WITH TIME ZONE,
                                VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, INT,
                                VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, INT);
                        END IF;
                    END $$;
                ";
                await connection.ExecuteAsync(dropSql);

                // Create get_all_quotations
                var getAllSql = @"
                    CREATE OR REPLACE FUNCTION get_all_quotations()
                    RETURNS TABLE (
                        id INT,
                        user_created INT,
                        date_created TIMESTAMP,
                        user_updated INT,
                        date_updated TIMESTAMP,
                        version VARCHAR(255),
                        terms VARCHAR(255),
                        valid_till TIMESTAMP,
                        quotation_for VARCHAR(255),
                        status VARCHAR(255),
                        lost_reason VARCHAR(255),
                        customer_id INT,
                        quotation_type VARCHAR(255),
                        quotation_date TIMESTAMP,
                        order_type VARCHAR(255),
                        comments VARCHAR(255),
                        delivery_within VARCHAR(255),
                        delivery_after VARCHAR(255),
                        is_active BOOLEAN,
                        quotation_id VARCHAR(255),
                        opportunity_id INT,
                        customer_name VARCHAR(255),
                        taxes VARCHAR(255),
                        delivery VARCHAR(255),
                        payment VARCHAR(255),
                        warranty VARCHAR(255),
                        freight_charge VARCHAR(255),
                        is_current BOOLEAN,
                        parent_sales_quotations_id INT
                    ) AS $$
                    BEGIN    
                        RETURN QUERY 
                        SELECT 
                            sq.id,
                            sq.user_created,
                            sq.date_created,
                            sq.user_updated,
                            sq.date_updated,
                            sq.version,
                            sq.terms,
                            sq.valid_till,
                            sq.quotation_for,
                            sq.status,
                            sq.lost_reason,
                            sq.customer_id,
                            sq.quotation_type,
                            sq.quotation_date,
                            sq.order_type,
                            sq.comments,
                            sq.delivery_within,
                            sq.delivery_after,
                            sq.is_active,
                            sq.quotation_id,
                            sq.opportunity_id,
                            sq.customer_name,
                            sq.taxes,
                            sq.delivery,
                            sq.payment,
                            sq.warranty,
                            sq.freight_charge,
                            sq.is_current,
                            sq.parent_sales_quotations_id
                        FROM public.sales_quotations sq 
                        WHERE sq.is_active = true
                        ORDER BY sq.date_created DESC;
                    END;
                    $$ LANGUAGE plpgsql;
                ";
                await connection.ExecuteAsync(getAllSql);

                // Create get_quotation_by_id
                var getByIdSql = @"
                    CREATE OR REPLACE FUNCTION get_quotation_by_id(p_id INT)
                    RETURNS TABLE (
                        id INT,
                        user_created INT,
                        date_created TIMESTAMP,
                        user_updated INT,
                        date_updated TIMESTAMP,
                        version VARCHAR(255),
                        terms VARCHAR(255),
                        valid_till TIMESTAMP,
                        quotation_for VARCHAR(255),
                        status VARCHAR(255),
                        lost_reason VARCHAR(255),
                        customer_id INT,
                        quotation_type VARCHAR(255),
                        quotation_date TIMESTAMP,
                        order_type VARCHAR(255),
                        comments VARCHAR(255),
                        delivery_within VARCHAR(255),
                        delivery_after VARCHAR(255),
                        is_active BOOLEAN,
                        quotation_id VARCHAR(255),
                        opportunity_id INT,
                        customer_name VARCHAR(255),
                        taxes VARCHAR(255),
                        delivery VARCHAR(255),
                        payment VARCHAR(255),
                        warranty VARCHAR(255),
                        freight_charge VARCHAR(255),
                        is_current BOOLEAN,
                        parent_sales_quotations_id INT
                    ) AS $$
                    BEGIN    
                        RETURN QUERY 
                        SELECT 
                            sq.id,
                            sq.user_created,
                            sq.date_created,
                            sq.user_updated,
                            sq.date_updated,
                            sq.version,
                            sq.terms,
                            sq.valid_till,
                            sq.quotation_for,
                            sq.status,
                            sq.lost_reason,
                            sq.customer_id,
                            sq.quotation_type,
                            sq.quotation_date,
                            sq.order_type,
                            sq.comments,
                            sq.delivery_within,
                            sq.delivery_after,
                            sq.is_active,
                            sq.quotation_id,
                            sq.opportunity_id,
                            sq.customer_name,
                            sq.taxes,
                            sq.delivery,
                            sq.payment,
                            sq.warranty,
                            sq.freight_charge,
                            sq.is_current,
                            sq.parent_sales_quotations_id
                        FROM public.sales_quotations sq 
                        WHERE sq.id = p_id AND sq.is_active = true;
                    END;
                    $$ LANGUAGE plpgsql;
                ";
                await connection.ExecuteAsync(getByIdSql);

                // Create create_quotation stored procedure
                var createQuotationSql = @"
                    CREATE OR REPLACE FUNCTION create_quotation(
                        p_user_created INT,
                        p_version VARCHAR,
                        p_terms VARCHAR,
                        p_valid_till TIMESTAMP,
                        p_quotation_for VARCHAR,
                        p_status VARCHAR,
                        p_lost_reason VARCHAR,
                        p_customer_id INT,
                        p_quotation_type VARCHAR,
                        p_quotation_date TIMESTAMP,
                        p_order_type VARCHAR,
                        p_comments VARCHAR,
                        p_delivery_within VARCHAR,
                        p_delivery_after VARCHAR,
                        p_quotation_id VARCHAR,
                        p_opportunity_id INT,
                        p_customer_name VARCHAR,
                        p_taxes VARCHAR,
                        p_delivery VARCHAR,
                        p_payment VARCHAR,
                        p_warranty VARCHAR,
                        p_freight_charge VARCHAR,
                        p_parent_sales_quotations_id INT
                    )
                    RETURNS INT AS $$
                    DECLARE
                        new_id INT;
                    BEGIN
                        INSERT INTO sales_quotations (
                            user_created,
                            date_created,
                            version,
                            terms,
                            valid_till,
                            quotation_for,
                            status,
                            lost_reason,
                            customer_id,
                            quotation_type,
                            quotation_date,
                            order_type,
                            comments,
                            delivery_within,
                            delivery_after,
                            is_active,
                            quotation_id,
                            opportunity_id,
                            customer_name,
                            taxes,
                            delivery,
                            payment,
                            warranty,
                            freight_charge,
                            is_current,
                            parent_sales_quotations_id
                        )
                        VALUES (
                            p_user_created,
                            CURRENT_TIMESTAMP,
                            p_version,
                            p_terms,
                            p_valid_till,
                            p_quotation_for,
                            p_status,
                            p_lost_reason,
                            p_customer_id,
                            p_quotation_type,
                            p_quotation_date,
                            p_order_type,
                            p_comments,
                            p_delivery_within,
                            p_delivery_after,
                            true,
                            p_quotation_id,
                            p_opportunity_id,
                            p_customer_name,
                            p_taxes,
                            p_delivery,
                            p_payment,
                            p_warranty,
                            p_freight_charge,
                            true,
                            p_parent_sales_quotations_id
                        )
                        RETURNING id INTO new_id;

                        RETURN new_id;
                    END;
                    $$ LANGUAGE plpgsql;
                ";
                await connection.ExecuteAsync(createQuotationSql);

                // Create get_quotations_by_opportunity
                var getByOpportunitySql = @"
                    CREATE OR REPLACE FUNCTION get_quotations_by_opportunity(p_opportunity_id INT)
                    RETURNS TABLE (LIKE sales_quotations) AS $$
                    BEGIN    
                        RETURN QUERY 
                        SELECT *
                        FROM sales_quotations sq 
                        WHERE sq.opportunity_id = p_opportunity_id 
                        AND sq.is_active = true
                        ORDER BY sq.date_created DESC;
                    END;
                    $$ LANGUAGE plpgsql;
                ";
                await connection.ExecuteAsync(getByOpportunitySql);

                // Create get_quotations_by_customer
                var getByCustomerSql = @"
                    CREATE OR REPLACE FUNCTION get_quotations_by_customer(p_customer_id INT)
                    RETURNS TABLE (LIKE sales_quotations) AS $$
                    BEGIN    
                        RETURN QUERY 
                        SELECT *
                        FROM sales_quotations sq 
                        WHERE sq.customer_id = p_customer_id 
                        AND sq.is_active = true
                        ORDER BY sq.date_created DESC;
                    END;
                    $$ LANGUAGE plpgsql;
                ";
                await connection.ExecuteAsync(getByCustomerSql);

                // Create update_quotation stored procedure
                var updateQuotationSql = @"
                    CREATE OR REPLACE FUNCTION update_quotation(
                        p_id INT,
                        p_user_updated INT,
                        p_version VARCHAR(255),
                        p_terms VARCHAR(255),
                        p_valid_till TIMESTAMP WITH TIME ZONE,
                        p_quotation_for VARCHAR(255),
                        p_status VARCHAR(255),
                        p_lost_reason VARCHAR(255),
                        p_customer_id INT,
                        p_quotation_type VARCHAR(255),
                        p_quotation_date TIMESTAMP WITH TIME ZONE,
                        p_order_type VARCHAR(255),
                        p_comments VARCHAR(255),
                        p_delivery_within VARCHAR(255),
                        p_delivery_after VARCHAR(255),
                        p_quotation_id VARCHAR(255),
                        p_opportunity_id INT,
                        p_customer_name VARCHAR(255),
                        p_taxes VARCHAR(255),
                        p_delivery VARCHAR(255),
                        p_payment VARCHAR(255),
                        p_warranty VARCHAR(255),
                        p_freight_charge VARCHAR(255),
                        p_parent_sales_quotations_id INT
                    )
                    RETURNS BOOLEAN AS $$
                    DECLARE
                        updated_rows INT;
                        exists_but_inactive BOOLEAN;
                        exists_at_all BOOLEAN;
                    BEGIN
                        -- Check if the record exists at all
                        SELECT EXISTS(SELECT 1 FROM sales_quotations WHERE id = p_id)
                        INTO exists_at_all;

                        -- Check if record exists but is inactive
                        SELECT EXISTS(SELECT 1 FROM sales_quotations WHERE id = p_id AND is_active = false)
                        INTO exists_but_inactive;

                        -- Log the state
                        RAISE NOTICE 'Updating quotation ID: %, Exists: %, Is Inactive: %', 
                            p_id, exists_at_all, exists_but_inactive;

                        IF NOT exists_at_all THEN
                            RAISE EXCEPTION 'Quotation with ID % does not exist', p_id;
                        END IF;

                        IF exists_but_inactive THEN
                            RAISE EXCEPTION 'Quotation with ID % exists but is inactive', p_id;
                        END IF;

                        UPDATE sales_quotations 
                        SET 
                            user_updated = p_user_updated,
                            date_updated = CURRENT_TIMESTAMP,
                            version = COALESCE(NULLIF(p_version, ''), version),
                            terms = COALESCE(NULLIF(p_terms, ''), terms),
                            valid_till = COALESCE(p_valid_till, valid_till),
                            quotation_for = COALESCE(NULLIF(p_quotation_for, ''), quotation_for),
                            status = COALESCE(NULLIF(p_status, ''), status),
                            lost_reason = COALESCE(NULLIF(p_lost_reason, ''), lost_reason),
                            customer_id = COALESCE(p_customer_id, customer_id),
                            quotation_type = COALESCE(NULLIF(p_quotation_type, ''), quotation_type),
                            quotation_date = COALESCE(p_quotation_date, quotation_date),
                            order_type = COALESCE(NULLIF(p_order_type, ''), order_type),
                            comments = COALESCE(NULLIF(p_comments, ''), comments),
                            delivery_within = COALESCE(NULLIF(p_delivery_within, ''), delivery_within),
                            delivery_after = COALESCE(NULLIF(p_delivery_after, ''), delivery_after),
                            quotation_id = COALESCE(NULLIF(p_quotation_id, ''), quotation_id),
                            opportunity_id = COALESCE(p_opportunity_id, opportunity_id),
                            customer_name = COALESCE(NULLIF(p_customer_name, ''), customer_name),
                            taxes = COALESCE(NULLIF(p_taxes, ''), taxes),
                            delivery = COALESCE(NULLIF(p_delivery, ''), delivery),
                            payment = COALESCE(NULLIF(p_payment, ''), payment),
                            warranty = COALESCE(NULLIF(p_warranty, ''), warranty),
                            freight_charge = COALESCE(NULLIF(p_freight_charge, ''), freight_charge),
                            parent_sales_quotations_id = COALESCE(p_parent_sales_quotations_id, parent_sales_quotations_id)
                        WHERE id = p_id AND is_active = true
                        RETURNING 1 INTO updated_rows;

                        IF updated_rows = 0 THEN
                            RAISE EXCEPTION 'No rows were updated for quotation ID %', p_id;
                        END IF;

                        RETURN true;
                    END;
                    $$ LANGUAGE plpgsql;
                ";
                await connection.ExecuteAsync(updateQuotationSql);

                Console.WriteLine("Successfully created/updated stored procedures.");
            }
            catch (PostgresException pgEx) when (pgEx.SqlState == "23505")
            {
                // Ignore duplicate key violation - procedures already exist
                Console.WriteLine("Stored procedures already exist, skipping creation.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating stored procedures: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                // Don't throw - allow service to continue with existing procedures
            }
        }

        public override async Task<SalesQuotation?> GetByIdAsync(int? id)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            if (!id.HasValue || id.Value <= 0)
                throw new ArgumentException("Invalid quotation ID", nameof(id));

            try
            {
                using var connection = CreateConnection();
                var quotation = await connection.QuerySingleOrDefaultAsync<SalesQuotation>(
                    "SELECT * FROM get_quotation_by_id(@p_id);",
                    new { p_id = id.Value });

                return quotation;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving quotation with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<SalesQuotation?> GetByIdAsync(int id)
        {
            await EnsureDatabaseObjectsInitializedAsync();
            return await base.GetByIdAsync(id);
        }

        public override async Task<IEnumerable<SalesQuotation>> GetAllAsync()
        {
            await EnsureDatabaseObjectsInitializedAsync();

            try
            {
                using var connection = CreateConnection();
                var quotations = await connection.QueryAsync<SalesQuotation>(
                    "SELECT * FROM get_all_quotations();");

                return quotations ?? Enumerable.Empty<SalesQuotation>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving quotations: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<SalesQuotation>> GetQuotationsByOpportunityIdAsync(string opportunityId)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            if (string.IsNullOrEmpty(opportunityId))
                throw new ArgumentException("OpportunityId cannot be null or empty", nameof(opportunityId));

            try
            {
                using var connection = CreateConnection();
                var quotations = await connection.QueryAsync<SalesQuotation>(
                    "SELECT * FROM get_quotations_by_opportunity(@p_opportunity_id);",
                    new { p_opportunity_id = opportunityId });

                return quotations ?? Enumerable.Empty<SalesQuotation>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving quotations for opportunity {opportunityId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<SalesQuotation>> GetQuotationsByCustomerIdAsync(string customerId)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentException("CustomerId cannot be null or empty", nameof(customerId));

            if (!int.TryParse(customerId, out int custId))
                throw new ArgumentException("Invalid customer ID format", nameof(customerId));

            try
            {
                using var connection = CreateConnection();
                var quotations = await connection.QueryAsync<SalesQuotation>(
                    "SELECT * FROM get_quotations_by_customer(@p_customer_id);",
                    new { p_customer_id = custId });

                return quotations ?? Enumerable.Empty<SalesQuotation>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving quotations for customer {customerId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<SalesQuotation>> GetQuotationsByLeadIdAsync(string leadId)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            if (string.IsNullOrEmpty(leadId))
                throw new ArgumentException("LeadId cannot be null or empty", nameof(leadId));

            try
            {
                using var connection = CreateConnection();
                var quotations = await connection.QueryAsync<SalesQuotation>(
                    "SELECT * FROM sales_quotations WHERE lead_id = @p_lead_id AND is_active = true ORDER BY date_created DESC;",
                    new { p_lead_id = leadId });

                return quotations ?? Enumerable.Empty<SalesQuotation>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving quotations for lead {leadId}: {ex.Message}", ex);
            }
        }

        public override async Task<int> CreateAsync(SalesQuotation quotation)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            if (quotation == null)
                throw new ArgumentNullException(nameof(quotation));

            ValidateQuotation(quotation);

            // Validate user exists
            if (!await ValidateUserExists(quotation.UserCreated))
                throw new ArgumentException($"User with ID {quotation.UserCreated} does not exist", nameof(quotation.UserCreated));

            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                var parameters = new
                {
                    p_user_created = quotation.UserCreated.GetValueOrDefault(1),
                    p_version = quotation.Version,
                    p_terms = quotation.Terms,
                    p_valid_till = quotation.ValidTill ?? DateTime.UtcNow.AddDays(30),
                    p_quotation_for = quotation.QuotationFor,
                    p_status = quotation.Status ?? "Draft",
                    p_lost_reason = quotation.LostReason,
                    p_customer_id = quotation.CustomerId > 0 ? quotation.CustomerId : 0,
                    p_quotation_type = quotation.QuotationType,
                    p_quotation_date = quotation.QuotationDate ?? DateTime.UtcNow,
                    p_order_type = quotation.OrderType,
                    p_comments = quotation.Comments,
                    p_delivery_within = quotation.DeliveryWithin,
                    p_delivery_after = quotation.DeliveryPrepareAfter,
                    p_quotation_id = quotation.QuotationId,
                    p_opportunity_id = string.IsNullOrEmpty(quotation.OpportunityId) ? DBNull.Value : (object)quotation.OpportunityId,
                    p_customer_name = quotation.CustomerName,
                    p_taxes = quotation.Taxes,
                    p_delivery = quotation.Delivery,
                    p_payment = quotation.Payment,
                    p_warranty = quotation.Warranty,
                    p_freight_charge = quotation.FreightCharge,
                    p_parent_sales_quotations_id = (quotation.ParentSalesQuotationsId.HasValue && quotation.ParentSalesQuotationsId.Value > 0)
                        ? quotation.ParentSalesQuotationsId.Value
                        : (object)DBNull.Value
                };

                var sql = @"SELECT create_quotation(
                    @p_user_created, @p_version, @p_terms, @p_valid_till, @p_quotation_for,
                    @p_status, @p_lost_reason, @p_customer_id, @p_quotation_type, @p_quotation_date,
                    @p_order_type, @p_comments, @p_delivery_within, @p_delivery_after, @p_quotation_id,
                    @p_opportunity_id, @p_customer_name, @p_taxes, @p_delivery, @p_payment, @p_warranty,
                    @p_freight_charge, @p_parent_sales_quotations_id
                );";

                var id = await connection.QuerySingleAsync<int>(sql, parameters);

                if (id <= 0)
                    throw new Exception("Failed to create quotation - no ID returned");

                return id;
            }
            catch (PostgresException pgEx)
            {
                throw new Exception($"Database error creating quotation: {pgEx.Message}", pgEx);
            }
            catch (Exception ex)
            {
                // If it's our validation exception, throw it as is
                if (ex is ArgumentException)
                    throw;
                throw new Exception($"Error creating quotation: {ex.Message}", ex);
            }
        }

        public override async Task<bool> UpdateAsync(SalesQuotation quotation)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            if (quotation == null)
                throw new ArgumentNullException(nameof(quotation));

            if (!quotation.Id.HasValue)
                throw new ArgumentException("Quotation ID is required for update", nameof(quotation));

            ValidateQuotation(quotation);

            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                // First check the quotation's existence and status directly
                var quotationStatus = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT id, is_active 
                    FROM sales_quotations 
                    WHERE id = @Id",
                    new { Id = quotation.Id.Value });

                if (quotationStatus == null)
                {
                    throw new Exception($"Quotation with ID {quotation.Id.Value} does not exist");
                }

                if (!quotationStatus.is_active)
                {
                    throw new Exception($"Quotation with ID {quotation.Id.Value} is not active");
                }

                // Validate user exists
                if (!await ValidateUserExists(quotation.UserUpdated))
                    throw new ArgumentException($"User with ID {quotation.UserUpdated} does not exist", nameof(quotation.UserUpdated));

                var parameters = CreateParametersFromQuotation(quotation, quotation.CustomerId, quotation.OpportunityId);
                parameters.Add("p_id", quotation.Id.Value);

                // Log the update attempt and status
                Console.WriteLine($"Attempting to update quotation ID {quotation.Id.Value} with status: {quotation.Status}");
                Console.WriteLine($"Status check result - Exists: true, Is Active: {quotationStatus.is_active}");

                // Do a direct update instead of using the stored procedure
                var updateSql = @"
                    UPDATE sales_quotations 
                    SET 
                        user_updated = @p_user_updated,
                        date_updated = CURRENT_TIMESTAMP,
                        version = COALESCE(NULLIF(@p_version, ''), version),
                        terms = COALESCE(NULLIF(@p_terms, ''), terms),
                        valid_till = COALESCE(@p_valid_till, valid_till),
                        quotation_for = COALESCE(NULLIF(@p_quotation_for, ''), quotation_for),
                        status = COALESCE(NULLIF(@p_status, ''), status),
                        lost_reason = COALESCE(NULLIF(@p_lost_reason, ''), lost_reason),
                        customer_id = COALESCE(@p_customer_id, customer_id),
                        quotation_type = COALESCE(NULLIF(@p_quotation_type, ''), quotation_type),
                        quotation_date = COALESCE(@p_quotation_date, quotation_date),
                        order_type = COALESCE(NULLIF(@p_order_type, ''), order_type),
                        comments = COALESCE(NULLIF(@p_comments, ''), comments),
                        delivery_within = COALESCE(NULLIF(@p_delivery_within, ''), delivery_within),
                        delivery_after = COALESCE(NULLIF(@p_delivery_after, ''), delivery_after),
                        quotation_id = COALESCE(NULLIF(@p_quotation_id, ''), quotation_id),
                        opportunity_id = COALESCE(@p_opportunity_id, opportunity_id),
                        customer_name = COALESCE(NULLIF(@p_customer_name, ''), customer_name),
                        taxes = COALESCE(NULLIF(@p_taxes, ''), taxes),
                        delivery = COALESCE(NULLIF(@p_delivery, ''), delivery),
                        payment = COALESCE(NULLIF(@p_payment, ''), payment),
                        warranty = COALESCE(NULLIF(@p_warranty, ''), warranty),
                        freight_charge = COALESCE(NULLIF(@p_freight_charge, ''), freight_charge),
                        parent_sales_quotations_id = COALESCE(@p_parent_sales_quotations_id, parent_sales_quotations_id)
                    WHERE id = @p_id AND is_active = true;";

                var rowsAffected = await connection.ExecuteAsync(updateSql, parameters);
                Console.WriteLine($"Update result: {rowsAffected} rows affected");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // If it's our validation exception, throw it as is
                if (ex is ArgumentException)
                    throw;
                throw new Exception($"Error updating quotation: {ex.Message}", ex);
            }
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            try
            {
                using var connection = CreateConnection();
                var result = await connection.QuerySingleAsync<bool>(
                    "SELECT delete_quotation(@p_id, @p_user_updated);",
                    new { p_id = id, p_user_updated = 0 });

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting quotation {id}: {ex.Message}", ex);
            }
        }

        protected override string GenerateInsertQuery()
        {
            return @"SELECT create_quotation(
                @UserCreated,
                @Version,
                @Terms,
                @ValidTill,
                @QuotationFor,
                @Status,
                @LostReason,
                @CustomerId,
                @QuotationType,
                @QuotationDate,
                @OrderType,
                @Comments,
                @DeliveryWithin,
                @DeliveryPrepareAfter,
                @QuotationId,
                @OpportunityId,
                @CustomerName,
                @Taxes,
                @Delivery,
                @Payment,
                @Warranty,
                @FreightCharge,
                @ParentSalesQuotationsId
            );";
        }

        protected override string GenerateUpdateQuery()
        {
            return @"
                SELECT update_quotation(
                    @p_id,
                    @p_user_updated,
                    @p_version,
                    @p_terms,
                    @p_valid_till,
                    @p_quotation_for,
                    @p_status,
                    @p_lost_reason,
                    @p_customer_id,
                    @p_quotation_type,
                    @p_quotation_date,
                    @p_order_type,
                    @p_comments,
                    @p_delivery_within,
                    @p_delivery_after,
                    @p_quotation_id,
                    @p_opportunity_id,
                    @p_customer_name,
                    @p_taxes,
                    @p_delivery,
                    @p_payment,
                    @p_warranty,
                    @p_freight_charge,
                    @p_parent_sales_quotations_id
                );";
        }

        private void ValidateQuotation(SalesQuotation quotation)
        {
            // All fields are optional now
            // Provide default values if needed
            if (string.IsNullOrEmpty(quotation.Version))
                quotation.Version = "1.0";

            // Only allow certain statuses for update
            var allowedStatuses = new[] { "Draft", "Approved", "Submitted", "Final Quotation" };
            if (string.IsNullOrEmpty(quotation.Status))
            {
                quotation.Status = "Draft";
            }
            else if (!allowedStatuses.Contains(quotation.Status))
            {
                throw new ArgumentException($"Unsupported status for update. Only {string.Join(", ", allowedStatuses)} are allowed.");
            }

            if (!quotation.QuotationDate.HasValue)
                quotation.QuotationDate = DateTime.UtcNow;
        }

        private DynamicParameters CreateParametersFromQuotation(SalesQuotation quotation, int? customerId = null, string? opportunityId = null)
        {
            var parameters = new DynamicParameters();

            // Handle nullable int parameters with DBNull
            parameters.Add("p_user_updated", quotation.UserUpdated.HasValue ? (object)quotation.UserUpdated.Value : DBNull.Value);
            parameters.Add("p_version", (object)(quotation.Version?.Trim() ?? string.Empty));
            parameters.Add("p_terms", (object)(quotation.Terms?.Trim() ?? string.Empty));
            parameters.Add("p_valid_till", (object)(quotation.ValidTill ?? DateTime.UtcNow.AddMonths(1)));
            parameters.Add("p_quotation_for", (object)(quotation.QuotationFor?.Trim() ?? string.Empty));
            parameters.Add("p_status", (object)(quotation.Status?.Trim() ?? string.Empty));
            parameters.Add("p_lost_reason", (object)(quotation.LostReason?.Trim() ?? string.Empty));
            parameters.Add("p_customer_id", customerId.HasValue ? (object)customerId.Value : DBNull.Value);
            parameters.Add("p_quotation_type", (object)(quotation.QuotationType?.Trim() ?? string.Empty));
            parameters.Add("p_quotation_date", (object)(quotation.QuotationDate ?? DateTime.UtcNow));
            parameters.Add("p_order_type", (object)(quotation.OrderType?.Trim() ?? string.Empty));
            parameters.Add("p_comments", (object)(quotation.Comments?.Trim() ?? string.Empty));
            parameters.Add("p_delivery_within", (object)(quotation.DeliveryWithin?.Trim() ?? string.Empty));
            parameters.Add("p_delivery_after", (object)(quotation.DeliveryPrepareAfter?.Trim() ?? string.Empty));
            parameters.Add("p_quotation_id", (object)(quotation.QuotationId?.Trim() ?? string.Empty));
            parameters.Add("p_opportunity_id", string.IsNullOrEmpty(opportunityId) ? DBNull.Value : (object)opportunityId);
            parameters.Add("p_customer_name", (object)(quotation.CustomerName?.Trim() ?? string.Empty));
            parameters.Add("p_taxes", quotation.Taxes);
            parameters.Add("p_delivery", (object)(quotation.Delivery?.Trim() ?? string.Empty));
            parameters.Add("p_payment", (object)(quotation.Payment?.Trim() ?? string.Empty));
            parameters.Add("p_warranty", (object)(quotation.Warranty?.Trim() ?? string.Empty));
            parameters.Add("p_freight_charge", quotation.FreightCharge);
            parameters.Add("p_parent_sales_quotations_id", quotation.ParentSalesQuotationsId.HasValue ? (object)quotation.ParentSalesQuotationsId.Value : DBNull.Value);

            return parameters;
        }

        private async Task<bool> ValidateUserExists(int? userId)
        {
            if (!userId.HasValue)
                return false;

            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();

                var exists = await connection.QuerySingleOrDefaultAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM users WHERE user_id = @UserId)",
                    new { UserId = userId });

                return exists;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<SalesQuotationCardDto>> GetQuotationCardsCountAsync()
        {
            await EnsureDatabaseObjectsInitializedAsync();

            var statuses = new[] { "Draft", "Pending Approval", "Approved", "Rejected", "Sent" };
            using var connection = CreateConnection();
            var result = new List<SalesQuotationCardDto>();
            foreach (var status in statuses)
            {
                var count = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM sales_quotations WHERE status = @status AND is_active = true;",
                    new { status });
                if (count > 0)
                {
                    result.Add(new SalesQuotationCardDto { Status = status, Count = count });
                }
            }
            return result;
        }

        public async Task<SalesQuotationCardsDto> GetSalesQuotationCardsAsync()
        {
            await EnsureDatabaseObjectsInitializedAsync();

            using var connection = CreateConnection();
            var sql = @"SELECT
                COUNT(*) FILTER (WHERE status = 'Draft' AND is_active = true) AS Draft,
                COUNT(*) FILTER (WHERE status = 'Pending Approval' AND is_active = true) AS PendingApproval,
                COUNT(*) FILTER (WHERE status = 'Approved' AND is_active = true) AS Approved,
                COUNT(*) FILTER (WHERE status = 'Rejected' AND is_active = true) AS Rejected,
                COUNT(*) FILTER (WHERE status = 'Sent' AND is_active = true) AS Sent
            FROM public.sales_quotations;";
            var result = await connection.QueryFirstOrDefaultAsync<SalesQuotationCardsDto>(sql);
            return result ?? new SalesQuotationCardsDto();
        }

        // Get per-user quotation cards similar to how lead cards are handled in SalesLeadService
        public async Task<SalesQuotationCardsDto> GetSalesQuotationCardsByUserAsync(int userId)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            using var connection = CreateConnection();
            // Count quotations where the user is either the assignee or creator to match expected UI behavior
            var sql = @"SELECT
                COUNT(*) FILTER (WHERE status = 'Draft' AND is_active = true AND (assigned_to = @UserId OR user_created = @UserId)) AS Draft,
                COUNT(*) FILTER (WHERE status = 'Submitted' AND is_active = true AND (assigned_to = @UserId OR user_created = @UserId)) AS Submitted,
                COUNT(*) FILTER (WHERE status = 'Final Quotation' AND is_active = true AND (assigned_to = @UserId OR user_created = @UserId)) AS FinalQuotation,
                COUNT(*) FILTER (WHERE status = 'Approved' AND is_active = true AND (assigned_to = @UserId OR user_created = @UserId)) AS Approved,
                COUNT(*) FILTER (WHERE status = 'Cancelled' AND is_active = true AND (assigned_to = @UserId OR user_created = @UserId)) AS Cancelled,
                COUNT(*) FILTER (WHERE status = 'Negotiation' AND is_active = true AND (assigned_to = @UserId OR user_created = @UserId)) AS Negotiation
            FROM public.sales_quotations;";
            var result = await connection.QueryFirstOrDefaultAsync<SalesQuotationCardsDto>(sql, new { UserId = userId });
            return result ?? new SalesQuotationCardsDto();
        }

        public async Task<int> GetQuotationCountByStatusAsync(string statusName)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            using var connection = CreateConnection();
            var sql = "SELECT COUNT(*) FROM sales_quotations WHERE status = @statusName AND is_active = true;";
            return await connection.ExecuteScalarAsync<int>(sql, new { statusName });
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }

        public async Task<IEnumerable<SalesQuotationWithItemsResponse>> GetQuotationsWithItemsAsync()
        {
            await EnsureDatabaseObjectsInitializedAsync();

            using var connection = CreateConnection();
            await connection.OpenAsync();
            var quotations = (await connection.QueryAsync<QuotationResponseDto>(@"
                SELECT user_created, date_created, user_updated, date_updated, id, quotation_type, order_type, 
                       quotation_date, status, version, terms, valid_till, quotation_for, lost_reason, customer_id, 
                       comments, delivery_within, delivery_after, is_active, quotation_id, opportunity_id, lead_id, 
                       CASE WHEN taxes ~ '^[0-9]+$' THEN taxes::int ELSE NULL END as taxes, 
                       delivery, payment, warranty, 
                       CASE WHEN freight_charge ~ '^[0-9]+\.?[0-9]*$' THEN freight_charge::decimal ELSE NULL END as freight_charge, 
                       is_current, parent_sales_quotations_id, 
                       customer_name, contact_name, contact_mobile_no
                FROM sales_quotations 
                WHERE is_active = true 
                ORDER BY date_created DESC")).ToList();
            var result = new List<SalesQuotationWithItemsResponse>();
            foreach (var quotation in quotations)
            {
                // Return quotations without items since sales_quotation_items table doesn't exist
                result.Add(new SalesQuotationWithItemsResponse
                {
                    Quotation = quotation,
                    Items = new List<object>() // Empty list instead of querying non-existent table
                });
            }
            return result;
        }

        public async Task<SalesQuotationWithItemsResponse> GetQuotationWithItemsByIdAsync(int id)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            using var connection = CreateConnection();
            await connection.OpenAsync();
            var quotation = await connection.QueryFirstOrDefaultAsync<QuotationResponseDto>(@"
                SELECT user_created, date_created, user_updated, date_updated, id, quotation_type, order_type, 
                       quotation_date, status, version, terms, valid_till, quotation_for, lost_reason, customer_id, 
                       comments, delivery_within, delivery_after, is_active, quotation_id, opportunity_id, lead_id, 
                       CASE WHEN taxes ~ '^[0-9]+$' THEN taxes::int ELSE NULL END as taxes, 
                       delivery, payment, warranty, 
                       CASE WHEN freight_charge ~ '^[0-9]+\.?[0-9]*$' THEN freight_charge::decimal ELSE NULL END as freight_charge, 
                       is_current, parent_sales_quotations_id, 
                       customer_name, contact_name, contact_mobile_no
                FROM sales_quotations 
                WHERE id = @Id", new { Id = id });
            if (quotation == null) return null;

            // Return quotation without items since sales_quotation_items table doesn't exist
            return new SalesQuotationWithItemsResponse
            {
                Quotation = quotation,
                Items = new List<object>() // Empty list instead of querying non-existent table
            };
        }

        public async Task<SalesQuotationWithItemsResponse?> GetLatestQuotationWithItemsByLeadIdAsync(string leadId)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            using var connection = CreateConnection();
            await connection.OpenAsync();
            var quotation = await connection.QueryFirstOrDefaultAsync<QuotationResponseDto>(@"
                SELECT user_created, date_created, user_updated, date_updated, id, quotation_type, order_type, 
                       quotation_date, status, version, terms, valid_till, quotation_for, lost_reason, customer_id, 
                       comments, delivery_within, delivery_after, is_active, quotation_id, opportunity_id, lead_id, 
                       CASE WHEN taxes ~ '^[0-9]+$' THEN taxes::int ELSE NULL END as taxes, 
                       delivery, payment, warranty, 
                       CASE WHEN freight_charge ~ '^[0-9]+\.?[0-9]*$' THEN freight_charge::decimal ELSE NULL END as freight_charge, 
                       is_current, parent_sales_quotations_id, 
                       customer_name, contact_name, contact_mobile_no
                FROM sales_quotations 
                WHERE is_active = true AND lead_id = @LeadId 
                ORDER BY date_created DESC LIMIT 1",
                new { LeadId = leadId });
            if (quotation == null) return null;

            // Return only quotation info without items
            return new SalesQuotationWithItemsResponse
            {
                Quotation = quotation,
                Items = new List<object>(), // Empty list instead of null
                CustomerName = quotation.CustomerName
            };
        }

        // Stub: Fetch PO ID by Quotation ID
        public async Task<string?> GetPoIdByQuotationIdAsync(int quotationId)
        {
            await EnsureDatabaseObjectsInitializedAsync();
            // TODO: Implement actual DB logic
            return null;
        }

        // Stub: Fetch Sales Order ID by Quotation ID
        public async Task<string?> GetSalesOrderIdByQuotationIdAsync(int quotationId)
        {
            await EnsureDatabaseObjectsInitializedAsync();
            // TODO: Implement actual DB logic
            return null;
        }

        // Stub: Fetch Quotation Details
        public async Task<object?> GetDetailsByQuotationIdAsync(int quotationId)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                var sql = "SELECT * FROM sales_quotations WHERE id = @Id LIMIT 1;";
                var quotation = await connection.QueryFirstOrDefaultAsync(sql, new { Id = quotationId });
                return quotation;
            }
        }

        // Stub: Fetch Quotation Items
        public async Task<object?> GetItemsByQuotationIdAsync(int quotationId)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();

                // Get quotation items from sales_product with BOM details
                var quotationItems = await connection.QueryAsync<dynamic>(@"
                    SELECT sp.id, sp.bom_id, sp.qty, sp.bom_child_item_ids, sp.bom_accessory_item_ids,
                           bom.bom_name, bom.bom_type, bom.quote_title_id, bom.tc_template_id
                    FROM sales_product sp
                    LEFT JOIN bill_of_materials bom ON sp.bom_id = bom.bom_id
                    WHERE sp.stage = 'Quotation' AND sp.stage_item_id = @QuotationId",
                    new { QuotationId = quotationId.ToString() });

                var items = new List<object>();
                int? responseQuoteTitleId = null;
                int? responseTcTemplateId = null;
                
                foreach (var qItem in quotationItems)
                {
                    // Skip items with empty bomId or bomName
                    if (string.IsNullOrWhiteSpace(qItem.bom_id?.ToString()) || 
                        string.IsNullOrWhiteSpace(qItem.bom_name?.ToString()))
                        continue;
                    
                    // Extract quote title ID and TC template ID from first valid item
                    if (responseQuoteTitleId == null && qItem.quote_title_id != null)
                        responseQuoteTitleId = qItem.quote_title_id;
                    if (responseTcTemplateId == null && qItem.tc_template_id != null)
                        responseTcTemplateId = qItem.tc_template_id;
                    // Parse bom_child_item_ids and bom_accessory_item_ids from JSONB
                    int[] bomChildItemIds = null;
                    int[] bomAccessoryItemIds = null;

                    try
                    {
                        bomChildItemIds = qItem.bom_child_item_ids != null ?
                            System.Text.Json.JsonSerializer.Deserialize<int[]>(qItem.bom_child_item_ids.ToString()) :
                            Array.Empty<int>();
                    }
                    catch
                    {
                        bomChildItemIds = Array.Empty<int>();
                    }

                    try
                    {
                        bomAccessoryItemIds = qItem.bom_accessory_item_ids != null ?
                            System.Text.Json.JsonSerializer.Deserialize<int[]>(qItem.bom_accessory_item_ids.ToString()) :
                            Array.Empty<int>();
                    }
                    catch
                    {
                        bomAccessoryItemIds = Array.Empty<int>();
                    }

                    // Fetch BOM child items with full details
                    var bomChildItemList = new List<dynamic>();
                    if (bomChildItemIds.Length > 0)
                    {
                        var childQuery = @"
                            SELECT im.id, im.item_name, im.item_code, im.unit_price, im.hsn, im.tax_percentage, 
                                   c.name as category_name, u.code as uom_name,
                                   m.name as make, mo.name as model, p.name as product,
                                   vm.name as valuation_method_name, ime.name as inventory_method_name, itt.name as inventory_type_name,
                                   rmi.purchase_rate, rmi.sales_rate as sale_rate, rmi.quotation_rate as quote_rate,
                                   bci.quantity, bci.child_item_id
                            FROM bill_of_material_child_items bci
                            JOIN item_master im ON bci.child_item_id = im.id
                            LEFT JOIN categories c ON im.category_id = c.id
                            LEFT JOIN uom u ON im.uom_id = u.id
                            LEFT JOIN make m ON im.make_id = m.id
                            LEFT JOIN model mo ON im.model_id = mo.id
                            LEFT JOIN product p ON im.product_id = p.id
                            LEFT JOIN valuation_method vm ON im.valuation_method_id = vm.id
                            LEFT JOIN inventory_method ime ON im.inventory_method_id = ime.id
                            LEFT JOIN inventory_types itt ON im.group_id = itt.id
                            LEFT JOIN rate_master_items rmi ON im.id = rmi.item_id
                            WHERE bci.bill_of_material_id = (SELECT id FROM bill_of_materials WHERE bom_id = @BomId LIMIT 1)";
                        var rawChildItems = await connection.QueryAsync<dynamic>(childQuery, new { BomId = qItem.bom_id });
                        bomChildItemList = rawChildItems.Select(ci => new
                        {
                            childItemId = ci.child_item_id,
                            quantity = ci.quantity,
                            make = ci.make,
                            model = ci.model,
                            product = ci.product,
                            categoryName = ci.category_name,
                            valuationMethodName = ci.valuation_method_name,
                            inventoryMethodName = ci.inventory_method_name,
                            inventoryTypeName = ci.inventory_type_name,
                            unitPrice = ci.unit_price,
                            itemName = ci.item_name,
                            itemCode = ci.item_code,
                            catNo = ci.cat_no ?? "",
                            uomName = ci.uom_name,
                            purchaseRate = ci.purchase_rate,
                            saleRate = ci.sale_rate,
                            quoteRate = ci.quote_rate,
                            hsn = ci.hsn,
                            tax = ci.tax_percentage == -1 ? 0 : ci.tax_percentage
                        }).ToList<dynamic>();
                    }

                    // Fetch accessory items with full details
                    var accessoryItemList = new List<dynamic>();
                    if (bomAccessoryItemIds.Length > 0)
                    {
                        var accessoryQuery = @"
                            SELECT im.id, im.item_name, im.item_code, im.unit_price, im.hsn, im.tax_percentage, 
                                   c.name as category_name, u.code as uom,
                                   m.name as make, mo.name as model, p.name as product
                            FROM item_master im
                            LEFT JOIN categories c ON im.category_id = c.id
                            LEFT JOIN uom u ON im.uom_id = u.id
                            LEFT JOIN make m ON im.make_id = m.id
                            LEFT JOIN model mo ON im.model_id = mo.id
                            LEFT JOIN product p ON im.product_id = p.id
                            WHERE im.id = ANY(@Ids)";
                        var rawAccessoryItems = await connection.QueryAsync<dynamic>(accessoryQuery, new { Ids = bomAccessoryItemIds });
                        accessoryItemList = rawAccessoryItems.Select(ai => new
                        {
                            id = ai.id,
                            make = ai.make,
                            model = ai.model,
                            product = ai.product,
                            itemName = ai.item_name,
                            itemCode = ai.item_code,
                            unitPrice = ai.unit_price,
                            hsn = ai.hsn,
                            taxPercentage = ai.tax_percentage,
                            categoryName = ai.category_name,
                            uom = ai.uom
                        }).ToList<dynamic>();
                    }

                    // Only add items that have either childItems or accessoryItems with content
                    if (bomChildItemList.Count > 0 || accessoryItemList.Count > 0)
                    {
                        items.Add(new
                        {
                            bomId = qItem.bom_id?.ToString() ?? "",
                            bomName = qItem.bom_name?.ToString() ?? "",
                            bomType = qItem.bom_type?.ToString() ?? "",
                            childItems = bomChildItemList,
                            accessoryItemIds = bomAccessoryItemIds,
                            accessoryItems = accessoryItemList,
                            quantity = qItem.qty ?? 1
                        });
                    }
                }

                return new
                {
                    items = items,
                    quoteTitleId = responseQuoteTitleId,
                    tcTemplateId = responseTcTemplateId
                };
            }
        }

        public async Task<object?> GetTermsAndConditionsByQuotationIdAsync(int quotationId)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            using var connection = CreateConnection();
            await connection.OpenAsync();
            var termsAndConditions = await connection.QueryFirstOrDefaultAsync(
                "SELECT * FROM sales_terms_and_conditions WHERE quotation_id = @QuotationId ORDER BY id DESC LIMIT 1",
                new { QuotationId = quotationId });
            return termsAndConditions;
        }

        public async Task<object?> GetLeadAddressByQuotationIdAsync(int quotationId)
        {
            await EnsureDatabaseObjectsInitializedAsync();

            using var connection = CreateConnection();
            await connection.OpenAsync();
            var leadAddress = await connection.QueryFirstOrDefaultAsync(@"
                SELECT sl.pincode, sl.area, sl.state, sl.district, sl.city, sl.door_no, sl.street, sl.landmark
                FROM sales_quotations sq
                JOIN sales_opportunities so ON sq.opportunity_id = so.opportunity_id
                JOIN sales_lead sl ON so.lead_id = sl.lead_id
                WHERE sq.id = @QuotationId
                LIMIT 1",
                new { QuotationId = quotationId });
            return leadAddress;
        }
    }
}