-- Search TDS Rates with pagination
CREATE OR REPLACE FUNCTION public.sp_cs_tds_rates_search(
    p_company_id integer DEFAULT NULL,
    p_section_type varchar DEFAULT NULL,
    p_page_size integer DEFAULT 10,
    p_page_number integer DEFAULT 1
)
RETURNS TABLE (
    tds_rate_id integer,
    company_id integer,
    section_type varchar,
    threshold_amount numeric(18,2),
    rate numeric(5,2),
    effective_date date,
    created_at timestamptz,
    updated_at timestamptz,
    total_records bigint
) AS $$
BEGIN
    RETURN QUERY
    WITH filtered_data AS (
        SELECT 
            tr.*,
            COUNT(*) OVER() as total_count
        FROM public.cs_tds_rates tr
        WHERE (p_company_id IS NULL OR tr.company_id = p_company_id)
        AND (p_section_type IS NULL OR tr.section_type ILIKE '%' || p_section_type || '%')
    )
    SELECT 
        fd.tds_rate_id,
        fd.company_id,
        fd.section_type,
        fd.threshold_amount,
        fd.rate,
        fd.effective_date,
        fd.created_at,
        fd.updated_at,
        fd.total_count
    FROM filtered_data fd
    ORDER BY fd.effective_date DESC, fd.section_type
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

-- Get TDS Rate by ID
CREATE OR REPLACE FUNCTION public.sp_cs_tds_rates_get_by_id(
    p_tds_rate_id integer
)
RETURNS TABLE (
    tds_rate_id integer,
    company_id integer,
    section_type varchar,
    threshold_amount numeric(18,2),
    rate numeric(5,2),
    effective_date date,
    created_at timestamptz,
    updated_at timestamptz
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        tr.tds_rate_id,
        tr.company_id,
        tr.section_type,
        tr.threshold_amount,
        tr.rate,
        tr.effective_date,
        tr.created_at,
        tr.updated_at
    FROM public.cs_tds_rates tr
    WHERE tr.tds_rate_id = p_tds_rate_id;
END;
$$ LANGUAGE plpgsql;

-- Create TDS Rate
CREATE OR REPLACE FUNCTION public.sp_cs_tds_rates_create(
    p_company_id integer,
    p_section_type varchar,
    p_threshold_amount numeric(18,2),
    p_rate numeric(5,2),
    p_effective_date date
)
RETURNS integer AS $$
DECLARE
    v_tds_rate_id integer;
BEGIN
    INSERT INTO public.cs_tds_rates(
        company_id,
        section_type,
        threshold_amount,
        rate,
        effective_date
    )
    VALUES (
        p_company_id,
        p_section_type,
        p_threshold_amount,
        p_rate,
        p_effective_date
    )
    RETURNING tds_rate_id INTO v_tds_rate_id;

    RETURN v_tds_rate_id;
END;
$$ LANGUAGE plpgsql;

-- Update TDS Rate
CREATE OR REPLACE FUNCTION public.sp_cs_tds_rates_update(
    p_tds_rate_id integer,
    p_company_id integer,
    p_section_type varchar,
    p_threshold_amount numeric(18,2),
    p_rate numeric(5,2),
    p_effective_date date
)
RETURNS boolean AS $$
BEGIN
    UPDATE public.cs_tds_rates
    SET 
        company_id = p_company_id,
        section_type = p_section_type,
        threshold_amount = p_threshold_amount,
        rate = p_rate,
        effective_date = p_effective_date,
        updated_at = CURRENT_TIMESTAMP
    WHERE tds_rate_id = p_tds_rate_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- Delete TDS Rate
CREATE OR REPLACE FUNCTION public.sp_cs_tds_rates_delete(
    p_tds_rate_id integer
)
RETURNS boolean AS $$
BEGIN
    DELETE FROM public.cs_tds_rates
    WHERE tds_rate_id = p_tds_rate_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- Get TDS Rates by Company
CREATE OR REPLACE FUNCTION public.sp_cs_tds_rates_get_by_company(
    p_company_id integer,
    p_page_size integer DEFAULT 10,
    p_page_number integer DEFAULT 1
)
RETURNS TABLE (
    tds_rate_id integer,
    company_id integer,
    section_type varchar,
    threshold_amount numeric(18,2),
    rate numeric(5,2),
    effective_date date,
    created_at timestamptz,
    updated_at timestamptz,
    total_records bigint
) AS $$
BEGIN
    RETURN QUERY
    WITH counted_data AS (
        SELECT COUNT(*) as total_count
        FROM cs_tds_rates
        WHERE company_id = p_company_id
    )
    SELECT 
        tr.tds_rate_id,
        tr.company_id,
        tr.section_type,
        tr.threshold_amount,
        tr.rate,
        tr.effective_date,
        tr.created_at,
        tr.updated_at,
        cd.total_count
    FROM cs_tds_rates tr
    CROSS JOIN counted_data cd
    WHERE tr.company_id = p_company_id
    ORDER BY tr.effective_date DESC, tr.section_type
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;
