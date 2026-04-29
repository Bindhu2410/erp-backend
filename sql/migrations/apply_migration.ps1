# Apply database migration script
$connectionParams = "Host=localhost;Port=5433;Username=postgres;Password=Magnus123$;Database=postgres"

# Create a temporary file with PGPASSWORD to avoid password prompt
$env:PGPASSWORD = "Magnus123$"

try {
    # Execute the migration script
    $scriptPath = Join-Path $PSScriptRoot "002_add_opportunity_id_to_sales_products.sql"
    if (Test-Path $scriptPath) {
        Write-Host "Executing migration script: $scriptPath"
        Get-Content $scriptPath | & psql -h localhost -p 5433 -U postgres -d postgres
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Migration completed successfully"
        } else {
            Write-Error "Migration failed with exit code $LASTEXITCODE"
        }
    } else {
        Write-Error "Migration script not found at: $scriptPath"
    }
} catch {
    Write-Error "Error executing migration: $_"
} finally {
    # Clear the password from environment
    Remove-Item Env:\PGPASSWORD
}
