using ERP.API.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.API.Services
{
    public class SalesDemoAssignmentService : BaseDataService<SalesDemoAssignment>, IDataService<SalesDemoAssignment>
    {
        public SalesDemoAssignmentService(IConfiguration configuration)
            : base(configuration.GetConnectionString("DefaultConnection")!, "demo_assignments")
        {
        }

        public SalesDemoAssignmentService(string connectionString)
            : base(connectionString, "demo_assignments")
        {
        }

        public async Task<SalesDemoAssignment?> GetByIdAsync(int id)
        {
            return await GetByIdInternalAsync(id);
        }

        private async Task<SalesDemoAssignment?> GetByIdInternalAsync(int id)
        {
            return await base.GetByIdAsync(id);
        }

        protected override string GenerateInsertQuery()
        {
            return @"INSERT INTO demo_assignments (
                user_created, date_created, user_updated, date_updated, demo_item_id, assigned_to_type, assigned_to_id, assignment_start_date, expected_return_date, actual_return_date, status
            ) VALUES (
                @UserCreated, @DateCreated, @UserUpdated, @DateUpdated, @DemoItemId, @AssignedToType, @AssignedToId, @AssignmentStartDate, @ExpectedReturnDate, @ActualReturnDate, @Status
            ) RETURNING id;";
        }

        protected override string GenerateUpdateQuery()
        {
            return @"UPDATE demo_assignments SET
                user_created = @UserCreated,
                date_created = @DateCreated,
                user_updated = @UserUpdated,
                date_updated = @DateUpdated,
                demo_item_id = @DemoItemId,
                assigned_to_type = @AssignedToType,
                assigned_to_id = @AssignedToId,
                assignment_start_date = @AssignmentStartDate,
                expected_return_date = @ExpectedReturnDate,
                actual_return_date = @ActualReturnDate,
                status = @Status
            WHERE id = @Id;";
        }
    }
}
