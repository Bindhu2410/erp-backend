-- Create a new chart of account
CREATE OR REPLACE FUNCTION public.sp_create_cs_chart_of_account(
    p_company_id integer,
    p_parent_account_id integer,
    p_account_code varchar(50),
    p_account_name varchar(255),
    p_account_type varchar(50),
    p_is_active boolean DEFAULT true,
    p_cost_centre_allocation_required boolean DEFAULT false
)
RETURNS TABLE (
    account_id integer,
    company_id integer,
    parent_account_id integer,
    account_code varchar(50),
    account_name varchar(255),
    account_type varchar(50),
    is_active boolean,
    cost_centre_allocation_required boolean,
    created_at timestamptz,
    updated_at timestamptz
) AS $$
BEGIN
    RETURN QUERY
    INSERT INTO public.cs_chart_of_accounts (
        company_id,
        parent_account_id,
        account_code,
        account_name,
        account_type,
        is_active,
        cost_centre_allocation_required
    )
    VALUES (
        p_company_id,
        p_parent_account_id,
        p_account_code,
        p_account_name,
        p_account_type,
        p_is_active,
        p_cost_centre_allocation_required
    )
    RETURNING *;
END;
$$ LANGUAGE plpgsql;

-- Update an existing chart of account
CREATE OR REPLACE FUNCTION public.sp_update_cs_chart_of_account(
    p_account_id integer,
    p_company_id integer,
    p_parent_account_id integer,
    p_account_code varchar(50),
    p_account_name varchar(255),
    p_account_type varchar(50),
    p_is_active boolean,
    p_cost_centre_allocation_required boolean
)
RETURNS TABLE (
    account_id integer,
    company_id integer,
    parent_account_id integer,
    account_code varchar(50),
    account_name varchar(255),
    account_type varchar(50),
    is_active boolean,
    cost_centre_allocation_required boolean,
    created_at timestamptz,
    updated_at timestamptz
) AS $$
BEGIN
    RETURN QUERY
    UPDATE public.cs_chart_of_accounts
    SET
        company_id = p_company_id,
        parent_account_id = p_parent_account_id,
        account_code = p_account_code,
        account_name = p_account_name,
        account_type = p_account_type,
        is_active = p_is_active,
        cost_centre_allocation_required = p_cost_centre_allocation_required,
        updated_at = CURRENT_TIMESTAMP
    WHERE account_id = p_account_id
    RETURNING *;
END;
$$ LANGUAGE plpgsql;

-- Delete a chart of account
CREATE OR REPLACE FUNCTION public.sp_delete_cs_chart_of_account(
    p_account_id integer
)
RETURNS boolean AS $$
DECLARE
    v_count integer;
BEGIN
    -- Check if the account has child accounts
    SELECT COUNT(*)
    INTO v_count
    FROM public.cs_chart_of_accounts
    WHERE parent_account_id = p_account_id;

    -- Only delete if there are no child accounts
    IF v_count = 0 THEN
        DELETE FROM public.cs_chart_of_accounts
        WHERE account_id = p_account_id;

        GET DIAGNOSTICS v_count = ROW_COUNT;
        RETURN v_count > 0;
    END IF;

    RETURN false;
END;
$$ LANGUAGE plpgsql;

-- Get a chart of account by ID
CREATE OR REPLACE FUNCTION public.sp_get_cs_chart_of_account_by_id(
    p_account_id integer
)
RETURNS TABLE (
    account_id integer,
    company_id integer,
    parent_account_id integer,
    account_code varchar(50),
    account_name varchar(255),
    account_type varchar(50),
    is_active boolean,
    cost_centre_allocation_required boolean,
    created_at timestamptz,
    updated_at timestamptz
) AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM public.cs_chart_of_accounts
    WHERE account_id = p_account_id;
END;
$$ LANGUAGE plpgsql;

