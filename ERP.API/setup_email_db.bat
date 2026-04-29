@echo off
echo Setting up Email System Database Tables...
echo.

REM Set PostgreSQL path (adjust if needed)
set PGPATH="C:\Program Files\PostgreSQL\15\bin"
if not exist %PGPATH% set PGPATH="C:\Program Files\PostgreSQL\14\bin"
if not exist %PGPATH% set PGPATH="C:\Program Files\PostgreSQL\13\bin"
if not exist %PGPATH% set PGPATH="C:\Program Files\PostgreSQL\16\bin"

REM Add PostgreSQL to PATH temporarily
set PATH=%PGPATH%;%PATH%

echo Creating OAuth States table...
psql -h localhost -p 5433 -U postgres -d postgres -f "Sqlscript\OAuthStatesTable.sql"

if %ERRORLEVEL% NEQ 0 (
    echo Failed to create OAuth States table!
    pause
    exit /b 1
)

echo Creating Email System tables...
psql -h localhost -p 5433 -U postgres -d postgres -f "Sqlscript\EmailSystemTables.sql"

if %ERRORLEVEL% NEQ 0 (
    echo Failed to create Email System tables!
    pause
    exit /b 1
)

echo.
echo Verifying tables were created...
psql -h localhost -p 5433 -U postgres -d postgres -c "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND (table_name LIKE 'email_%' OR table_name = 'oauth_states') ORDER BY table_name;"

echo.
echo Email System database setup completed successfully!
echo.
pause
