-- Drop all existing bank account functions to avoid conflicts
DROP FUNCTION IF EXISTS public.sp_get_cs_bank_account_by_id(int4);
DROP FUNCTION IF EXISTS public.sp_create_cs_bank_account(int4, varchar, varchar, varchar, varchar, varchar, varchar, varchar);
DROP FUNCTION IF EXISTS public.sp_update_cs_bank_account(int4, varchar, varchar, varchar, varchar, varchar, varchar, varchar);
DROP FUNCTION IF EXISTS public.sp_delete_cs_bank_account(int4);
DROP FUNCTION IF EXISTS public.sp_get_cs_bank_accounts_by_company(int4, int4, int4);
DROP FUNCTION IF EXISTS public.sp_search_cs_bank_accounts(int4, varchar, varchar, varchar, int4, int4);

-- 1. Get bank account by ID
CREATE OR REPLACE FUNCTION public.sp_get_cs_bank_account_by_id(p_bank_account_id integer)
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
    updated_at timestamp with time zone
)
LANGUAGE plpgsql
AS $function$
BEGIN
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
        ba.updated_at
    FROM cs_bank_accounts ba
    WHERE ba.bank_account_id = p_bank_account_id;
END;
$function$;

-- 2. Create new bank account
CREATE OR REPLACE FUNCTION public.sp_create_cs_bank_account(
    p_company_id integer,
    p_bank_name varchar,
    p_bank_branch_name varchar,
    p_account_number varchar,
    p_ifsc_code varchar,
    p_swift_code varchar,
    p_purpose varchar,
    p_currency varchar
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
    updated_at timestamp with time zone
)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_bank_account_id integer;
BEGIN
    INSERT INTO cs_bank_accounts(
        company_id, bank_name, bank_branch_name, account_number,
        ifsc_code, swift_code, purpose, currency
    )
    VALUES(
        p_company_id, p_bank_name, p_bank_branch_name, p_account_number,
        p_ifsc_code, p_swift_code, p_purpose, p_currency
    )
    RETURNING bank_account_id INTO v_bank_account_id;
    
    RETURN QUERY
    SELECT * FROM sp_get_cs_bank_account_by_id(v_bank_account_id);
END;
$function$;

-- 3. Update bank account
CREATE OR REPLACE FUNCTION public.sp_update_cs_bank_account(
    p_bank_account_id integer,
    p_bank_name varchar,
    p_bank_branch_name varchar,
    p_account_number varchar,
    p_ifsc_code varchar,
    p_swift_code varchar,
    p_purpose varchar,
    p_currency varchar
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
    updated_at timestamp with time zone
)
LANGUAGE plpgsql
AS $function$
BEGIN
    UPDATE cs_bank_accounts ba
    SET
        bank_name = p_bank_name,
        bank_branch_name = p_bank_branch_name,
        account_number = p_account_number,
        ifsc_code = p_ifsc_code,
        swift_code = p_swift_code,
        purpose = p_purpose,
        currency = p_currency,
        updated_at = CURRENT_TIMESTAMP
    WHERE ba.bank_account_id = p_bank_account_id;
    
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
        ba.updated_at
    FROM cs_bank_accounts ba
    WHERE ba.bank_account_id = p_bank_account_id;
END;
$function$;

-- 4. Delete bank account
CREATE OR REPLACE FUNCTION public.sp_delete_cs_bank_account(p_bank_account_id integer)
RETURNS boolean
LANGUAGE plpgsql
AS $function$
DECLARE
    v_count integer;
BEGIN
    DELETE FROM cs_bank_accounts
    WHERE bank_account_id = p_bank_account_id;
    
    GET DIAGNOSTICS v_count = ROW_COUNT;
    RETURN v_count > 0;
END;
$function$;

-- 5. Get bank accounts by company with pagination
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
    -- Get total count first
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

-- 6. Search bank accounts with filtering and pagination
CREATE OR REPLACE FUNCTION public.sp_search_cs_bank_accounts(
    p_company_id integer,
    p_search_text varchar,
    p_purpose varchar,
    p_currency varchar,
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
    v_search_text varchar := LOWER(COALESCE(p_search_text, ''));
BEGIN
    -- Get total count with filters
    SELECT COUNT(*) INTO v_total_count
    FROM cs_bank_accounts ba
    WHERE ba.company_id = p_company_id
      AND (p_search_text IS NULL OR 
           LOWER(ba.bank_name) LIKE '%' || v_search_text || '%' OR
           LOWER(ba.bank_branch_name) LIKE '%' || v_search_text || '%' OR
           LOWER(ba.account_number) LIKE '%' || v_search_text || '%' OR
           LOWER(ba.ifsc_code) LIKE '%' || v_search_text || '%')
      AND (p_purpose IS NULL OR ba.purpose = p_purpose)
      AND (p_currency IS NULL OR ba.currency = p_currency);
    
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
      AND (p_search_text IS NULL OR 
           LOWER(ba.bank_name) LIKE '%' || v_search_text || '%' OR
           LOWER(ba.bank_branch_name) LIKE '%' || v_search_text || '%' OR
           LOWER(ba.account_number) LIKE '%' || v_search_text || '%' OR
           LOWER(ba.ifsc_code) LIKE '%' || v_search_text || '%')
      AND (p_purpose IS NULL OR ba.purpose = p_purpose)
      AND (p_currency IS NULL OR ba.currency = p_currency)
    ORDER BY ba.bank_name
    LIMIT p_page_size
    OFFSET v_offset;
END;
$function$;
