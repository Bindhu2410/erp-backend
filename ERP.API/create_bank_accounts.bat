@echo off
echo Creating cs_bank_accounts table and stored procedures...

REM Update these connection parameters according to your PostgreSQL setup
set PGHOST=localhost
set PGPORT=5432
set PGDATABASE=your_database_name
set PGUSER=your_username

REM Execute the SQL script
psql -f "Sqlscript\cs_bank_accounts.sql"

if %ERRORLEVEL% EQU 0 (
    echo Successfully created cs_bank_accounts table and stored procedures!
) else (
    echo Error occurred while executing SQL script!
)

pause
