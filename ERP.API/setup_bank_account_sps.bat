@echo off
echo ========================================
echo  Bank Account Stored Procedures Setup
echo ========================================
echo.

REM Check if PostgreSQL command line tools are available
where psql >nul 2>nul
if %errorlevel% neq 0 (
    echo ERROR: PostgreSQL command line tools (psql) not found in PATH
    echo Please ensure PostgreSQL is installed and psql is in your PATH
    echo.
    pause
    exit /b 1
)

REM Default connection parameters - modify these as needed
set DB_HOST=localhost
set DB_PORT=5432
set DB_NAME=your_database_name
set DB_USER=your_username

echo Current settings:
echo   Host: %DB_HOST%
echo   Port: %DB_PORT%
echo   Database: %DB_NAME%
echo   User: %DB_USER%
echo.

REM Prompt for connection details
set /p DB_HOST="Enter database host (default: %DB_HOST%): " || set DB_HOST=%DB_HOST%
set /p DB_PORT="Enter database port (default: %DB_PORT%): " || set DB_PORT=%DB_PORT%
set /p DB_NAME="Enter database name (default: %DB_NAME%): " || set DB_NAME=%DB_NAME%
set /p DB_USER="Enter database user (default: %DB_USER%): " || set DB_USER=%DB_USER%

echo.
echo Executing Bank Account stored procedures...
echo.

REM Execute the SQL script
psql -h %DB_HOST% -p %DB_PORT% -d %DB_NAME% -U %DB_USER% -f CsBankAccountSps.sql

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo  SUCCESS: All stored procedures created
    echo ========================================
    echo.
    echo The following stored procedures are now available:
    echo   - sp_get_cs_bank_account_by_id
    echo   - sp_create_cs_bank_account
    echo   - sp_update_cs_bank_account
    echo   - sp_delete_cs_bank_account
    echo   - sp_get_cs_bank_accounts_by_company
    echo   - sp_search_cs_bank_accounts
    echo.
    echo Your application should now work without the PostgreSQL errors.
) else (
    echo.
    echo ========================================
    echo  ERROR: Failed to create stored procedures
    echo ========================================
    echo.
    echo Please check the error messages above and verify:
    echo   1. Database connection parameters are correct
    echo   2. Database user has sufficient privileges
    echo   3. The cs_bank_accounts table exists
    echo.
)

echo.
pause
