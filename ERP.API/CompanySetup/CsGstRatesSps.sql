-- Drop existing procedures if they exist
DROP PROCEDURE IF EXISTS sp_create_cs_gst_rate;
DROP PROCEDURE IF EXISTS sp_update_cs_gst_rate;
DROP PROCEDURE IF EXISTS sp_delete_cs_gst_rate;
DROP FUNCTION IF EXISTS sp_get_cs_gst_rate_by_id;
DROP FUNCTION IF EXISTS sp_get_cs_gst_rates_by_company;
DROP FUNCTION IF EXISTS sp_get_cs_gst_rate_by_hsn_sac;

-- Create a new GST rate
CREATE OR REPLACE PROCEDURE sp_create_cs_gst_rate(
    p_company_id INT,
    p_hsn_sac_code VARCHAR(20),
    p_is_hsn BOOLEAN,
    p_gst_rate NUMERIC(5,2),
    p_effective_date DATE,
    INOUT p_gst_rate_id INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Check if rate already exists for company, HSN/SAC code, type and effective date
    IF EXISTS (
        SELECT 1 
        FROM cs_gst_rates 
        WHERE company_id = p_company_id 
        AND hsn_sac_code = p_hsn_sac_code
        AND is_hsn = p_is_hsn
        AND effective_date = p_effective_date
    ) THEN
        RAISE EXCEPTION 'GST rate already exists for this company, code, type and effective date';
    END IF;

    -- Insert new GST rate
    INSERT INTO cs_gst_rates (
        company_id,
        hsn_sac_code,
        is_hsn,
        gst_rate,
        effective_date
    )
    VALUES (
        p_company_id,
        p_hsn_sac_code,
        p_is_hsn,
        p_gst_rate,
        p_effective_date
    )
    RETURNING gst_rate_id INTO p_gst_rate_id;
END;
$$;

-- Update an existing GST rate
CREATE OR REPLACE PROCEDURE sp_update_cs_gst_rate(
    p_gst_rate_id INT,
    p_company_id INT,
    p_hsn_sac_code VARCHAR(20),
    p_is_hsn BOOLEAN,
    p_gst_rate NUMERIC(5,2),
    p_effective_date DATE,
    INOUT p_success BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    p_success := FALSE;
    
    -- Check if rate exists with different ID for same company, code, type and date
    IF EXISTS (
        SELECT 1 
        FROM cs_gst_rates 
        WHERE company_id = p_company_id 
        AND hsn_sac_code = p_hsn_sac_code
        AND is_hsn = p_is_hsn
        AND effective_date = p_effective_date
        AND gst_rate_id != p_gst_rate_id
    ) THEN
        RAISE EXCEPTION 'Another GST rate already exists for this company, code, type and effective date';
    END IF;

    -- Update GST rate
    UPDATE cs_gst_rates
    SET company_id = p_company_id,
        hsn_sac_code = p_hsn_sac_code,
        is_hsn = p_is_hsn,
        gst_rate = p_gst_rate,
        effective_date = p_effective_date,
        updated_at = CURRENT_TIMESTAMP
    WHERE gst_rate_id = p_gst_rate_id;

    IF FOUND THEN
        p_success := TRUE;
    END IF;
END;
$$;

-- Delete a GST rate
CREATE OR REPLACE PROCEDURE sp_delete_cs_gst_rate(
    p_gst_rate_id INT,
    INOUT p_success BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    p_success := FALSE;
    
    DELETE FROM cs_gst_rates
    WHERE gst_rate_id = p_gst_rate_id;
    
    IF FOUND THEN
        p_success := TRUE;
    END IF;
END;
$$;

-- Get a GST rate by ID
CREATE OR REPLACE FUNCTION sp_get_cs_gst_rate_by_id(
    p_gst_rate_id INT
)
RETURNS TABLE (
    gst_rate_id INT,
    company_id INT,
    hsn_sac_code VARCHAR(20),
    is_hsn BOOLEAN,
    gst_rate NUMERIC(5,2),
    effective_date DATE,
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        gr.gst_rate_id,
        gr.company_id,
        gr.hsn_sac_code,
        gr.is_hsn,
        gr.gst_rate,
        gr.effective_date,
        gr.created_at,
        gr.updated_at
    FROM cs_gst_rates gr
    WHERE gr.gst_rate_id = p_gst_rate_id;
END;
$$;

-- Get GST rates by company with optional search
CREATE OR REPLACE FUNCTION sp_get_cs_gst_rates_by_company(
    p_company_id INT,
    p_search_text VARCHAR = NULL,
    p_is_hsn BOOLEAN = NULL,
    p_effective_date DATE = NULL,
    OUT total_records INT,
    OUT filtered_records INT
)
RETURNS SETOF RECORD
LANGUAGE plpgsql
AS $$
BEGIN
    -- Get total records for the company
    SELECT COUNT(*)
    INTO total_records
    FROM cs_gst_rates
    WHERE company_id = p_company_id;

    -- Get filtered records count
    SELECT COUNT(*)
    INTO filtered_records
    FROM cs_gst_rates gr
    WHERE gr.company_id = p_company_id
    AND (p_is_hsn IS NULL OR gr.is_hsn = p_is_hsn)
    AND (p_effective_date IS NULL OR gr.effective_date <= p_effective_date)
    AND (
        p_search_text IS NULL
        OR CAST(gr.gst_rate AS VARCHAR) LIKE '%' || p_search_text || '%'
        OR gr.hsn_sac_code LIKE '%' || p_search_text || '%'
    );

    -- Return the results
    RETURN QUERY
    SELECT 
        gr.gst_rate_id,
        gr.company_id,
        gr.hsn_sac_code,
        gr.is_hsn,
        gr.gst_rate,
        gr.effective_date,
        gr.created_at,
        gr.updated_at,
        total_records,
        filtered_records
    FROM cs_gst_rates gr
    WHERE gr.company_id = p_company_id
    AND (p_is_hsn IS NULL OR gr.is_hsn = p_is_hsn)
    AND (p_effective_date IS NULL OR gr.effective_date <= p_effective_date)
    AND (
        p_search_text IS NULL
        OR CAST(gr.gst_rate AS VARCHAR) LIKE '%' || p_search_text || '%'
        OR gr.hsn_sac_code LIKE '%' || p_search_text || '%'
    )
    ORDER BY gr.hsn_sac_code, gr.effective_date DESC;
END;
$$;

-- Get latest GST rate by HSN/SAC code
CREATE OR REPLACE FUNCTION sp_get_cs_gst_rate_by_hsn_sac(
    p_company_id INT,
    p_hsn_sac_code VARCHAR(20),
    p_is_hsn BOOLEAN,
    p_effective_date DATE
)
RETURNS TABLE (
    gst_rate_id INT,
    company_id INT,
    hsn_sac_code VARCHAR(20),
    is_hsn BOOLEAN,
    gst_rate NUMERIC(5,2),
    effective_date DATE,
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        gr.gst_rate_id,
        gr.company_id,
        gr.hsn_sac_code,
        gr.is_hsn,
        gr.gst_rate,
        gr.effective_date,
        gr.created_at,
        gr.updated_at
    FROM cs_gst_rates gr
    WHERE gr.company_id = p_company_id
    AND gr.hsn_sac_code = p_hsn_sac_code
    AND gr.is_hsn = p_is_hsn
    AND gr.effective_date <= p_effective_date
    ORDER BY gr.effective_date DESC
    LIMIT 1;
END;
$$;
