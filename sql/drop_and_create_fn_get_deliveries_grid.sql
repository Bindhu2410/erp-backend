DROP FUNCTION IF EXISTS fn_get_deliveries_grid(jsonb);

CREATE OR REPLACE FUNCTION fn_get_deliveries_grid(p_request jsonb)
RETURNS TABLE(
    total_records INTEGER,
    id INTEGER,
    user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    sales_order_id VARCHAR(50),
    po_id VARCHAR(50),
    delivery_id VARCHAR(50),
    delivery_date DATE,
    delivery_status VARCHAR(30),
    dispatch_address VARCHAR(100),
    priority VARCHAR(100),
    transporter_name VARCHAR(100),
    user_created_name TEXT,
    user_updated_name TEXT
) AS $$
DECLARE
    v_page_size INTEGER;
    v_offset INTEGER;
    v_search_text VARCHAR;
    v_statuses VARCHAR[];
    v_delivery_ids VARCHAR[];
    v_po_ids VARCHAR[];
    v_page_number INTEGER;
    v_order_by TEXT;
    v_order_direction TEXT;
    v_query TEXT;
BEGIN
    -- Extract values from JSON
    v_search_text := NULLIF(p_request->>'SearchText', 'string');
    v_statuses := ARRAY(SELECT jsonb_array_elements_text(COALESCE(p_request->'Statuses', '[]'::jsonb)));
    v_delivery_ids := ARRAY(SELECT jsonb_array_elements_text(COALESCE(p_request->'DeliveryIds', '[]'::jsonb)));
    v_po_ids := ARRAY(SELECT jsonb_array_elements_text(COALESCE(p_request->'PoIds', '[]'::jsonb)));
    v_page_number := COALESCE((p_request->>'PageNumber')::INTEGER, 1);
    v_page_size := LEAST(COALESCE((p_request->>'PageSize')::INTEGER, 10), 1000);
    v_order_by := COALESCE(p_request->>'OrderBy', 'date_created');
    v_order_direction := COALESCE(p_request->>'OrderDirection', 'DESC');
    v_offset := (v_page_number - 1) * v_page_size;

    v_query := '
        WITH base_data AS (
            SELECT 
                COUNT(*) OVER() as total_records,
                d.id,
                d.user_created,
                d.date_created,
                d.user_updated,
                d.date_updated,
                d.sales_order_id,
                d.po_id,
                d.delivery_id,
                d.delivery_date,
                d.delivery_status,
                d.dispatch_address,
                d.priority,
                d.transporter_name,
                CONCAT(u_created.firstname, '' '', u_created.lastname) as user_created_name,
                CONCAT(u_updated.firstname, '' '', u_updated.lastname) as user_updated_name
            FROM public.deliveries d
            LEFT JOIN users u_created ON d.user_created = u_created.userid
            LEFT JOIN users u_updated ON d.user_updated = u_updated.userid
            WHERE 1=1';

    -- Dynamic filters
    IF v_search_text IS NOT NULL AND v_search_text <> '' THEN
        v_query := v_query || ' AND (
            d.delivery_id ILIKE ''%'' || ' || quote_literal(v_search_text) || ' || ''%'' OR
            d.delivery_status ILIKE ''%'' || ' || quote_literal(v_search_text) || ' || ''%'' OR
            d.sales_order_id ILIKE ''%'' || ' || quote_literal(v_search_text) || ' || ''%'' OR
            d.po_id ILIKE ''%'' || ' || quote_literal(v_search_text) || ' || ''%'' OR
            d.dispatch_address ILIKE ''%'' || ' || quote_literal(v_search_text) || ' || ''%'' OR
            d.priority ILIKE ''%'' || ' || quote_literal(v_search_text) || ' || ''%'' OR
            d.transporter_name ILIKE ''%'' || ' || quote_literal(v_search_text) || ' || ''%''
        )';
    END IF;
    IF array_length(v_statuses, 1) IS NOT NULL AND NOT (array_length(v_statuses, 1) = 1 AND v_statuses[1] = 'string') THEN
        v_query := v_query || ' AND d.delivery_status = ANY(''' || array_to_string(v_statuses, ',') || '''::varchar[])';
    END IF;
    IF array_length(v_delivery_ids, 1) IS NOT NULL AND NOT (array_length(v_delivery_ids, 1) = 1 AND v_delivery_ids[1] = 'string') THEN
        v_query := v_query || ' AND d.delivery_id = ANY(''' || array_to_string(v_delivery_ids, ',') || '''::varchar[])';
    END IF;
    IF array_length(v_po_ids, 1) IS NOT NULL AND NOT (array_length(v_po_ids, 1) = 1 AND v_po_ids[1] = 'string') THEN
        v_query := v_query || ' AND d.po_id = ANY(''' || array_to_string(v_po_ids, ',') || '''::varchar[])';
    END IF;

    v_query := v_query || '
        )
        SELECT 
            total_records::INTEGER,
            id::INTEGER,
            user_created::INTEGER,
            date_created::TIMESTAMP,
            user_updated::INTEGER,
            date_updated::TIMESTAMP,
            sales_order_id::VARCHAR(50),
            po_id::VARCHAR(50),
            delivery_id::VARCHAR(50),
            delivery_date::DATE,
            delivery_status::VARCHAR(30),
            dispatch_address::VARCHAR(100),
            priority::VARCHAR(100),
            transporter_name::VARCHAR(100),
            user_created_name::TEXT,
            user_updated_name::TEXT
        FROM base_data
        ORDER BY COALESCE(date_updated, date_created) DESC, date_created DESC
        LIMIT ' || v_page_size || ' OFFSET ' || v_offset;

    RETURN QUERY EXECUTE v_query;
END;
$$ LANGUAGE plpgsql;
