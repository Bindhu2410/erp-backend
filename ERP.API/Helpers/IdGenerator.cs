using System;
using System.Threading.Tasks;
using Dapper;
using System.Data;

namespace ERP.API.Helpers
{
    public static class IdGenerator
    {
        private const string LeadPrefix = "LEAD-";
        private const string OpportunityPrefix = "OPP";
        private const string DemoPrefix = "DM";
        private const int SequenceLength = 3;

        public static async Task<string> GenerateLeadId(IDbConnection connection)
        {
            // Get the current max lead_id that follows our format (LEAD- + 3 digits)
            const string sql = @"
                SELECT lead_id 
                FROM sales_leads 
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
            return $"{LeadPrefix}{nextNumber.ToString().PadLeft(SequenceLength, '0')}";
        }

        public static async Task<string> GenerateOpportunityId(IDbConnection connection)
        {
            // Get the current max opportunity_id that follows our format (OPP + 5 digits)
            const string sql = @"
                SELECT opportunity_id 
                FROM sales_opportunities 
                WHERE opportunity_id SIMILAR TO 'OPP[0-9]{5}'
                ORDER BY opportunity_id DESC 
                LIMIT 1";

            string? newId = null;
            int maxAttempts = 10;
            int attempt = 0;
            while (attempt < maxAttempts)
            {
                var lastId = await connection.QueryFirstOrDefaultAsync<string>(sql);
                int nextNumber = 1;
                if (lastId != null && int.TryParse(lastId.Substring(3), out int lastNumber))
                    nextNumber = lastNumber + 1;
                // Format: OPP + 5 digits padded with zeros
                newId = $"{OpportunityPrefix}{nextNumber.ToString().PadLeft(5, '0')}";
                // Check if this ID already exists
                var exists = await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT opportunity_id FROM sales_opportunities WHERE opportunity_id = @id", new { id = newId });
                if (string.IsNullOrEmpty(exists))
                    break; // Unique!
                attempt++;
            }
            if (newId == null)
                throw new Exception("Failed to generate unique OpportunityId after multiple attempts.");
            return newId;
        }

        public static async Task<string> GenerateDemoId(IDbConnection connection)
        {
            // Get the current max demo_id that follows our format (DM + 5 digits)
            const string sql = @"
                SELECT demo_id 
                FROM sales_demos 
                WHERE demo_id SIMILAR TO 'DM[0-9]{5}'
                ORDER BY demo_id DESC 
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
                if (int.TryParse(lastId.Substring(2), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
                else
                {
                    nextNumber = 1;
                }
            }

            // Format: DM + 5 digits padded with zeros
            return $"{DemoPrefix}{nextNumber.ToString().PadLeft(SequenceLength, '0')}";
        }
    }
}
