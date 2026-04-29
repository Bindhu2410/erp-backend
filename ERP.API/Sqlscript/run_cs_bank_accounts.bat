@echo off
echo Running CS Bank Accounts SQL Script...

:: Set your PostgreSQL connection details
set PGHOST=localhost
set PGPORT=5432
set PGDATABASE=your_database_name
set PGUSER=your_username

:: Prompt for password (or set PGPASSWORD environment variable)
echo Please enter your PostgreSQL password when prompted

:: Run the SQL script
psql -f cs_bank_accounts.sql

echo.
echo Script execution completed!
pause
