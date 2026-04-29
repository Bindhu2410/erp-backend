param(
    [string]$ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres123`$;Database=postgres;"
)

Write-Host "Setting up database for cs_bank_account..." -ForegroundColor Green

try {
    # Use .NET reflection to load Npgsql dynamically
    $assembly = [System.Reflection.Assembly]::LoadFrom("$PWD\bin\Debug\net8.0\Npgsql.dll")
    if ($assembly) {
        Write-Host "Loaded Npgsql assembly successfully" -ForegroundColor Green
    }
    
    # Read the SQL fix script
    $sqlContent = Get-Content -Path ".\fix_sp.sql" -Raw
    
    # Create connection and execute
    $connectionType = $assembly.GetType("Npgsql.NpgsqlConnection")
    $connection = [Activator]::CreateInstance($connectionType, $ConnectionString)
    $connection.Open()
    Write-Host "Connected to PostgreSQL" -ForegroundColor Green
    
    $commandType = $assembly.GetType("Npgsql.NpgsqlCommand")
    $command = [Activator]::CreateInstance($commandType, $sqlContent, $connection)
    $result = $command.ExecuteNonQuery()
    
    Write-Host "Stored procedure updated successfully!" -ForegroundColor Green
    Write-Host "Fixed the ambiguous column reference issue" -ForegroundColor Yellow
    
    $connection.Close()
    
    Write-Host "`nNow test your API with:" -ForegroundColor Cyan
    Write-Host "POST http://localhost:5104/api/v1/CsBankAccount" -ForegroundColor White
    Write-Host 'Body: {"CompanyId": 1, "BankName": "Test Bank", "BankBranchName": "Main Branch", "AccountNumber": "123456789", "IFSCCode": "TEST0001", "SwiftCode": "TESTINBB", "Purpose": "Business", "Currency": "INR"}' -ForegroundColor White
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nAlternative: Copy and paste the content of quick_setup.sql into your PostgreSQL client (pgAdmin, DBeaver, etc.)" -ForegroundColor Yellow
    exit 1
}
