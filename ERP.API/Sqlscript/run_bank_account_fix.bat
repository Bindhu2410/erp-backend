@echo off
echo Running SQL fix script for the company_id ambiguous error...

REM Update these variables with your actual database connection details
set PGHOST=localhost
set PGPORT=5432
set PGUSER=postgres
set PGPASSWORD=your_password
set PGDATABASE=your_database

echo Applying fix for ambiguous column reference in sp_get_cs_bank_accounts_by_company...
psql -h %PGHOST% -p %PGPORT% -U %PGUSER% -d %PGDATABASE% -c "
-- Fix the ambiguous company_id reference
DROP FUNCTION IF EXISTS public.sp_get_cs_bank_accounts_by_company(int4, int4, int4);
CREATE OR REPLACE FUNCTION public.sp_get_cs_bank_accounts_by_company(
    p_company_id integer,
    p_page_number integer,
    p_page_size integer
)
RETURNS TABLE(
    bank_account_id integer, 
    company_id integer, 
    bank_name character varying, 
    bank_branch_name character varying, 
    account_number character varying, 
    ifsc_code character varying, 
    swift_code character varying, 
    purpose character varying, 
    currency character varying, 
    created_at timestamp with time zone, 
    updated_at timestamp with time zone,
    total_count bigint
)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_offset integer := (p_page_number - 1) * p_page_size;
    v_total_count bigint;
BEGIN
    -- Get total count first with explicit table alias
    SELECT COUNT(*) INTO v_total_count
    FROM cs_bank_accounts ba
    WHERE ba.company_id = p_company_id;
    
    RETURN QUERY
    SELECT
        ba.bank_account_id,
        ba.company_id,
        ba.bank_name,
        ba.bank_branch_name,
        ba.account_number,
        ba.ifsc_code,
        ba.swift_code,
        ba.purpose,
        ba.currency,
        ba.created_at,
        ba.updated_at,
        v_total_count as total_count
    FROM cs_bank_accounts ba
    WHERE ba.company_id = p_company_id
    ORDER BY ba.bank_name
    LIMIT p_page_size
    OFFSET v_offset;
END;
$function$;
"

if %ERRORLEVEL% NEQ 0 (
    echo Error applying the fix
    echo Please check the error message above and make sure your database credentials are correct.
    pause
    exit /b %ERRORLEVEL%
)

echo Fix applied successfully.
pause
