using System;
using System.IO;
using Npgsql;

class Program
{
    static void Main()
    {
        string connectionString = "Host=localhost;Port=5433;Username=postgres;Password=Magnus123$;Database=postgres";
        string scriptPath = "002_add_opportunity_id_to_sales_products.sql";

        try
        {
            string sql = File.ReadAllText(scriptPath);

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Console.WriteLine("Migration applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying migration: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
