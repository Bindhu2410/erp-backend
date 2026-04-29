using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesTempLeadController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public SalesTempLeadController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM public.get_all_sales_temp_leads()", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            var result = new List<Dictionary<string, object>>();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.GetValue(i);
                result.Add(row);
            }
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM get_sales_temp_lead_by_id(@p_id)", conn);
            cmd.Parameters.AddWithValue("p_id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.GetValue(i);
                return Ok(row);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] List<Models.SalesTempLeadModel> models)
        {
            var ids = new List<object>();
            using var conn = GetConnection();
            await conn.OpenAsync();
            foreach (var model in models)
            {
                using var cmd = new NpgsqlCommand("SELECT insert_sales_temp_lead(@p_user_created, @p_customer_name, @p_lead_source, @p_lead_id, @p_status, @p_score, @p_isactive, @p_comments, @p_lead_type, @p_contact_name, @p_salutation, @p_contact_mobile_no, @p_land_line_no, @p_email, @p_door_no, @p_street, @p_landmark, @p_website, @p_area, @p_city, @p_pincode, @p_district, @p_state, @p_country)", conn);
                cmd.Parameters.AddWithValue("p_user_created", (object?)model.UserCreated ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_customer_name", (object?)model.CustomerName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_lead_source", (object?)model.LeadSource ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_lead_id", (object?)model.LeadId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_status", (object?)model.Status ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_score", (object?)model.Score ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_isactive", (object?)model.IsActive ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_comments", (object?)model.Comments ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_lead_type", (object?)model.LeadType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_contact_name", (object?)model.ContactName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_salutation", (object?)model.Salutation ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_contact_mobile_no", (object?)model.ContactMobileNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_land_line_no", (object?)model.LandLineNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_email", (object?)model.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_door_no", (object?)model.DoorNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_street", (object?)model.Street ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_landmark", (object?)model.Landmark ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_website", (object?)model.Website ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_area", (object?)model.Area ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_city", (object?)model.City ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_pincode", (object?)model.Pincode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_district", (object?)model.District ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_state", (object?)model.State ?? DBNull.Value);
                cmd.Parameters.AddWithValue("p_country", (object?)model.Country ?? DBNull.Value);
                var newId = await cmd.ExecuteScalarAsync();
                ids.Add(newId);
            }
            return Ok(new { ids });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Dictionary<string, object> model)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT update_sales_temp_lead(@p_id, @p_user_updated, @p_customer_name, @p_lead_source, @p_lead_id, @p_status, @p_score, @p_isactive, @p_comments, @p_lead_type, @p_contact_name, @p_salutation, @p_contact_mobile_no, @p_land_line_no, @p_email, @p_door_no, @p_street, @p_landmark, @p_website, @p_area, @p_city, @p_pincode, @p_district, @p_state, @p_country)", conn);
            cmd.Parameters.AddWithValue("p_id", id);
            cmd.Parameters.AddWithValue("p_user_updated", model["user_updated"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_customer_name", model["customer_name"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_lead_source", model["lead_source"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_lead_id", model["lead_id"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_status", model["status"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_score", model["score"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_isactive", model["isactive"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_comments", model["comments"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_lead_type", model["lead_type"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_contact_name", model["contact_name"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_salutation", model["salutation"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_contact_mobile_no", model["contact_mobile_no"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_land_line_no", model["land_line_no"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_email", model["email"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_door_no", model["door_no"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_street", model["street"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_landmark", model["landmark"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_website", model["website"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_area", model["area"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_city", model["city"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_pincode", model["pincode"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_district", model["district"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_state", model["state"] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_country", model["country"] ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT delete_sales_temp_lead(@p_id)", conn);
            cmd.Parameters.AddWithValue("p_id", id);
            await cmd.ExecuteNonQueryAsync();
            return NoContent();
        }
    }
}
