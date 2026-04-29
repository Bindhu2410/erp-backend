-- =====================================================
-- Company Setup: Bank Accounts Table and Stored Procedures
-- =====================================================

-- Drop existing table if needed
-- DROP TABLE IF EXISTS public.cs_bank_accounts CASCADE;

-- Create cs_bank_accounts table
CREATE TABLE IF NOT EXISTS public.cs_bank_accounts (
    bank_account_id SERIAL PRIMARY KEY,
    company_id INTEGER NOT NULL,
    bank_name VARCHAR(255) NOT NULL,
    bank_branch_name VARCHAR(255) NOT NULL,
    account_number VARCHAR(50) NOT NULL,
    ifsc_code VARCHAR(20) NOT NULL,
    swift_code VARCHAR(20),
    purpose VARCHAR(50) NOT NULL,
    currency VARCHAR(5) NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    
    -- Constraints
    CONSTRAINT cs_bank_accounts_company_id_account_number_key UNIQUE (company_id, account_number),
    CONSTRAINT cs_bank_accounts_company_id_ifsc_code_key UNIQUE (company_id, ifsc_code)
);

-- Add foreign key constraint (assuming cs_companies table exists)
-- ALTER TABLE public.cs_bank_accounts 
-- ADD CONSTRAINT cs_bank_accounts_company_id_fkey 
-- FOREIGN KEY (company_id) REFERENCES public.cs_companies(company_id) ON DELETE CASCADE;

-- =====================================================
-- Stored Procedures
-- =====================================================

