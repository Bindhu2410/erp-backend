DROP FUNCTION IF EXISTS fn_get_sales_quotations_grid;

CREATE OR REPLACE FUNCTION fn_get_sales_quotations_grid(
    p_request json
)
RETURNS TABLE (
    total_records INTEGER,
    id INTEGER,
    user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    version VARCHAR,
    terms VARCHAR,
    valid_till TIMESTAMP,
    quotation_for VARCHAR,
    status VARCHAR,
    lost_reason VARCHAR,
    customer_id INTEGER,
    quotation_type VARCHAR,
    quotation_date TIMESTAMP,
    order_type VARCHAR,
    comments VARCHAR,
    delivery_within VARCHAR,
    delivery_after VARCHAR,
    is_active BOOLEAN,
    quotation_id VARCHAR,
    opportunity_id TEXT, -- changed from INTEGER to TEXT
    lead_id TEXT,        -- changed from INTEGER to TEXT
    customer_name VARCHAR,
    taxes VARCHAR,
    delivery VARCHAR,
    payment VARCHAR,
    warranty VARCHAR,
    freight_charge VARCHAR,
    is_current BOOLEAN,
    parent_sales_quotations_id INTEGER,
    products JSON
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_search_text TEXT;
    v_customer_names TEXT[];
    v_statuses TEXT[];
    v_quotation_ids TEXT[];
    v_page_number INTEGER;
    v_page_size INTEGER;
    v_offset INTEGER;
    v_where_clause TEXT;
    v_total_records INTEGER;
    v_base_query TEXT;
    v_order_by TEXT;
    v_order_direction TEXT;
BEGIN
    -- Extract parameters from JSON
    v_search_text := p_request->>'searchText';
    v_customer_names := ARRAY(SELECT jsonb_array_elements_text(CAST(p_request AS jsonb)->'customerNames'));
    v_statuses := ARRAY(SELECT jsonb_array_elements_text(CAST(p_request AS jsonb)->'statuses'));
    v_quotation_ids := ARRAY(SELECT jsonb_array_elements_text(CAST(p_request AS jsonb)->'quotationIds'));
    v_page_number := COALESCE((p_request->>'pageNumber')::INTEGER, 1);
    v_page_size := COALESCE((p_request->>'pageSize')::INTEGER, 10);
    v_order_by := COALESCE(p_request->>'orderBy', 'quotation_date');
    v_order_direction := COALESCE(p_request->>'orderDirection', 'DESC');

    -- Calculate offset for pagination
    v_offset := (v_page_number - 1) * v_page_size;

    -- Initialize WHERE clause
    v_where_clause := 'WHERE sq.is_active = true';    -- Add search filter with partial matching
    IF v_search_text IS NOT NULL AND v_search_text != '' AND v_search_text != 'string' THEN
        v_where_clause := v_where_clause || format(
            ' AND (
                LOWER(sq.customer_name) LIKE ''%%'' || LOWER(%L) || ''%%'' OR
                LOWER(sq.quotation_id) LIKE ''%%'' || LOWER(%L) || ''%%'' OR
                LOWER(sq.status) LIKE ''%%'' || LOWER(%L) || ''%%'' OR
                LOWER(sq.quotation_type) LIKE ''%%'' || LOWER(%L) || ''%%'' OR
                LOWER(sq.comments) LIKE ''%%'' || LOWER(%L) || ''%%'' OR
                LOWER(sq.delivery_within) LIKE ''%%'' || LOWER(%L) || ''%%''
            )',
            v_search_text, v_search_text, v_search_text, v_search_text, v_search_text, v_search_text
        );
    END IF;    -- Add array filters with partial matching support
    IF array_length(v_customer_names, 1) > 0 AND NOT (array_length(v_customer_names, 1) = 1 AND v_customer_names[1] = 'string') THEN
        v_where_clause := v_where_clause || format(
            ' AND EXISTS (
                SELECT 1 FROM unnest(%L::varchar[]) AS cn 
                WHERE LOWER(sq.customer_name) LIKE ''%%'' || LOWER(cn) || ''%%''
            )',
            v_customer_names
        );
    END IF;

    IF array_length(v_statuses, 1) > 0 AND NOT (array_length(v_statuses, 1) = 1 AND v_statuses[1] = 'string') THEN
        v_where_clause := v_where_clause || format(
            ' AND EXISTS (
                SELECT 1 FROM unnest(%L::varchar[]) AS s 
                WHERE LOWER(sq.status) LIKE ''%%'' || LOWER(s) || ''%%''
            )',
            v_statuses
        );
    END IF;

    IF array_length(v_quotation_ids, 1) > 0 AND NOT (array_length(v_quotation_ids, 1) = 1 AND v_quotation_ids[1] = 'string') THEN
        v_where_clause := v_where_clause || format(
            ' AND EXISTS (
                SELECT 1 FROM unnest(%L::varchar[]) AS qid 
                WHERE LOWER(sq.quotation_id) LIKE ''%%'' || LOWER(qid) || ''%%''
            )',
            v_quotation_ids
        );
    END IF;

    -- Base query for getting quotation data including products
    v_base_query := '
        FROM sales_quotations sq
        LEFT JOIN users u ON sq.user_created = u.user_id
    ' || v_where_clause;    -- Prepare ORDER BY clause
    v_order_by := CASE lower(v_order_by)
        WHEN 'quotation_date' THEN 'sq.quotation_date'
        WHEN 'customer_name' THEN 'sq.customer_name'
        WHEN 'quotation_id' THEN 'sq.quotation_id'
        WHEN 'status' THEN 'sq.status'
        WHEN 'quotation_type' THEN 'sq.quotation_type'
        ELSE 'sq.quotation_date'
    END;
    
    v_order_direction := CASE upper(v_order_direction)
        WHEN 'ASC' THEN 'ASC'
        ELSE 'DESC'
    END;    -- Get total record count
    EXECUTE format('SELECT COUNT(*) %s', v_base_query)
    INTO v_total_records;    -- Return main query results with products as JSON
    RETURN QUERY EXECUTE format('
        SELECT 
            %s AS total_records,
            sq.id,
            sq.user_created,
            sq.date_created,
            sq.user_updated,
            sq.date_updated,
            sq.version,
            sq.terms,
            sq.valid_till,
            sq.quotation_for,
            sq.status,
            sq.lost_reason,
            sq.customer_id,
            sq.quotation_type,
            sq.quotation_date,
            sq.order_type,
            sq.comments,
            sq.delivery_within,
            sq.delivery_after,
            sq.is_active,
            sq.quotation_id,
            sq.opportunity_id,
            sq.lead_id,
            sq.customer_name,
            sq.taxes,
            sq.delivery,
            sq.payment,
            sq.warranty,
            sq.freight_charge,
            sq.is_current,
            sq.parent_sales_quotations_id,
            COALESCE(
                (
                    SELECT json_agg(json_build_object(
                        ''id'', qp.id,
                        ''inventory_items_id'', qp.inventory_items_id,
                        ''qty'', qp.qty,
                        ''amount'', qp.amount,
                        ''isactive'', qp.isactive,
                        ''item_code'', i.item_code,
                        ''item_name'', i.item_name,
                        ''make_id'', i.make_id,
                        ''model_id'', i.model_id,
                        ''product_id'', i.product_id,
                        ''category_id'', i.category_id
                    ))
                    FROM quotation_products qp
                    LEFT JOIN inventory_items i ON qp.inventory_items_id = i.id
                    WHERE qp.quotation_id = sq.id AND qp.isactive = true
                ),
                ''[]''::json
            ) as products
        %s
        ORDER BY COALESCE(sq.date_updated, sq.date_created) DESC, sq.date_created DESC
        LIMIT %s 
        OFFSET %s',
        v_total_records,
        v_base_query,
        v_page_size,
        v_offset
    );
END;
$$;

-- Function to get quotation dropdown options
CREATE OR REPLACE FUNCTION fn_get_quotation_dropdown_options()
RETURNS TABLE (
    statuses TEXT[],
    customer_names TEXT[],
    quotation_types TEXT[]
) AS
$$
BEGIN
    -- Get distinct values for statuses
    SELECT array_agg(DISTINCT status) FILTER (WHERE status IS NOT NULL)
    INTO statuses 
    FROM sales_quotations 
    WHERE is_active = true;
    
    -- Get distinct values for customer names
    SELECT array_agg(DISTINCT customer_name) FILTER (WHERE customer_name IS NOT NULL)
    INTO customer_names 
    FROM sales_quotations 
    WHERE is_active = true;
    
    -- Get distinct values for quotation types
    SELECT array_agg(DISTINCT quotation_type) FILTER (WHERE quotation_type IS NOT NULL)
    INTO quotation_types 
    FROM sales_quotations 
    WHERE is_active = true;
    
    RETURN NEXT;
END;
$$ LANGUAGE plpgsql;
