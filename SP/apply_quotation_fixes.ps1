$env:PGPASSWORD = "Magnus123$"
$pgHost = "localhost"
$port = "5433"
$database = "postgres"
$username = "postgres"
$psqlPath = "C:\Program Files\PostgreSQL\16\bin\psql.exe"

# First, drop existing functions to avoid return type conflicts
$dropFunctions = @"
DROP FUNCTION IF EXISTS create_quotation(
    integer, varchar, varchar, timestamp, varchar, varchar, varchar,
    integer, varchar, timestamp, varchar, varchar, varchar, varchar,
    boolean, varchar, integer, integer, varchar, varchar, varchar,
    varchar, varchar, varchar, boolean, integer
);
"@

Write-Host "Dropping existing functions..."
$dropFunctions | & $psqlPath -h $pgHost -p $port -U $username -d $database

Write-Host "Applying stored procedure fixes..."

# Run QuotationsSp.sql and create_quotation.sql
try {
    Get-Content ".\QuotationsSp.sql", ".\create_quotation.sql" | & $psqlPath -h $pgHost -p $port -U $username -d $database
    Write-Host "Successfully applied stored procedure fixes."
} catch {
    Write-Host "Error applying stored procedure fixes: $_"
    exit 1
}

Write-Host "Done."