-- 1. Get bank account by ID
CREATE OR REPLACE FUNCTION public.sp_get_cs_bank_account_by_id(p_bank_account_id INTEGER)
RETURNS TABLE(
    bank_account_id INTEGER,
    company_id INTEGER,
    bank_name VARCHAR(255),
    bank_branch_name VARCHAR(255),
    account_number VARCHAR(50),
    ifsc_code VARCHAR(20),
    swift_code VARCHAR(20),
    purpose VARCHAR(50),
    currency VARCHAR(5),
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ
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

-- 2. Create bank account
CREATE OR REPLACE FUNCTION public.sp_create_cs_bank_account(
    p_company_id INTEGER,
    p_bank_name VARCHAR(255),
    p_bank_branch_name VARCHAR(255),
    p_account_number VARCHAR(50),
    p_ifsc_code VARCHAR(20),
    p_swift_code VARCHAR(20),
    p_purpose VARCHAR(50),
    p_currency VARCHAR(5)
)
RETURNS TABLE(
    bank_account_id INTEGER,
    company_id INTEGER,
    bank_name VARCHAR(255),
    bank_branch_name VARCHAR(255),
    account_number VARCHAR(50),
    ifsc_code VARCHAR(20),
    swift_code VARCHAR(20),
    purpose VARCHAR(50),
    currency VARCHAR(5),
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ
)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_bank_account_id INTEGER;
BEGIN
    INSERT INTO cs_bank_accounts (
        company_id,
        bank_name,
        bank_branch_name,
        account_number,
        ifsc_code,
        swift_code,
        purpose,
        currency,
        created_at,
        updated_at
    )
    VALUES (
        p_company_id,
        p_bank_name,
        p_bank_branch_name,
        p_account_number,
        p_ifsc_code,
        p_swift_code,
        p_purpose,
        p_currency,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    )
    RETURNING cs_bank_accounts.bank_account_id INTO v_bank_account_id;

    RETURN QUERY
    SELECT * FROM sp_get_cs_bank_account_by_id(v_bank_account_id);
END;
$function$;

-- 3. Update bank account
CREATE OR REPLACE FUNCTION public.sp_update_cs_bank_account(
    p_bank_account_id INTEGER,
    p_bank_name VARCHAR(255),
    p_bank_branch_name VARCHAR(255),
    p_account_number VARCHAR(50),
    p_ifsc_code VARCHAR(20),
    p_swift_code VARCHAR(20),
    p_purpose VARCHAR(50),
    p_currency VARCHAR(5)
)
RETURNS TABLE(
    bank_account_id INTEGER,
    company_id INTEGER,
    bank_name VARCHAR(255),
    bank_branch_name VARCHAR(255),
    account_number VARCHAR(50),
    ifsc_code VARCHAR(20),
    swift_code VARCHAR(20),
    purpose VARCHAR(50),
    currency VARCHAR(5),
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ
)
LANGUAGE plpgsql
AS $function$
BEGIN
    UPDATE cs_bank_accounts
    SET
        bank_name = p_bank_name,
        bank_branch_name = p_bank_branch_name,
        account_number = p_account_number,
        ifsc_code = p_ifsc_code,
        swift_code = p_swift_code,
        purpose = p_purpose,
        currency = p_currency,
        updated_at = CURRENT_TIMESTAMP
    WHERE cs_bank_accounts.bank_account_id = p_bank_account_id;

    RETURN QUERY
    SELECT * FROM sp_get_cs_bank_account_by_id(p_bank_account_id);
END;
$function$;

-- 4. Delete bank account
CREATE OR REPLACE FUNCTION public.sp_delete_cs_bank_account(p_bank_account_id INTEGER)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $function$
DECLARE
    v_rows_affected INTEGER;
BEGIN
    DELETE FROM cs_bank_accounts
    WHERE bank_account_id = p_bank_account_id;
    
    GET DIAGNOSTICS v_rows_affected = ROW_COUNT;
    
    RETURN v_rows_affected > 0;
END;
$function$;

-- 5. Get bank accounts by company with pagination
CREATE OR REPLACE FUNCTION public.sp_get_cs_bank_accounts_by_company(
    p_company_id INTEGER,
    p_page_number INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 10
)
RETURNS TABLE(
    bank_account_id INTEGER,
    company_id INTEGER,
    bank_name VARCHAR(255),
    bank_branch_name VARCHAR(255),
    account_number VARCHAR(50),
    ifsc_code VARCHAR(20),
    swift_code VARCHAR(20),
    purpose VARCHAR(50),
    currency VARCHAR(5),
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ,
    totalcount BIGINT
)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_offset INTEGER;
    v_total_count BIGINT;
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Get total count
    SELECT COUNT(*)
    INTO v_total_count
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
        v_total_count
    FROM cs_bank_accounts ba
    WHERE ba.company_id = p_company_id
    ORDER BY ba.created_at DESC
    LIMIT p_page_size OFFSET v_offset;
END;
$function$;

-- 6. Search bank accounts
CREATE OR REPLACE FUNCTION public.sp_search_cs_bank_accounts(
    p_company_id INTEGER,
    p_search_text VARCHAR(255) DEFAULT NULL,
    p_purpose VARCHAR(50) DEFAULT NULL,
    p_currency VARCHAR(5) DEFAULT NULL,
    p_page_number INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 10
)
RETURNS TABLE(
    bank_account_id INTEGER,
    company_id INTEGER,
    bank_name VARCHAR(255),
    bank_branch_name VARCHAR(255),
    account_number VARCHAR(50),
    ifsc_code VARCHAR(20),
    swift_code VARCHAR(20),
    purpose VARCHAR(50),
    currency VARCHAR(5),
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ,
    totalcount BIGINT
)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_offset INTEGER;
    v_total_count BIGINT;
    v_search_pattern VARCHAR(257);
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Prepare search pattern
    IF p_search_text IS NOT NULL THEN
        v_search_pattern := '%' || p_search_text || '%';
    END IF;
    
    -- Get total count with filters
    SELECT COUNT(*)
    INTO v_total_count
    FROM cs_bank_accounts ba
    WHERE ba.company_id = p_company_id
      AND (p_search_text IS NULL OR (
          ba.bank_name ILIKE v_search_pattern
          OR ba.bank_branch_name ILIKE v_search_pattern
          OR ba.account_number ILIKE v_search_pattern
          OR ba.ifsc_code ILIKE v_search_pattern
      ))
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
        v_total_count
    FROM cs_bank_accounts ba
    WHERE ba.company_id = p_company_id
      AND (p_search_text IS NULL OR (
          ba.bank_name ILIKE v_search_pattern
          OR ba.bank_branch_name ILIKE v_search_pattern
          OR ba.account_number ILIKE v_search_pattern
          OR ba.ifsc_code ILIKE v_search_pattern
      ))
      AND (p_purpose IS NULL OR ba.purpose = p_purpose)
      AND (p_currency IS NULL OR ba.currency = p_currency)
    ORDER BY ba.created_at DESC
    LIMIT p_page_size OFFSET v_offset;
END;
$function$;

-- =====================================================
-- Sample Data (Optional - Remove if not needed)
-- =====================================================

-- Insert sample data if companies exist
-- INSERT INTO cs_bank_accounts (company_id, bank_name, bank_branch_name, account_number, ifsc_code, swift_code, purpose, currency)
-- VALUES 
--     (1, 'State Bank of India', 'Main Branch', '123456789012', 'SBIN0000123', 'SBININBB123', 'Current Account', 'INR'),
--     (1, 'HDFC Bank', 'Corporate Branch', '987654321098', 'HDFC0000456', 'HDFCINBB456', 'Savings Account', 'INR');
