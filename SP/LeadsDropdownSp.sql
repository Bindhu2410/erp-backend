CREATE OR REPLACE FUNCTION get_leads_dropdown(
    p_search_text VARCHAR DEFAULT NULL,
    p_page_number INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 10
)
RETURNS TABLE (
    total_records INTEGER,
    id INTEGER,
    lead_id VARCHAR,
    customer_name VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INTEGER := (p_page_number - 1) * p_page_size;
    v_total_records INTEGER;
    v_sql TEXT;
    v_params TEXT := '';
BEGIN    -- Construct search filter
    IF p_search_text IS NOT NULL AND p_search_text <> '' THEN
        v_params := format(
            ' AND ("customer_name" ILIKE %L OR CAST("lead_id" AS TEXT) ILIKE %L)',
            '%' || p_search_text || '%',
            '%' || p_search_text || '%'
        );
    END IF;

    -- Get total record count
    EXECUTE '        SELECT COUNT(*) FROM sales_lead WHERE isactive = true' || v_params
    INTO v_total_records;

    -- Fetch paginated results
    v_sql := format($f$        SELECT %s AS total_records, id, "lead_id" as lead_id, "customer_name" as customer_name        FROM sales_lead
        WHERE isactive = true%s
        ORDER BY "customer_name", "lead_id"
        LIMIT %s OFFSET %s
    $f$,
        v_total_records,
        v_params,
        p_page_size,
        v_offset
    );

    RETURN QUERY EXECUTE v_sql;
END;
$$;
