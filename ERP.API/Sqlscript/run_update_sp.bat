@echo off
echo Running SQL script to update bank account stored procedures...

REM Update these variables with your actual database connection details
set PGHOST=localhost
set PGPORT=5432
set PGUSER=postgres
set PGPASSWORD=your_password
set PGDATABASE=your_database

echo Updating PostgreSQL functions for CsBankAccount...
psql -h %PGHOST% -p %PGPORT% -U %PGUSER% -d %PGDATABASE% -f update_cs_bank_account_sp.sql

if %ERRORLEVEL% NEQ 0 (
    echo Error running SQL script
    echo Please check the error message above and make sure your database credentials are correct.
    pause
    exit /b %ERRORLEVEL%
)

echo SQL script executed successfully.
echo All CsBankAccount PostgreSQL functions have been updated.
pause
