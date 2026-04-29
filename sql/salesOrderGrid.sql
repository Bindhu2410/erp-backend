DROP FUNCTION IF EXISTS fn_get_sales_orders_grid;

CREATE OR REPLACE FUNCTION fn_get_sales_orders_grid(
    p_request json
)
RETURNS SETOF RECORD
LANGUAGE plpgsql
AS $$
DECLARE
    v_search_text TEXT;
    v_customer_names TEXT[];
    v_statuses TEXT[];
    v_order_ids TEXT[];
    v_page_number INTEGER;
    v_page_size INTEGER;
    v_order_by TEXT;
    v_order_direction TEXT;
    v_offset INTEGER;
    v_where_clause TEXT := ' WHERE 1=1 ';
    v_order_clause TEXT;
    v_total_records INTEGER;
BEGIN    -- Extract parameters from JSON
    v_search_text := p_request->>'searchText';
    v_customer_names := ARRAY(SELECT jsonb_array_elements_text(CAST(p_request AS jsonb)->'customerNames'));
    v_statuses := ARRAY(SELECT jsonb_array_elements_text(CAST(p_request AS jsonb)->'statuses'));
    v_order_ids := ARRAY(SELECT jsonb_array_elements_text(CAST(p_request AS jsonb)->'orderIds'));
    v_page_number := COALESCE((p_request->>'pageNumber')::INTEGER, 1);
    v_page_size := COALESCE((p_request->>'pageSize')::INTEGER, 10);
    v_order_by := COALESCE(p_request->>'orderBy', 'order_date');
    v_order_direction := COALESCE(p_request->>'orderDirection', 'DESC');    -- Calculate offset
    v_offset := (v_page_number - 1) * v_page_size;

    -- Build dynamic WHERE clause
    IF v_search_text IS NOT NULL AND v_search_text != '' AND v_search_text != 'string' THEN
        v_where_clause := v_where_clause || format(
            ' AND (
                LOWER(so.order_id) LIKE ''%%'' || LOWER(%L) || ''%%'' OR
                LOWER(c.name) LIKE ''%%'' || LOWER(%L) || ''%%'' OR
                LOWER(so.status) LIKE ''%%'' || LOWER(%L) || ''%%'' OR
                LOWER(so.po_id) LIKE ''%%'' || LOWER(%L) || ''%%''
            )',
            v_search_text, v_search_text, v_search_text, v_search_text
        );
    END IF;

    -- Add filter conditions
    IF array_length(v_customer_names, 1) > 0 AND NOT (array_length(v_customer_names, 1) = 1 AND v_customer_names[1] = 'string') THEN
        v_where_clause := v_where_clause || format(' AND c.name = ANY(%L)', v_customer_names);
    END IF;

    IF array_length(v_statuses, 1) > 0 AND NOT (array_length(v_statuses, 1) = 1 AND v_statuses[1] = 'string') THEN
        v_where_clause := v_where_clause || format(' AND so.status = ANY(%L)', v_statuses);
    END IF;

    IF array_length(v_order_ids, 1) > 0 AND NOT (array_length(v_order_ids, 1) = 1 AND v_order_ids[1] = 'string') THEN
        v_where_clause := v_where_clause || format(' AND so.order_id = ANY(%L)', v_order_ids);
    END IF;    -- Build ORDER BY clause
    v_order_clause := ' ORDER BY ' || 
        CASE lower(v_order_by)
            WHEN 'order_date' THEN 'so.order_date'
            WHEN 'customer_name' THEN 'c.name'
            WHEN 'order_id' THEN 'so.order_id'
            WHEN 'status' THEN 'so.status'
            WHEN 'grand_total' THEN 'so.grand_total'
            ELSE 'so.order_date'
        END || 
        CASE upper(v_order_direction)
            WHEN 'ASC' THEN ' ASC'
            ELSE ' DESC'
        END || 
        ' LIMIT ' || v_page_size || ' OFFSET ' || v_offset;

    -- Get total records    -- Get total records count
    EXECUTE format('
        SELECT COUNT(*) 
        FROM sales_orders so 
        LEFT JOIN sales_customers c ON so.customer_id = c.id
        %s', v_where_clause) 
    INTO v_total_records;    -- Return the result
    RETURN QUERY EXECUTE format('
        SELECT 
            %s::INTEGER as total_records,
            so.id::INTEGER,
            so.order_id::VARCHAR,
            c.name::VARCHAR as customer_name,
            so.order_date::TIMESTAMP WITH TIME ZONE,
            so.expected_delivery_date::TIMESTAMP WITH TIME ZONE,
            so.status::VARCHAR,
            so.po_id::VARCHAR,
            so.grand_total::NUMERIC(12,2)
        FROM sales_orders so
        LEFT JOIN sales_customers c ON so.customer_id = c.id
        %s
        %s',
        v_total_records,
        v_where_clause,
        v_order_clause);

END;
$$;
