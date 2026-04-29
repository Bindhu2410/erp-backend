        
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.API.Models;
using Dapper;

namespace ERP.API.Services
{
    public class SalesContactService : BaseDataService<SalesContact>
    {
        public SalesContactService(string connectionString) 
            : base(connectionString, "sales_contacts")
        {
        }

        public async Task<IEnumerable<ERP.API.Models.DTOs.ContactNameMobileDto>> GetContactNameMobileByOpportunityIdAsync(string opportunityId)
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM public.get_contact_by_opportunity_id(@OpportunityId)";
            var result = await connection.QueryAsync<ERP.API.Models.DTOs.ContactNameMobileDto>(sql, new { OpportunityId = opportunityId });
            return result ?? Enumerable.Empty<ERP.API.Models.DTOs.ContactNameMobileDto>();
        }

        protected override string GenerateInsertQuery()
        {
            return @"INSERT INTO sales_contacts (
                contact_name,
                department_name,
                specialist,
                degree,
                email,
                mobile_no,
                website,
                isactive,
                own_clinic,
                visiting_hours,
                clinic_visiting_hours,
                land_line_no,
                fax,
                salutation,
                job_title,
                is_default,
                sales_lead_id_custom,
                sales_lead_id,
                user_created,
                date_created,
                user_updated,
                date_updated
            ) VALUES (
                @ContactName,
                @DepartmentName,
                @Specialist,
                @Degree,
                @Email,
                @MobileNo,
                @Website,
                @IsActive,
                @OwnClinic,
                @VisitingHours,
                @ClinicVisitingHours,
                @LandLineNo,
                @Fax,
                @Salutation,
                @JobTitle,
                @IsDefault,
                @SalesLeadIdCustom,
                @SalesLeadId,
                @UserCreated,
                CURRENT_TIMESTAMP,
                @UserUpdated,
                CURRENT_TIMESTAMP
            ) RETURNING id";
        }

        protected override string GenerateUpdateQuery()
        {
            return @"UPDATE sales_contacts SET
                user_updated = @UserUpdated,
                date_updated = CURRENT_TIMESTAMP,
                contact_name = @ContactName,
                department_name = @DepartmentName,
                specialist = @Specialist,
                degree = @Degree,
                email = @Email,
                mobile_no = @MobileNo,
                website = @Website,
                isactive = @IsActive,
                own_clinic = @OwnClinic,
                visiting_hours = @VisitingHours,
                clinic_visiting_hours = @ClinicVisitingHours,
                land_line_no = @LandLineNo,
                fax = @Fax,
                salutation = @Salutation,
                job_title = @JobTitle,
                is_default = @IsDefault,
                sales_lead_id_custom = @SalesLeadIdCustom,
                sales_lead_id = @SalesLeadId
                WHERE id = @Id";
        }

        public async Task<IEnumerable<SalesContact>> GetBySalesLeadIdAsync(int salesLeadId)
        {
            return await GetAllAsync("sales_lead_id = @SalesLeadId", new { SalesLeadId = salesLeadId });
        }

        public async Task<IEnumerable<SalesContact>> GetBySalesLeadIdAsync(string salesLeadId)
        {
            return await GetAllAsync("sales_lead_id = @SalesLeadId", new { SalesLeadId = salesLeadId });
        }

        public async Task<bool> SetDefaultContactAsync(int contactId, int salesLeadId)
        {
            using var connection = CreateConnection();
            
            // First, set all contacts for this lead to non-default
            await connection.ExecuteAsync(
                $"UPDATE {_tableName} SET is_default = false WHERE sales_lead_id = @SalesLeadId",
                new { SalesLeadId = salesLeadId }
            );

            // Then set the specified contact as default
            var rowsAffected = await connection.ExecuteAsync(
                $"UPDATE {_tableName} SET is_default = true WHERE id = @Id AND sales_lead_id = @SalesLeadId",
                new { Id = contactId, SalesLeadId = salesLeadId }
            );
            
            return rowsAffected > 0;
        }
    }
}

