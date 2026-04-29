-- Search SAC Codes with pagination
CREATE OR REPLACE FUNCTION public.sp_cs_sac_codes_search(
    p_company_id integer DEFAULT NULL,
    p_sac_code varchar DEFAULT NULL,
    p_description text DEFAULT NULL,
    p_page_size integer DEFAULT 10,
    p_page_number integer DEFAULT 1
)
RETURNS TABLE (
    sac_code_id integer,
    company_id integer,
    sac_code varchar,
    description text,
    default_gst_rate numeric(5,2),
    created_at timestamptz,
    updated_at timestamptz,
    total_records bigint
) AS $$
BEGIN
    RETURN QUERY
    WITH filtered_data AS (
        SELECT 
            sc.*,
            COUNT(*) OVER() as total_count
        FROM public.cs_sac_codes sc
        WHERE (p_company_id IS NULL OR sc.company_id = p_company_id)
        AND (p_sac_code IS NULL OR sc.sac_code ILIKE '%' || p_sac_code || '%')
        AND (p_description IS NULL OR sc.description ILIKE '%' || p_description || '%')
    )
    SELECT 
        fd.sac_code_id,
        fd.company_id,
        fd.sac_code,
        fd.description,
        fd.default_gst_rate,
        fd.created_at,
        fd.updated_at,
        fd.total_count
    FROM filtered_data fd
    ORDER BY fd.sac_code
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

-- Get SAC Code by ID
CREATE OR REPLACE FUNCTION public.sp_cs_sac_codes_get_by_id(
    p_sac_code_id integer
)
RETURNS TABLE (
    sac_code_id integer,
    company_id integer,
    sac_code varchar,
    description text,
    default_gst_rate numeric(5,2),
    created_at timestamptz,
    updated_at timestamptz
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        sc.sac_code_id,
        sc.company_id,
        sc.sac_code,
        sc.description,
        sc.default_gst_rate,
        sc.created_at,
        sc.updated_at
    FROM public.cs_sac_codes sc
    WHERE sc.sac_code_id = p_sac_code_id;
END;
$$ LANGUAGE plpgsql;

-- Create SAC Code
CREATE OR REPLACE FUNCTION public.sp_cs_sac_codes_create(
    p_company_id integer,
    p_sac_code varchar,
    p_description text,
    p_default_gst_rate numeric(5,2)
)
RETURNS integer AS $$
DECLARE
    v_sac_code_id integer;
BEGIN
    INSERT INTO public.cs_sac_codes(
        company_id,
        sac_code,
        description,
        default_gst_rate
    )
    VALUES (
        p_company_id,
        p_sac_code,
        p_description,
        p_default_gst_rate
    )
    RETURNING sac_code_id INTO v_sac_code_id;

    RETURN v_sac_code_id;
END;
$$ LANGUAGE plpgsql;

-- Update SAC Code
CREATE OR REPLACE FUNCTION public.sp_cs_sac_codes_update(
    p_sac_code_id integer,
    p_company_id integer,
    p_sac_code varchar,
    p_description text,
    p_default_gst_rate numeric(5,2)
)
RETURNS boolean AS $$
BEGIN
    UPDATE public.cs_sac_codes
    SET 
        company_id = p_company_id,
        sac_code = p_sac_code,
        description = p_description,
        default_gst_rate = p_default_gst_rate,
        updated_at = CURRENT_TIMESTAMP
    WHERE sac_code_id = p_sac_code_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- Delete SAC Code
CREATE OR REPLACE FUNCTION public.sp_cs_sac_codes_delete(
    p_sac_code_id integer
)
RETURNS boolean AS $$
BEGIN
    DELETE FROM public.cs_sac_codes
    WHERE sac_code_id = p_sac_code_id;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- Get SAC Codes by Company
CREATE OR REPLACE FUNCTION public.sp_cs_sac_codes_get_by_company(
    p_company_id integer,
    p_page_size integer DEFAULT 10,
    p_page_number integer DEFAULT 1
)
RETURNS TABLE (
    sac_code_id integer,
    company_id integer,
    sac_code varchar,
    description text,
    default_gst_rate numeric(5,2),
    created_at timestamptz,
    updated_at timestamptz,
    total_records bigint
) AS $$
BEGIN
    RETURN QUERY
    WITH counted_data AS (
        SELECT COUNT(*) as total_count
        FROM cs_sac_codes
        WHERE company_id = p_company_id
    )
    SELECT 
        sc.sac_code_id,
        sc.company_id,
        sc.sac_code,
        sc.description,
        sc.default_gst_rate,
        sc.created_at,
        sc.updated_at,
        cd.total_count
    FROM cs_sac_codes sc
    CROSS JOIN counted_data cd
    WHERE sc.company_id = p_company_id
    ORDER BY sc.sac_code
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;
