param(
    [string]$ConnectionString = "Host=localhost;Port=5433;Username=postgres;Password=Magnus123`$;Database=postgres"
)

Write-Host "Applying sales_lead_grid functions fix..." -ForegroundColor Green

try {
    # Use .NET reflection to load Npgsql dynamically
    $assembly = [System.Reflection.Assembly]::LoadFrom("$PWD\bin\Debug\net8.0\Npgsql.dll")
    if ($assembly) {
        Write-Host "Loaded Npgsql assembly successfully" -ForegroundColor Green
    }
    
    # Read the SQL fix script
    $sqlContent = Get-Content -Path "..\fix_sales_lead_grid_functions.sql" -Raw
    
    # Create connection and execute
    $connectionType = $assembly.GetType("Npgsql.NpgsqlConnection")
    $connection = [Activator]::CreateInstance($connectionType, $ConnectionString)
    $connection.Open()
    Write-Host "Connected to PostgreSQL" -ForegroundColor Green
    
    $commandType = $assembly.GetType("Npgsql.NpgsqlCommand")
    $command = [Activator]::CreateInstance($commandType, $sqlContent, $connection)
    $result = $command.ExecuteNonQuery()
    
    Write-Host "Sales lead grid functions fixed successfully!" -ForegroundColor Green
    Write-Host "Created both user-based and non-user-based function overloads" -ForegroundColor Yellow
    
    $connection.Close()
    
    Write-Host "`nNow test your API with:" -ForegroundColor Cyan
    Write-Host "POST http://localhost:5104/api/SalesLead/grid" -ForegroundColor White
    Write-Host 'Body: {"pageNumber": 1, "pageSize": 10, "userCreated": 1}' -ForegroundColor White
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nAlternative: Copy and paste the content of fix_sales_lead_grid_functions.sql into your PostgreSQL client (pgAdmin, DBeaver, etc.)" -ForegroundColor Yellow
    exit 1
}