-- Get chart of accounts by company ID with pagination and search
CREATE OR REPLACE FUNCTION public.sp_get_cs_chart_of_accounts_by_company(
    p_company_id integer,
    p_search_text varchar DEFAULT NULL,
    p_account_type varchar DEFAULT NULL,
    p_is_active boolean DEFAULT NULL,
    p_page_number integer DEFAULT 1,
    p_page_size integer DEFAULT 10
)
RETURNS TABLE (
    account_id integer,
    company_id integer,
    parent_account_id integer,
    account_code varchar(50),
    account_name varchar(255),
    account_type varchar(50),
    is_active boolean,
    cost_centre_allocation_required boolean,
    created_at timestamptz,
    updated_at timestamptz,
    total_count bigint
) AS $$
BEGIN
    RETURN QUERY
    WITH filtered_accounts AS (
        SELECT *
        FROM public.cs_chart_of_accounts coa
        WHERE coa.company_id = p_company_id
        AND (
            p_search_text IS NULL
            OR coa.account_code ILIKE '%' || p_search_text || '%'
            OR coa.account_name ILIKE '%' || p_search_text || '%'
        )
        AND (p_account_type IS NULL OR coa.account_type = p_account_type)
        AND (p_is_active IS NULL OR coa.is_active = p_is_active)
    ),
    paginated_results AS (
        SELECT 
            fa.*,
            COUNT(*) OVER() as total_count
        FROM filtered_accounts fa
        ORDER BY fa.account_code
        LIMIT p_page_size
        OFFSET ((p_page_number - 1) * p_page_size)
    )
    SELECT * FROM paginated_results;
END;
$$ LANGUAGE plpgsql;

-- Get chart of accounts hierarchy for a company
CREATE OR REPLACE FUNCTION public.sp_get_cs_chart_of_accounts_hierarchy(
    p_company_id integer,
    p_include_inactive boolean DEFAULT false
)
RETURNS TABLE (
    account_id integer,
    parent_account_id integer,
    account_code varchar(50),
    account_name varchar(255),
    account_type varchar(50),
    is_active boolean,
    cost_centre_allocation_required boolean,
    level integer,
    path text
) AS $$
BEGIN
    RETURN QUERY
    WITH RECURSIVE account_tree AS (
        -- Base case: top-level accounts
        SELECT 
            coa.account_id,
            coa.parent_account_id,
            coa.account_code,
            coa.account_name,
            coa.account_type,
            coa.is_active,
            coa.cost_centre_allocation_required,
            1 as level,
            coa.account_name::text as path
        FROM public.cs_chart_of_accounts coa
        WHERE coa.company_id = p_company_id 
        AND coa.parent_account_id IS NULL
        AND (p_include_inactive = true OR coa.is_active = true)
        
        UNION ALL
        
        -- Recursive case: child accounts
        SELECT 
            coa.account_id,
            coa.parent_account_id,
            coa.account_code,
            coa.account_name,
            coa.account_type,
            coa.is_active,
            coa.cost_centre_allocation_required,
            at.level + 1,
            at.path || ' > ' || coa.account_name
        FROM public.cs_chart_of_accounts coa
        INNER JOIN account_tree at ON coa.parent_account_id = at.account_id
        WHERE coa.company_id = p_company_id
        AND (p_include_inactive = true OR coa.is_active = true)
    )
    SELECT * FROM account_tree
    ORDER BY path;
END;
$$ LANGUAGE plpgsql;

-- Get chart of accounts dropdown items for a company
CREATE OR REPLACE FUNCTION public.sp_get_cs_chart_of_accounts_dropdown(
    p_company_id integer,
    p_account_type varchar DEFAULT NULL
)
RETURNS TABLE (
    account_id integer,
    account_code varchar(50),
    account_name varchar(255),
    parent_account_id integer,
    path text
) AS $$
BEGIN
    RETURN QUERY
    WITH RECURSIVE account_tree AS (
        -- Base case: top-level accounts
        SELECT 
            coa.account_id,
            coa.account_code,
            coa.account_name,
            coa.parent_account_id,
            coa.account_name::text as path
        FROM public.cs_chart_of_accounts coa
        WHERE coa.company_id = p_company_id 
        AND coa.parent_account_id IS NULL
        AND coa.is_active = true
        AND (p_account_type IS NULL OR coa.account_type = p_account_type)
        
        UNION ALL
        
        -- Recursive case: child accounts
        SELECT 
            coa.account_id,
            coa.account_code,
            coa.account_name,
            coa.parent_account_id,
            at.path || ' > ' || coa.account_name
        FROM public.cs_chart_of_accounts coa
        INNER JOIN account_tree at ON coa.parent_account_id = at.account_id
        WHERE coa.company_id = p_company_id
        AND coa.is_active = true
        AND (p_account_type IS NULL OR coa.account_type = p_account_type)
    )
    SELECT * FROM account_tree
    ORDER BY path;
END;
$$ LANGUAGE plpgsql;
