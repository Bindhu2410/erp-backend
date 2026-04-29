-- Create Accounting Period
CREATE OR REPLACE FUNCTION public.sp_create_cs_accounting_period(
    p_company_id integer,
    p_period_name varchar(100),
    p_start_date date,
    p_end_date date,
    p_status varchar(20),
    p_is_current_active boolean
)
RETURNS TABLE(
    period_id integer,
    company_id integer,
    period_name varchar,
    start_date date,
    end_date date,
    status varchar,
    is_current_active boolean,
    created_at timestamptz,
    updated_at timestamptz
)
LANGUAGE plpgsql
AS $function$
BEGIN
    -- Update current active period if new period is set as active
    IF p_is_current_active THEN
        UPDATE cs_accounting_periods
        SET is_current_active = false
        WHERE company_id = p_company_id AND is_current_active = true;
    END IF;

    RETURN QUERY
    INSERT INTO cs_accounting_periods(
        company_id,
        period_name,
        start_date,
        end_date,
        status,
        is_current_active
    )
    VALUES (
        p_company_id,
        p_period_name,
        p_start_date,
        p_end_date,
        p_status,
        p_is_current_active
    )
    RETURNING
        period_id,
        company_id,
        period_name,
        start_date,
        end_date,
        status,
        is_current_active,
        created_at,
        updated_at;
END;
$function$;

-- Update Accounting Period
CREATE OR REPLACE FUNCTION public.sp_update_cs_accounting_period(
    p_period_id integer,
    p_period_name varchar(100),
    p_start_date date,
    p_end_date date,
    p_status varchar(20),
    p_is_current_active boolean
)
RETURNS TABLE(
    period_id integer,
    company_id integer,
    period_name varchar,
    start_date date,
    end_date date,
    status varchar,
    is_current_active boolean,
    created_at timestamptz,
    updated_at timestamptz
)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_company_id int4;
BEGIN
    -- Get company_id for the period
    SELECT company_id INTO v_company_id
    FROM cs_accounting_periods
    WHERE period_id = p_period_id;

    -- Update current active period if this period is set as active
    IF p_is_current_active THEN
        UPDATE cs_accounting_periods
        SET is_current_active = false
        WHERE company_id = v_company_id AND is_current_active = true;
    END IF;

    RETURN QUERY
    UPDATE cs_accounting_periods
    SET
        period_name = p_period_name,
        start_date = p_start_date,
        end_date = p_end_date,
        status = p_status,
        is_current_active = p_is_current_active,
        updated_at = CURRENT_TIMESTAMP
    WHERE period_id = p_period_id
    RETURNING
        period_id,
        company_id,
        period_name,
        start_date,
        end_date,
        status,
        is_current_active,
        created_at,
        updated_at;
END;
$function$;

-- Delete Accounting Period
CREATE OR REPLACE FUNCTION public.sp_delete_cs_accounting_period(p_period_id integer)
RETURNS boolean
LANGUAGE plpgsql
AS $function$
DECLARE
    v_status VARCHAR(20);
BEGIN
    -- Get the status of the period
    SELECT status INTO v_status
    FROM cs_accounting_periods
    WHERE period_id = p_period_id;

    -- Only allow deletion if period is not closed
    IF v_status = 'Closed' THEN
        RAISE EXCEPTION 'Cannot delete a closed accounting period';
    END IF;

    DELETE FROM cs_accounting_periods
    WHERE period_id = p_period_id;

    RETURN FOUND;
END;
$function$;

-- Get Accounting Period by ID
CREATE OR REPLACE FUNCTION public.sp_get_cs_accounting_period_by_id(p_period_id integer)
RETURNS TABLE(
    period_id integer,
    company_id integer,
    period_name varchar,
    start_date date,
    end_date date,
    status varchar,
    is_current_active boolean,
    created_at timestamptz,
    updated_at timestamptz
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT
        ap.period_id,
        ap.company_id,
        ap.period_name,
        ap.start_date,
        ap.end_date,
        ap.status,
        ap.is_current_active,
        ap.created_at,
        ap.updated_at
    FROM cs_accounting_periods ap
    WHERE ap.period_id = p_period_id;
END;
$function$;

-- Get Accounting Periods by Company
CREATE OR REPLACE FUNCTION public.sp_get_cs_accounting_periods_by_company(
    p_company_id integer,
    p_page_number integer DEFAULT 1,
    p_page_size integer DEFAULT 10
)
RETURNS TABLE(
    period_id integer,
    company_id integer,
    period_name varchar,
    start_date date,
    end_date date,
    status varchar,
    is_current_active boolean,
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
        FROM cs_accounting_periods
        WHERE company_id = p_company_id
    )
    SELECT
        ap.period_id,
        ap.company_id,
        ap.period_name,
        ap.start_date,
        ap.end_date,
        ap.status,
        ap.is_current_active,
        ap.created_at,
        ap.updated_at,
        c.total_count
    FROM cs_accounting_periods ap
    CROSS JOIN CountCTE c
    WHERE ap.company_id = p_company_id
    ORDER BY ap.start_date DESC
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
END;
$function$;

-- Search Accounting Periods
CREATE OR REPLACE FUNCTION public.sp_search_cs_accounting_periods(
    p_company_id integer,
    p_search_text varchar DEFAULT NULL,
    p_status varchar DEFAULT NULL,
    p_date date DEFAULT NULL,
    p_page_number integer DEFAULT 1,
    p_page_size integer DEFAULT 10
)
RETURNS TABLE(
    period_id integer,
    company_id integer,
    period_name varchar,
    start_date date,
    end_date date,
    status varchar,
    is_current_active boolean,
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
        FROM cs_accounting_periods ap
        WHERE ap.company_id = p_company_id
        AND (
            p_search_text IS NULL OR 
            ap.period_name ILIKE '%' || p_search_text || '%'
        )
        AND (p_status IS NULL OR ap.status = p_status)
        AND (p_date IS NULL OR (ap.start_date <= p_date AND ap.end_date >= p_date))
    )
    SELECT
        ap.period_id,
        ap.company_id,
        ap.period_name,
        ap.start_date,
        ap.end_date,
        ap.status,
        ap.is_current_active,
        ap.created_at,
        ap.updated_at,
        c.total_count
    FROM cs_accounting_periods ap
    CROSS JOIN CountCTE c
    WHERE ap.company_id = p_company_id
    AND (
        p_search_text IS NULL OR 
        ap.period_name ILIKE '%' || p_search_text || '%'
    )
    AND (p_status IS NULL OR ap.status = p_status)
    AND (p_date IS NULL OR (ap.start_date <= p_date AND ap.end_date >= p_date))
    ORDER BY ap.start_date DESC
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
END;
$function$;
