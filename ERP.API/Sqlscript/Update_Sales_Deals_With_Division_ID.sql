-- Updated stored procedures for SalesDeal to include division_id

-- Updated function for counting filtered deals
CREATE OR REPLACE FUNCTION public.fn_count_filtered_sales_deals(
    IN p_territory_id INT DEFAULT NULL,
    IN p_zone_id INT DEFAULT NULL,
    IN p_division_id INT DEFAULT NULL,
    IN p_customer_name VARCHAR DEFAULT NULL,
    IN p_status VARCHAR DEFAULT NULL,
    IN p_score VARCHAR DEFAULT NULL,
    IN p_lead_type VARCHAR DEFAULT NULL,
    IN p_search_term VARCHAR DEFAULT NULL
)
RETURNS INT
LANGUAGE plpgsql
AS $$
DECLARE
    v_count INT;
    query TEXT := 'SELECT COUNT(*) FROM sales_deals WHERE isactive = TRUE';
BEGIN
    -- Add filters if provided
    IF p_territory_id IS NOT NULL THEN
        query := query || ' AND territory_id = ' || p_territory_id;
    END IF;
    
    IF p_zone_id IS NOT NULL THEN
        -- Add appropriate join for zone filtering if applicable
        query := query || ' AND territory_id IN (SELECT id FROM sales_territories WHERE zone_id = ' || p_zone_id || ')';
    END IF;
    
    IF p_division_id IS NOT NULL THEN
        query := query || ' AND division_id = ' || p_division_id;
    END IF;
    
    IF p_customer_name IS NOT NULL AND p_customer_name <> '' THEN
        query := query || ' AND deal_name ILIKE ''%' || p_customer_name || '%''';
    END IF;
    
    IF p_status IS NOT NULL AND p_status <> '' THEN
        query := query || ' AND status = ''' || p_status || '''';
    END IF;
    
    IF p_score IS NOT NULL AND p_score <> '' THEN
        query := query || ' AND deal_age = ''' || p_score || '''';
    END IF;
    
    IF p_search_term IS NOT NULL AND p_search_term <> '' THEN
        query := query || ' AND (deal_name ILIKE ''%' || p_search_term || '%'' OR 
                            comments ILIKE ''%' || p_search_term || '%'' OR 
                            status ILIKE ''%' || p_search_term || '%'')';
    END IF;
    
    EXECUTE query INTO v_count;
    RETURN v_count;
END;
$$;

-- Updated function to get filtered deals
CREATE OR REPLACE FUNCTION public.fn_get_filtered_sales_deals(
    IN p_territory_id INT DEFAULT NULL,
    IN p_zone_id INT DEFAULT NULL,
    IN p_division_id INT DEFAULT NULL,
    IN p_customer_name VARCHAR DEFAULT NULL,
    IN p_status VARCHAR DEFAULT NULL,
    IN p_score VARCHAR DEFAULT NULL,
    IN p_lead_type VARCHAR DEFAULT NULL,
    IN p_search_term VARCHAR DEFAULT NULL,
    IN p_sort_column VARCHAR DEFAULT 'id',
    IN p_sort_direction VARCHAR DEFAULT 'ASC',
    IN p_page_number INT DEFAULT 1,
    IN p_page_size INT DEFAULT 10
)
RETURNS TABLE (
    id INT,
    deal_id INT,
    clinic_hospital_individual VARCHAR,
    lead_id VARCHAR,
    amount FLOAT8,
    status VARCHAR,
    customer_name VARCHAR,
    closing_date DATE,
    territory VARCHAR,
    contact_name VARCHAR,
    payment_status VARCHAR,
    expected_revenue FLOAT8,
    deal_age VARCHAR,
    contact_phone VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INT;
    query TEXT;
    order_clause TEXT;
BEGIN
    -- Calculate pagination offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Base query
    query := 'SELECT 
                sd.id,
                sd.id AS deal_id,
                sd.deal_for AS clinic_hospital_individual,
                so.opportunity_id AS lead_id,
                sd.amount,
                sd.status,
                sd.deal_name AS customer_name,
                sd.close_date AS closing_date,
                st.name AS territory,
                NULL AS contact_name, -- Add appropriate join if contact info is needed
                NULL AS payment_status, -- Add payment status if applicable
                sd.expected_revenue,
                sd.deal_age,
                NULL AS contact_phone -- Add appropriate join if contact info is needed
              FROM sales_deals sd
              LEFT JOIN sales_territories st ON sd.territory_id = st.id
              LEFT JOIN sales_opportunities so ON sd.opportunities_id = so.id
              WHERE sd.isactive = TRUE';
    
    -- Add filters if provided
    IF p_territory_id IS NOT NULL THEN
        query := query || ' AND sd.territory_id = ' || p_territory_id;
    END IF;
    
    IF p_zone_id IS NOT NULL THEN
        -- Add appropriate join for zone filtering if applicable
        query := query || ' AND sd.territory_id IN (SELECT id FROM sales_territories WHERE zone_id = ' || p_zone_id || ')';
    END IF;

    IF p_division_id IS NOT NULL THEN
        query := query || ' AND sd.division_id = ' || p_division_id;
    END IF;
    
    IF p_customer_name IS NOT NULL AND p_customer_name <> '' THEN
        query := query || ' AND sd.deal_name ILIKE ''%' || p_customer_name || '%''';
    END IF;
    
    IF p_status IS NOT NULL AND p_status <> '' THEN
        query := query || ' AND sd.status = ''' || p_status || '''';
    END IF;
    
    IF p_score IS NOT NULL AND p_score <> '' THEN
        query := query || ' AND sd.deal_age = ''' || p_score || '''';
    END IF;
    
    IF p_search_term IS NOT NULL AND p_search_term <> '' THEN
        query := query || ' AND (sd.deal_name ILIKE ''%' || p_search_term || '%'' OR 
                               sd.comments ILIKE ''%' || p_search_term || '%'' OR 
                               sd.status ILIKE ''%' || p_search_term || '%'')';
    END IF;
    
    -- Add ordering
    order_clause := ' ORDER BY ';
    CASE 
        WHEN p_sort_column = 'customer_name' THEN order_clause := order_clause || 'sd.deal_name';
        WHEN p_sort_column = 'amount' THEN order_clause := order_clause || 'sd.amount';
        WHEN p_sort_column = 'status' THEN order_clause := order_clause || 'sd.status';
        WHEN p_sort_column = 'closing_date' THEN order_clause := order_clause || 'sd.close_date';
        WHEN p_sort_column = 'territory' THEN order_clause := order_clause || 'st.name';
        ELSE order_clause := order_clause || 'sd.id';
    END CASE;
    
    IF p_sort_direction = 'DESC' THEN
        order_clause := order_clause || ' DESC';
    ELSE
        order_clause := order_clause || ' ASC';
    END IF;
    
    -- Add pagination
    order_clause := order_clause || ' LIMIT ' || p_page_size || ' OFFSET ' || v_offset;
    
    -- Execute query
    RETURN QUERY EXECUTE query || order_clause;
END;
$$;

-- Update the sp_create_sales_deal procedure
-- This procedure has already been defined in your input
/*
CREATE OR REPLACE PROCEDURE public.sp_create_sales_deal(
    IN p_user_created INT,
    IN p_status VARCHAR(255),
    IN p_deal_name VARCHAR(255),
    IN p_amount FLOAT8,
    IN p_expected_revenue FLOAT8,
    IN p_deal_age VARCHAR(255),
    IN p_deal_for VARCHAR(255),
    IN p_close_date DATE,
    IN p_comments TEXT,
    IN p_opportunities_id INT4,
    IN p_sales_representative_id INT4,
    IN p_territory_id INT4,
    IN p_area_id INT4,
    IN p_city_id INT4,
    IN p_district_id INT4,
    IN p_state_id INT4,
    IN p_pincode_id INT4,
    IN p_division_id INT4,
    OUT p_id INT4
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO public.sales_deals (
        user_created, date_created, status, deal_name, amount, 
        expected_revenue, deal_age, deal_for, close_date, isactive, 
        comments, opportunities_id, sales_representative_id, territory_id, 
        area_id, city_id, district_id, state_id, pincode_id, division_id
    )
    VALUES (
        p_user_created, NOW(), p_status, p_deal_name, p_amount, 
        p_expected_revenue, p_deal_age, p_deal_for, p_close_date, TRUE, 
        p_comments, p_opportunities_id, p_sales_representative_id, p_territory_id, 
        p_area_id, p_city_id, p_district_id, p_state_id, p_pincode_id, p_division_id
    )
    RETURNING id INTO p_id;
END;
$$;
*/

-- Update the fn_get_sales_deal_by_id function
-- This function has already been defined in your input
/*
CREATE OR REPLACE FUNCTION public.fn_get_sales_deal_by_id(p_id INT)
RETURNS TABLE (
    id INT,
    user_created INT,
    date_created TIMESTAMP,
    user_updated INT,
    date_updated TIMESTAMP,
    status VARCHAR(255),
    deal_name VARCHAR(255),
    amount FLOAT8,
    expected_revenue FLOAT8,
    deal_age VARCHAR(255),
    deal_for VARCHAR(255),
    close_date DATE,
    isactive BOOLEAN,
    comments TEXT,
    opportunities_id INT,
    sales_representative_id INT,
    territory_id INT,
    area_id INT,
    city_id INT,
    district_id INT,
    state_id INT,
    pincode_id INT,
    division_id INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        sd.id,
        sd.user_created,
        sd.date_created,
        sd.user_updated, 
        sd.date_updated,
        sd.status,
        sd.deal_name,
        sd.amount,
        sd.expected_revenue,
        sd.deal_age,
        sd.deal_for,
        sd.close_date,
        sd.isactive,
        sd.comments,
        sd.opportunities_id,
        sd.sales_representative_id,
        sd.territory_id,
        sd.area_id,
        sd.city_id,
        sd.district_id,
        sd.state_id,
        sd.pincode_id,
        sd.division_id
    FROM public.sales_deals sd
    WHERE sd.id = p_id AND sd.isactive = TRUE;
END;
$$ LANGUAGE plpgsql;
*/

-- Update the sp_update_sales_deal procedure
-- This procedure has already been defined in your input
/*
CREATE OR REPLACE PROCEDURE public.sp_update_sales_deal(
    IN p_id INT4,
    IN p_user_updated INT4,
    IN p_status VARCHAR(255),
    IN p_deal_name VARCHAR(255),
    IN p_amount FLOAT8,
    IN p_expected_revenue FLOAT8,
    IN p_deal_age VARCHAR(255),
    IN p_deal_for VARCHAR(255),
    IN p_close_date DATE,
    IN p_comments TEXT,
    IN p_opportunities_id INT4,
    IN p_sales_representative_id INT4,
    IN p_territory_id INT4,
    IN p_area_id INT4,
    IN p_city_id INT4,
    IN p_district_id INT4,
    IN p_state_id INT4,
    IN p_pincode_id INT4,
    IN p_division_id INT4,
    OUT p_success BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE public.sales_deals SET
        user_updated = p_user_updated,
        date_updated = NOW(),
        status = p_status,
        deal_name = p_deal_name,
        amount = p_amount,
        expected_revenue = p_expected_revenue,
        deal_age = p_deal_age,
        deal_for = p_deal_for,
        close_date = p_close_date,
        comments = p_comments,
        opportunities_id = p_opportunities_id,
        sales_representative_id = p_sales_representative_id,
        territory_id = p_territory_id,
        area_id = p_area_id,
        city_id = p_city_id,
        district_id = p_district_id,
        state_id = p_state_id,
        pincode_id = p_pincode_id,
        division_id = p_division_id
    WHERE id = p_id AND isactive = TRUE;

    IF FOUND THEN
        p_success := TRUE;
    ELSE
        p_success := FALSE;
    END IF;
END;
$$;
*/

-- Create or update the sp_delete_sales_deal procedure
CREATE OR REPLACE PROCEDURE public.sp_delete_sales_deal(
    IN p_id INT4,
    IN p_user_updated INT4,
    OUT p_success BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE public.sales_deals SET
        isactive = FALSE,
        user_updated = p_user_updated,
        date_updated = NOW()
    WHERE id = p_id AND isactive = TRUE;

    IF FOUND THEN
        p_success := TRUE;
    ELSE
        p_success := FALSE;
    END IF;
END;
$$;

-- Create or update the function for getting deals summary
CREATE OR REPLACE FUNCTION public.fn_get_deals_summary()
RETURNS TABLE (
    current_deals BIGINT,
    total_deals BIGINT,
    total_amount NUMERIC,
    total_expected_revenue NUMERIC,
    expected_revenue_this_month NUMERIC,
    closing_this_month BIGINT,
    overdue_closing BIGINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    current_month_start DATE := date_trunc('month', CURRENT_DATE)::DATE;
    current_month_end DATE := (date_trunc('month', CURRENT_DATE) + interval '1 month - 1 day')::DATE;
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*) FILTER(WHERE isactive = TRUE) AS current_deals,
        COUNT(*) AS total_deals,
        COALESCE(SUM(amount), 0) AS total_amount,
        COALESCE(SUM(expected_revenue), 0) AS total_expected_revenue,
        COALESCE(SUM(expected_revenue) FILTER(WHERE close_date BETWEEN current_month_start AND current_month_end), 0) AS expected_revenue_this_month,
        COUNT(*) FILTER(WHERE close_date BETWEEN current_month_start AND current_month_end) AS closing_this_month,
        COUNT(*) FILTER(WHERE close_date < CURRENT_DATE AND status <> 'Closed') AS overdue_closing
    FROM public.sales_deals;
END;
$$;
