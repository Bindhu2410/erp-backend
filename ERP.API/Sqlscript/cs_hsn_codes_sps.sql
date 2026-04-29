-- Create HSN Code
CREATE OR REPLACE PROCEDURE sp_create_cs_hsn_code(
    p_company_id INT,
    p_hsn_code VARCHAR(20),
    p_description TEXT,
    p_default_gst_rate NUMERIC(5,2),
    INOUT p_hsn_code_id INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO cs_hsn_codes (
        company_id,
        hsn_code,
        description,
        default_gst_rate
    ) VALUES (
        p_company_id,
        p_hsn_code,
        p_description,
        p_default_gst_rate
    )
    RETURNING hsn_code_id INTO p_hsn_code_id;
END;
$$;

CREATE OR REPLACE FUNCTION sp_get_cs_hsn_codes_by_company(
    p_company_id INT,
    p_search_text VARCHAR DEFAULT NULL
)
RETURNS TABLE (
    hsn_code_id INT,
    company_id INT,
    hsn_code VARCHAR(20),
    description TEXT,
    default_gst_rate NUMERIC(5,2),
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ,
    total_records INT,
    filtered_records INT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_total INT;
    v_filtered INT;
BEGIN
    -- Get total records
    SELECT COUNT(*) INTO v_total
    FROM cs_hsn_codes
    WHERE company_id = p_company_id;

    -- Get filtered data
    RETURN QUERY
    WITH filtered_data AS (
        SELECT *
        FROM cs_hsn_codes
        WHERE company_id = p_company_id
        AND (
            p_search_text IS NULL
            OR hsn_code ILIKE '%' || p_search_text || '%'
            OR description ILIKE '%' || p_search_text || '%'
        )
    )
    SELECT 
        hsn_code_id,
        company_id,
        hsn_code,
        description,
        default_gst_rate,
        created_at,
        updated_at,
        v_total AS total_records,
        COUNT(*) OVER() AS filtered_records
    FROM filtered_data;
END;
$$;


-- Get HSN Code by Company and Code
CREATE OR REPLACE FUNCTION sp_get_cs_hsn_code_by_company_and_code(
    p_company_id INT,
    p_hsn_code VARCHAR(20)
)
RETURNS TABLE (
    hsn_code_id INT,
    company_id INT,
    hsn_code VARCHAR(20),
    description TEXT,
    default_gst_rate NUMERIC(5,2),
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM cs_hsn_codes
    WHERE cs_hsn_codes.company_id = p_company_id
    AND cs_hsn_codes.hsn_code = p_hsn_code;
END;
$$;

-- Update HSN Code
CREATE OR REPLACE PROCEDURE sp_update_cs_hsn_code(
    p_hsn_code_id INT,
    p_company_id INT,
    p_hsn_code VARCHAR(20),
    p_description TEXT,
    p_default_gst_rate NUMERIC(5,2),
    INOUT p_success BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE cs_hsn_codes
    SET
        company_id = p_company_id,
        hsn_code = p_hsn_code,
        description = p_description,
        default_gst_rate = p_default_gst_rate,
        updated_at = CURRENT_TIMESTAMP
    WHERE hsn_code_id = p_hsn_code_id;

    GET DIAGNOSTICS p_success = ROW_COUNT;
    p_success := p_success > 0;
END;
$$;

-- Delete HSN Code
CREATE OR REPLACE PROCEDURE sp_delete_cs_hsn_code(
    p_hsn_code_id INT,
    INOUT p_success BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM cs_hsn_codes
    WHERE hsn_code_id = p_hsn_code_id;

    GET DIAGNOSTICS p_success = ROW_COUNT;
    p_success := p_success > 0;
END;
$$;

-- DROP FUNCTION public.sp_get_all_cs_hsn_codes();

CREATE OR REPLACE FUNCTION public.sp_get_all_cs_hsn_codes()
RETURNS TABLE(
    hsn_code_id INTEGER,
    company_id INTEGER,
    hsn_code VARCHAR,
    description TEXT,
    default_gst_rate NUMERIC(5,2),
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ,
    company_name VARCHAR
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT 
        h.hsn_code_id,
        h.company_id,
        h.hsn_code,
        h.description,
        h.default_gst_rate,
        h.created_at,
        h.updated_at,
        c.legal_company_name AS company_name
    FROM cs_hsn_codes h
    LEFT JOIN cs_companies c ON h.company_id = c.company_id
    ORDER BY c.legal_company_name, h.hsn_code;
END;
$function$;


-- DROP FUNCTION public.sp_get_cs_hsn_code_dropdown(integer);

CREATE OR REPLACE FUNCTION public.sp_get_cs_hsn_code_dropdown(p_company_id INTEGER DEFAULT NULL)
RETURNS TABLE(
    hsn_code_id INTEGER,
    hsn_code VARCHAR,
    description TEXT,
    default_gst_rate NUMERIC(5,2),
    display_name VARCHAR
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT
        h.hsn_code_id,
        h.hsn_code,
        h.description,
        h.default_gst_rate,
        CASE 
            WHEN h.description IS NOT NULL AND h.description <> '' THEN
                h.hsn_code || ' - ' || h.description
            ELSE
                h.hsn_code
        END AS display_name
    FROM cs_hsn_codes h
    WHERE p_company_id IS NULL OR h.company_id = p_company_id
    ORDER BY h.hsn_code;
END;
$function$;
