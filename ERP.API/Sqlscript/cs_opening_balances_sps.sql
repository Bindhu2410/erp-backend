-- Search Opening Balances with pagination
CREATE OR REPLACE FUNCTION public.sp_cs_opening_balances_search(
    p_company_id integer DEFAULT NULL,
    p_account_id integer DEFAULT NULL,
    p_period_id integer DEFAULT NULL,
    p_page_size integer DEFAULT 10,
    p_page_number integer DEFAULT 1
)
RETURNS TABLE (
    balance_id integer,
    company_id integer,
    account_id integer,
    period_id integer,
    balance_amount numeric(18,2),
    balance_type varchar(10),
    created_at timestamptz,
    updated_at timestamptz,
    total_records integer
) AS $$
BEGIN
    RETURN QUERY
    WITH filtered_data AS (
        SELECT 
            ob.*,
            COUNT(*) OVER() as total_count
        FROM public.cs_opening_balances ob
        WHERE (p_company_id IS NULL OR ob.company_id = p_company_id)
        AND (p_account_id IS NULL OR ob.account_id = p_account_id)
        AND (p_period_id IS NULL OR ob.period_id = p_period_id)
    )
    SELECT 
        fd.balance_id,
        fd.company_id,
        fd.account_id,
        fd.period_id,
        fd.balance_amount,
        fd.balance_type,
        fd.created_at,
        fd.updated_at,
        fd.total_count::integer as total_records
    FROM filtered_data fd
    ORDER BY fd.balance_id
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

-- Get Opening Balance by ID
CREATE OR REPLACE FUNCTION public.sp_cs_opening_balances_get_by_id(
    p_balance_id integer
)
RETURNS TABLE (
    balance_id integer,
    company_id integer,
    account_id integer,
    period_id integer,
    balance_amount numeric(18,2),
    balance_type varchar(10),
    created_at timestamptz,
    updated_at timestamptz
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ob.balance_id,
        ob.company_id,
        ob.account_id,
        ob.period_id,
        ob.balance_amount,
        ob.balance_type,
        ob.created_at,
        ob.updated_at
    FROM public.cs_opening_balances ob
    WHERE ob.balance_id = p_balance_id;
END;
$$ LANGUAGE plpgsql;

-- Create Opening Balance
CREATE OR REPLACE FUNCTION public.sp_cs_opening_balances_create(
    p_company_id integer,
    p_account_id integer,
    p_period_id integer,
    p_balance_amount numeric(18,2),
    p_balance_type varchar(10)
)
RETURNS integer AS $$
DECLARE
    v_balance_id integer;
BEGIN
    INSERT INTO public.cs_opening_balances(
        company_id,
        account_id,
        period_id,
        balance_amount,
        balance_type
    )
    VALUES (
        p_company_id,
        p_account_id,
        p_period_id,
        p_balance_amount,
        p_balance_type
    )
    RETURNING balance_id INTO v_balance_id;

    RETURN v_balance_id;
END;
$$ LANGUAGE plpgsql;

-- Update Opening Balance
CREATE OR REPLACE FUNCTION public.sp_cs_opening_balances_update(
    p_balance_id integer,
    p_company_id integer,
    p_account_id integer,
    p_period_id integer,
    p_balance_amount numeric(18,2),
    p_balance_type varchar(10)
)
RETURNS boolean AS $$
BEGIN
    UPDATE public.cs_opening_balances
    SET 
        company_id = p_company_id,
        account_id = p_account_id,
        period_id = p_period_id,
        balance_amount = p_balance_amount,
        balance_type = p_balance_type,
        updated_at = CURRENT_TIMESTAMP
    WHERE balance_id = p_balance_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- Delete Opening Balance
CREATE OR REPLACE FUNCTION public.sp_cs_opening_balances_delete(
    p_balance_id integer
)
RETURNS boolean AS $$
BEGIN
    DELETE FROM public.cs_opening_balances
    WHERE balance_id = p_balance_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;



-- DROP FUNCTION public.sp_get_cs_opening_balances_by_company_period(int4, int4, int4, int4);

CREATE OR REPLACE FUNCTION public.sp_get_cs_opening_balances_by_company_period(
    p_company_id INTEGER,
    p_period_id INTEGER,
    p_page_number INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 10
)
RETURNS TABLE (
    balance_id INTEGER,
    company_id INTEGER,
    account_id INTEGER,
    period_id INTEGER,
    balance_amount NUMERIC(18,2),
    balance_type VARCHAR,
    account_code VARCHAR,
    account_name VARCHAR,
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ,
    total_count BIGINT
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    WITH CountCTE AS (
        SELECT COUNT(*) AS total_count
        FROM cs_opening_balances ob
        WHERE ob.company_id = p_company_id
        AND ob.period_id = p_period_id
    )
    SELECT 
        ob.balance_id,
        ob.company_id,
        ob.account_id,
        ob.period_id,
        ob.balance_amount,
        ob.balance_type,
        coa.account_code,
        coa.account_name,
        ob.created_at,
        ob.updated_at,
        c.total_count
    FROM cs_opening_balances ob
    JOIN cs_chart_of_accounts coa ON ob.account_id = coa.account_id
    CROSS JOIN CountCTE c
    WHERE ob.company_id = p_company_id
    AND ob.period_id = p_period_id
    ORDER BY coa.account_code
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
END;
$function$;
