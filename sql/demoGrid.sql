-- Drop existing functions if they exist
DROP FUNCTION IF EXISTS public.fn_get_demos_grid;
DROP FUNCTION IF EXISTS public.fn_get_sales_demos_grid;

CREATE OR REPLACE FUNCTION public.fn_get_sales_demos_grid(
    p_request jsonb
)
RETURNS TABLE (
    total_records INTEGER,
    id INTEGER,
    user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    user_id INTEGER,
    demo_date_time TIMESTAMP,
    status VARCHAR(100),
    customer_name VARCHAR(255),
    demo_name VARCHAR(255),
    demo_contact VARCHAR(255),
    demo_approach VARCHAR(255),
    demo_outcome VARCHAR(255),
    demo_feedback VARCHAR(255),    
    comments VARCHAR(255),
    opportunity_id INTEGER,
    presenter_id INTEGER,
    presenter_name TEXT,
    address_id INTEGER,
    customer_id INTEGER,
    opportunity_name VARCHAR(255),
    address_details TEXT,
    user_created_name TEXT,
    user_updated_name TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_page_size INTEGER;
    v_offset INTEGER;
    v_search_text VARCHAR;
    v_customer_names VARCHAR[];
    v_statuses VARCHAR[];
    v_demo_approaches VARCHAR[];
    v_demo_outcomes VARCHAR[];
    v_start_date TIMESTAMP;
    v_end_date TIMESTAMP;
    v_page_number INTEGER;
    v_order_by TEXT;
    v_order_direction TEXT;
    v_query TEXT;
BEGIN
    -- Extract values from JSON
    v_search_text := NULLIF(p_request->>'searchText', 'string');
    v_customer_names := ARRAY(SELECT jsonb_array_elements_text(COALESCE(p_request->'customerNames', '[]'::jsonb)));
    v_statuses := ARRAY(SELECT jsonb_array_elements_text(COALESCE(p_request->'statuses', '[]'::jsonb)));
    v_demo_approaches := ARRAY(SELECT jsonb_array_elements_text(COALESCE(p_request->'demoApproaches', '[]'::jsonb)));
    v_demo_outcomes := ARRAY(SELECT jsonb_array_elements_text(COALESCE(p_request->'demoOutcomes', '[]'::jsonb)));
    v_start_date := (p_request->>'startDate')::TIMESTAMP;
    v_end_date := (p_request->>'endDate')::TIMESTAMP;
    v_page_number := COALESCE((p_request->>'pageNumber')::INTEGER, 1);
    v_page_size := LEAST(COALESCE((p_request->>'pageSize')::INTEGER, 10), 1000);
    v_order_by := COALESCE(p_request->>'orderBy', 'date_created');
    v_order_direction := COALESCE(p_request->>'orderDirection', 'DESC');
      
    -- Calculate offset for pagination
    v_offset := (v_page_number - 1) * v_page_size;

    -- Build main query
    v_query := '
        WITH base_data AS (
            SELECT 
                COUNT(*) OVER() as total_records,
                sd.id,
                sd.user_created,
                sd.date_created,
                sd.user_updated,
                sd.date_updated,
                sd.user_id,
                sd.demo_date as demo_date_time,
                sd.status,
                sd.customer_name,
                sd.demo_name,
                sd.demo_contact,
                sd.demo_approach,
                sd.demo_outcome,
                sd.demo_feedback,
                sd.comments,
                sd.opportunity_id,
                sd.presenter_id,
                CONCAT(u_presenter.first_name, '' '', u_presenter.last_name) as presenter_name,
                sd.address_id,
                sd.customer_id,
                so.opportunity_name,
                CONCAT_WS('', '', 
                    NULLIF(sa.door_no, ''''),
                    NULLIF(sa.street, ''''),
                    NULLIF(sa.area, ''''),
                    NULLIF(sa.city, ''''),
                    NULLIF(sa.state, ''''),
                    NULLIF(sa.pincode, '''')
                ) as address_details,
                CONCAT(u_created.first_name, '' '', u_created.last_name) as user_created_name,
                CONCAT(u_updated.first_name, '' '', u_updated.last_name) as user_updated_name
            FROM 
                public.sales_demos sd
                LEFT JOIN users u_presenter ON sd.presenter_id = u_presenter.user_id
                LEFT JOIN users u_created ON sd.user_created = u_created.user_id
                LEFT JOIN users u_updated ON sd.user_updated = u_updated.user_id
                LEFT JOIN sales_opportunities so ON sd.opportunity_id = so.id
                LEFT JOIN sales_addresses sa ON sd.address_id = sa.id
            WHERE 1=1
    ';

    -- RAISE NOTICE for debugging
    RAISE NOTICE 'Final Query: %', v_query;

    -- Close CTE and select results
    v_query := v_query || '
        )
        SELECT 
            total_records::INTEGER,
            id::INTEGER,
            user_created::INTEGER,
            date_created::TIMESTAMP,
            user_updated::INTEGER,
            date_updated::TIMESTAMP,
            user_id::INTEGER,
            demo_date_time::TIMESTAMP,
            status::VARCHAR(100),
            customer_name::VARCHAR(255),
            demo_name::VARCHAR(255),
            demo_contact::VARCHAR(255),
            demo_approach::VARCHAR(255),
            demo_outcome::VARCHAR(255),
            demo_feedback::VARCHAR(255),
            comments::VARCHAR(255),
            opportunity_id::INTEGER,
            presenter_id::INTEGER,
            presenter_name::TEXT,
            address_id::INTEGER,
            customer_id::INTEGER,
            opportunity_name::VARCHAR(255),
            address_details::TEXT,
            user_created_name::TEXT,
            user_updated_name::TEXT
        FROM base_data
    ';

    -- Add ordering and pagination
    v_query := v_query || '
        ORDER BY COALESCE(date_updated, date_created) DESC, date_created DESC
        LIMIT ' || v_page_size || ' OFFSET ' || v_offset;

    -- Return query
    RETURN QUERY EXECUTE v_query;
END;
$$;
