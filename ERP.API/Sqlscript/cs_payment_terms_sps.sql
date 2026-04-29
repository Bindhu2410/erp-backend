-- Search Payment Terms with pagination
CREATE OR REPLACE FUNCTION public.sp_cs_payment_terms_search(
    p_company_id integer DEFAULT NULL,
    p_term_name varchar DEFAULT NULL,
    p_calculation_type varchar DEFAULT NULL,
    p_page_size integer DEFAULT 10,
    p_page_number integer DEFAULT 1
)
RETURNS TABLE (
    term_id integer,
    company_id integer,
    term_name varchar,
    calculation_type varchar,
    due_days integer,
    discount_percentage numeric(5,2),
    discount_days integer,
    created_at timestamptz,
    updated_at timestamptz,
    total_records bigint
) AS $$
BEGIN
    RETURN QUERY
    WITH filtered_data AS (
        SELECT 
            pt.*,
            COUNT(*) OVER() as total_count
        FROM public.cs_payment_terms pt
        WHERE (p_company_id IS NULL OR pt.company_id = p_company_id)
        AND (p_term_name IS NULL OR pt.term_name ILIKE '%' || p_term_name || '%')
        AND (p_calculation_type IS NULL OR pt.calculation_type = p_calculation_type)
    )
    SELECT 
        fd.term_id,
        fd.company_id,
        fd.term_name,
        fd.calculation_type,
        fd.due_days,
        fd.discount_percentage,
        fd.discount_days,
        fd.created_at,
        fd.updated_at,
        fd.total_count
    FROM filtered_data fd
    ORDER BY fd.term_name
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

-- Get Payment Term by ID
CREATE OR REPLACE FUNCTION public.sp_cs_payment_terms_get_by_id(
    p_term_id integer
)
RETURNS TABLE (
    term_id integer,
    company_id integer,
    term_name varchar,
    calculation_type varchar,
    due_days integer,
    discount_percentage numeric(5,2),
    discount_days integer,
    created_at timestamptz,
    updated_at timestamptz
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        pt.term_id,
        pt.company_id,
        pt.term_name,
        pt.calculation_type,
        pt.due_days,
        pt.discount_percentage,
        pt.discount_days,
        pt.created_at,
        pt.updated_at
    FROM public.cs_payment_terms pt
    WHERE pt.term_id = p_term_id;
END;
$$ LANGUAGE plpgsql;

-- Create Payment Term
CREATE OR REPLACE FUNCTION public.sp_cs_payment_terms_create(
    p_company_id integer,
    p_term_name varchar,
    p_calculation_type varchar,
    p_due_days integer,
    p_discount_percentage numeric(5,2),
    p_discount_days integer
)
RETURNS integer AS $$
DECLARE
    v_term_id integer;
BEGIN
    INSERT INTO public.cs_payment_terms(
        company_id,
        term_name,
        calculation_type,
        due_days,
        discount_percentage,
        discount_days
    )
    VALUES (
        p_company_id,
        p_term_name,
        p_calculation_type,
        p_due_days,
        p_discount_percentage,
        p_discount_days
    )
    RETURNING term_id INTO v_term_id;

    RETURN v_term_id;
END;
$$ LANGUAGE plpgsql;

-- Update Payment Term
CREATE OR REPLACE FUNCTION public.sp_cs_payment_terms_update(
    p_term_id integer,
    p_company_id integer,
    p_term_name varchar,
    p_calculation_type varchar,
    p_due_days integer,
    p_discount_percentage numeric(5,2),
    p_discount_days integer
)
RETURNS boolean AS $$
BEGIN
    UPDATE public.cs_payment_terms
    SET 
        company_id = p_company_id,
        term_name = p_term_name,
        calculation_type = p_calculation_type,
        due_days = p_due_days,
        discount_percentage = p_discount_percentage,
        discount_days = p_discount_days,
        updated_at = CURRENT_TIMESTAMP
    WHERE term_id = p_term_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- Delete Payment Term
CREATE OR REPLACE FUNCTION public.sp_cs_payment_terms_delete(
    p_term_id integer
)
RETURNS boolean AS $$
BEGIN
    DELETE FROM public.cs_payment_terms
    WHERE term_id = p_term_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- Get Payment Terms by Company
CREATE OR REPLACE FUNCTION public.sp_cs_payment_terms_get_by_company(
    p_company_id integer,
    p_page_size integer DEFAULT 10,
    p_page_number integer DEFAULT 1
)
RETURNS TABLE (
    term_id integer,
    company_id integer,
    term_name varchar,
    calculation_type varchar,
    due_days integer,
    discount_percentage numeric(5,2),
    discount_days integer,
    created_at timestamptz,
    updated_at timestamptz,
    total_records bigint
) AS $$
BEGIN
    RETURN QUERY
    WITH counted_data AS (
        SELECT COUNT(*) as total_count
        FROM cs_payment_terms
        WHERE company_id = p_company_id
    )
    SELECT 
        pt.term_id,
        pt.company_id,
        pt.term_name,
        pt.calculation_type,
        pt.due_days,
        pt.discount_percentage,
        pt.discount_days,
        pt.created_at,
        pt.updated_at,
        cd.total_count
    FROM cs_payment_terms pt
    CROSS JOIN counted_data cd
    WHERE pt.company_id = p_company_id
    ORDER BY pt.term_name
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;
