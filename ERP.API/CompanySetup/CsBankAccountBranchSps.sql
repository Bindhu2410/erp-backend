-- Create Bank Account Branch Mapping
CREATE OR REPLACE FUNCTION public.sp_create_cs_bank_account_branch(
    p_bank_account_id integer,
    p_branch_id integer
)
RETURNS TABLE(
    bank_account_id integer,
    branch_id integer
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    INSERT INTO cs_bank_account_branches(
        bank_account_id,
        branch_id
    )
    VALUES (
        p_bank_account_id,
        p_branch_id
    )
    RETURNING 
        bank_account_id,
        branch_id;
END;
$function$;

-- Delete Bank Account Branch Mapping
CREATE OR REPLACE FUNCTION public.sp_delete_cs_bank_account_branch(
    p_bank_account_id integer,
    p_branch_id integer
)
RETURNS boolean
LANGUAGE plpgsql
AS $function$
BEGIN
    DELETE FROM cs_bank_account_branches
    WHERE bank_account_id = p_bank_account_id
    AND branch_id = p_branch_id;
    RETURN FOUND;
END;
$function$;

-- Get Bank Account Branches by Bank Account ID
CREATE OR REPLACE FUNCTION public.sp_get_cs_bank_account_branches_by_account(
    p_bank_account_id integer
)
RETURNS TABLE(
    bank_account_id integer,
    branch_id integer,
    branch_name varchar,
    branch_code varchar,
    address varchar,
    contact_person varchar,
    contact_number varchar,
    email varchar
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT 
        bab.bank_account_id,
        bab.branch_id,
        b.branch_name,
        b.branch_code,
        b.address,
        b.contact_person,
        b.contact_number,
        b.email
    FROM cs_bank_account_branches bab
    INNER JOIN cs_branches b ON b.branch_id = bab.branch_id
    WHERE bab.bank_account_id = p_bank_account_id;
END;
$function$;

-- Get Bank Accounts by Branch ID
CREATE OR REPLACE FUNCTION public.sp_get_cs_bank_accounts_by_branch(
    p_branch_id integer,
    p_page_number integer DEFAULT 1,
    p_page_size integer DEFAULT 10
)
RETURNS TABLE(
    bank_account_id integer,
    company_id integer,
    bank_name varchar,
    bank_branch_name varchar,
    account_number varchar,
    ifsc_code varchar,
    swift_code varchar,
    purpose varchar,
    currency varchar,
    created_at timestamptz,
    updated_at timestamptz,
    total_count bigint
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    WITH CountCTE AS (
        SELECT COUNT(*) AS total_count
        FROM cs_bank_account_branches bab
        INNER JOIN cs_bank_accounts ba ON ba.bank_account_id = bab.bank_account_id
        WHERE bab.branch_id = p_branch_id
    )
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
        c.total_count
    FROM cs_bank_account_branches bab
    INNER JOIN cs_bank_accounts ba ON ba.bank_account_id = bab.bank_account_id
    CROSS JOIN CountCTE c
    WHERE bab.branch_id = p_branch_id
    ORDER BY ba.bank_name
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
END;
$function$;
