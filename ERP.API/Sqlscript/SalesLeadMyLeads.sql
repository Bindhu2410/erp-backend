-- Create stored procedure for My Leads functionality

CREATE OR REPLACE FUNCTION public.sp_sales_lead_my_leads(
    p_user_id INT,
    p_sort_field VARCHAR,
    p_sort_direction VARCHAR,
    p_page_number INT,
    p_page_size INT
)
RETURNS TABLE(
    id INT,
    customer_name VARCHAR,
    territory_name VARCHAR,
    status VARCHAR,
    score INT,
    lead_type VARCHAR,
    created_date TIMESTAMP,
    contact_name VARCHAR,
    contact_email VARCHAR,
    priority VARCHAR,
    total_count BIGINT
) AS $$
DECLARE
    v_offset INT;
    v_query TEXT;
    v_order_by TEXT;
    v_count_query TEXT;
    v_total_count BIGINT;
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Build the ORDER BY clause
    IF p_sort_field IS NOT NULL AND p_sort_field != '' THEN
        IF p_sort_direction = 'desc' THEN
            v_order_by := ' ORDER BY ' || p_sort_field || ' DESC';
        ELSE
            v_order_by := ' ORDER BY ' || p_sort_field || ' ASC';
        END IF;
    ELSE
        v_order_by := ' ORDER BY sl.created_date DESC';
    END IF;
    
    -- Count query for total results
    v_count_query := '
        SELECT COUNT(*) FROM sales_lead sl
        WHERE sl.user_id = ' || p_user_id;
    
    -- Execute count query
    EXECUTE v_count_query INTO v_total_count;
    
    -- Main query for results
    v_query := '
        SELECT 
            sl.id,
            sl.customer_name,
            sl.territory AS territory_name,  -- Direct text field
            sl.status,
            sl.score,
            sl.lead_type,
            sl.created_date,
            sl.contact_name,
            sl.contact_email,
            sl.priority,
            ' || v_total_count || ' AS total_count
        FROM sales_lead sl
        WHERE sl.user_id = ' || p_user_id;
    
    -- Add ORDER BY and LIMIT
    v_query := v_query || v_order_by || ' LIMIT ' || p_page_size || ' OFFSET ' || v_offset;
    
    -- Execute the query
    RETURN QUERY EXECUTE v_query;
END;
$$ LANGUAGE plpgsql;
