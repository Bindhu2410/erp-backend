-- Combined Sales Lead Dashboard Functions
-- This file contains all stored procedures needed for the Sales Lead Dashboard

-----------------------
-- SUMMARY CARDS
-----------------------

-- Returns counts for summary cards: Total Leads, New This Week, Qualified Leads, Converted Leads
CREATE OR REPLACE FUNCTION public.sp_sales_lead_summary_cards()
RETURNS TABLE (
    total_leads INTEGER,
    total_leads_growth NUMERIC,
    new_this_week INTEGER,
    new_this_week_growth NUMERIC,
    qualified_leads INTEGER,
    qualification_rate NUMERIC,
    converted_leads INTEGER,
    conversion_rate NUMERIC
) AS
$$
DECLARE
    v_last_month_count INTEGER;
    v_last_week_count INTEGER;
    v_total_qualified INTEGER;
BEGIN
    -- Calculate total leads
    SELECT COUNT(*) INTO total_leads
    FROM public.sales_lead
    WHERE isactive = true;
    
    -- Calculate total leads from last month for growth rate
    SELECT COUNT(*) INTO v_last_month_count
    FROM public.sales_lead
    WHERE isactive = true
    AND date_created <= (now() - INTERVAL '1 month');
    
    -- Calculate growth percentage (vs last month)
    IF v_last_month_count > 0 THEN
        total_leads_growth := ((total_leads - v_last_month_count)::NUMERIC / v_last_month_count) * 100;
    ELSE
        total_leads_growth := 0;
    END IF;
    
    -- New leads this week
    SELECT COUNT(*) INTO new_this_week
    FROM public.sales_lead
    WHERE isactive = true
    AND date_created >= (now() - INTERVAL '7 days');
    
    -- Last week's new leads count
    SELECT COUNT(*) INTO v_last_week_count
    FROM public.sales_lead
    WHERE isactive = true
    AND date_created BETWEEN (now() - INTERVAL '14 days') AND (now() - INTERVAL '7 days');
    
    -- Calculate growth percentage (vs last week)
    IF v_last_week_count > 0 THEN
        new_this_week_growth := ((new_this_week - v_last_week_count)::NUMERIC / v_last_week_count) * 100;
    ELSE
        new_this_week_growth := 0;
    END IF;
    
    -- Qualified leads count
    SELECT COUNT(*) INTO qualified_leads
    FROM public.sales_lead
    WHERE isactive = true
    AND qualification_status = 'Qualified';
    
    -- Calculate qualification rate
    IF total_leads > 0 THEN
        qualification_rate := (qualified_leads::NUMERIC / total_leads) * 100;
    ELSE
        qualification_rate := 0;
    END IF;
    
    -- Converted leads count
    SELECT COUNT(*) INTO converted_leads
    FROM public.sales_lead
    WHERE isactive = true
    AND converted_customer_id IS NOT NULL;
    
    -- Calculate conversion rate
    IF total_leads > 0 THEN
        conversion_rate := (converted_leads::NUMERIC / total_leads) * 100;
    ELSE
        conversion_rate := 0;
    END IF;
    
    RETURN NEXT;
END;
$$ LANGUAGE plpgsql;

-----------------------
-- LEAD FILTERING
-----------------------

-- Allows filtering by Territory, Customer Name, Status, Score, Lead Type
-- Plus pagination, sorting, and row count
CREATE OR REPLACE FUNCTION public.sp_sales_lead_filter(
    p_territory VARCHAR DEFAULT NULL,
    p_customer_name VARCHAR DEFAULT NULL,
    p_status VARCHAR DEFAULT NULL,
    p_score VARCHAR DEFAULT NULL,
    p_lead_type VARCHAR DEFAULT NULL,
    p_sort_field VARCHAR DEFAULT 'id',
    p_sort_direction VARCHAR DEFAULT 'ASC',
    p_page_number INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 10
)
RETURNS TABLE (
    id INTEGER,
    customer_name VARCHAR,
    territory_name VARCHAR,
    status VARCHAR,
    score INTEGER,
    lead_type VARCHAR,
    created_date TIMESTAMP,
    contact_name VARCHAR,
    contact_email VARCHAR,
    priority VARCHAR,
    total_count BIGINT
) AS
$$
DECLARE
    v_offset INTEGER;
    v_where_clause TEXT := 'WHERE isactive = true';
    v_order_clause TEXT;
    v_count_query TEXT;
    v_query TEXT;
BEGIN
    -- Build WHERE clause based on filter parameters
    IF p_territory IS NOT NULL THEN
        v_where_clause := v_where_clause || ' AND territory ILIKE ''%' || p_territory || '%''';
    END IF;
    
    IF p_customer_name IS NOT NULL THEN
        v_where_clause := v_where_clause || ' AND customer_name ILIKE ''%' || p_customer_name || '%''';
    END IF;
    
    IF p_status IS NOT NULL THEN
        v_where_clause := v_where_clause || ' AND status = ''' || p_status || '''';
    END IF;
    
    IF p_score IS NOT NULL THEN
        v_where_clause := v_where_clause || ' AND score = ''' || p_score || '''';
    END IF;
      IF p_lead_type IS NOT NULL THEN
        v_where_clause := v_where_clause || ' AND lead_type = ''' || p_lead_type || '''';
    END IF;
    
    -- Sanitize sort field to prevent SQL injection
    CASE LOWER(p_sort_field)
        WHEN 'id' THEN p_sort_field := 'id';
        WHEN 'customer_name' THEN p_sort_field := 'customer_name';
        WHEN 'territory_name' THEN p_sort_field := 'territory';
        WHEN 'contact_name' THEN p_sort_field := 'contact_name';
        WHEN 'contact_email' THEN p_sort_field := 'email';
        WHEN 'status' THEN p_sort_field := 'status';
        WHEN 'score' THEN p_sort_field := 'score';
        WHEN 'lead_type' THEN p_sort_field := 'lead_type';
        WHEN 'created_date' THEN p_sort_field := 'created_date';
        WHEN 'priority' THEN p_sort_field := 'priority';
        ELSE p_sort_field := 'id';
    END CASE;
    
    -- Sanitize sort direction
    CASE UPPER(p_sort_direction)
        WHEN 'ASC' THEN p_sort_direction := 'ASC';
        WHEN 'DESC' THEN p_sort_direction := 'DESC';
        ELSE p_sort_direction := 'ASC';
    END CASE;
    
    v_order_clause := ' ORDER BY ' || p_sort_field || ' ' || p_sort_direction;
    
    -- Calculate offset for pagination
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Get total count for pagination
    v_count_query := 'SELECT COUNT(*) FROM public.sales_lead ' || v_where_clause;
    EXECUTE v_count_query INTO total_count;
    
    -- Build final query
    v_query := 'SELECT id, customer_name, territory AS territory_name, status, CAST(score AS INTEGER) AS score, lead_type, created_date, contact_name, email AS contact_email, priority, '
        || total_count || ' as total_count '
        || 'FROM public.sales_lead '
        || v_where_clause
        || v_order_clause
        || ' LIMIT ' || p_page_size || ' OFFSET ' || v_offset;
    
    -- Log the query for debugging
    RAISE NOTICE 'Executing query: %', v_query;
    
    -- Execute the query
    RETURN QUERY EXECUTE v_query;
END;
$$ LANGUAGE plpgsql;

-----------------------
-- MY LEADS
-----------------------

-- Returns leads assigned to the current user
CREATE OR REPLACE FUNCTION public.sp_sales_lead_my_leads(
    p_user_id INTEGER,
    p_sort_field VARCHAR DEFAULT 'id',
    p_sort_direction VARCHAR DEFAULT 'ASC',
    p_page_number INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 10
)
RETURNS TABLE (
    id INTEGER,
    customer_name VARCHAR,
    territory_name VARCHAR,
    status VARCHAR,
    score INTEGER,
    lead_type VARCHAR,
    created_date TIMESTAMP,
    contact_name VARCHAR,
    contact_email VARCHAR,
    priority VARCHAR,
    total_count BIGINT
) AS
$$
DECLARE
    v_offset INTEGER;
    v_where_clause TEXT := 'WHERE isactive = true AND user_id = ' || p_user_id;
    v_order_clause TEXT;
    v_count_query TEXT;
    v_query TEXT;
BEGIN
    -- Sanitize sort field to prevent SQL injection
    CASE LOWER(p_sort_field)
        WHEN 'id' THEN p_sort_field := 'id';
        WHEN 'customer_name' THEN p_sort_field := 'customer_name';
        WHEN 'territory_name' THEN p_sort_field := 'territory';
        WHEN 'contact_name' THEN p_sort_field := 'contact_name';
        WHEN 'contact_email' THEN p_sort_field := 'email';
        WHEN 'status' THEN p_sort_field := 'status';
        WHEN 'score' THEN p_sort_field := 'score';
        WHEN 'lead_type' THEN p_sort_field := 'lead_type';
        WHEN 'created_date' THEN p_sort_field := 'created_date';
        WHEN 'priority' THEN p_sort_field := 'priority';
        ELSE p_sort_field := 'id';
    END CASE;
    
    -- Sanitize sort direction
    CASE UPPER(p_sort_direction)
        WHEN 'ASC' THEN p_sort_direction := 'ASC';
        WHEN 'DESC' THEN p_sort_direction := 'DESC';
        ELSE p_sort_direction := 'ASC';
    END CASE;
    
    v_order_clause := ' ORDER BY ' || p_sort_field || ' ' || p_sort_direction;
    
    -- Calculate offset for pagination
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Get total count for pagination
    v_count_query := 'SELECT COUNT(*) FROM public.sales_lead ' || v_where_clause;
    EXECUTE v_count_query INTO total_count;
    
    -- Build final query
    v_query := 'SELECT id, customer_name, territory AS territory_name, status, CAST(score AS INTEGER) AS score, lead_type, created_date, contact_name, email AS contact_email, priority, '
        || total_count || ' as total_count '
        || 'FROM public.sales_lead '
        || v_where_clause
        || v_order_clause
        || ' LIMIT ' || p_page_size || ' OFFSET ' || v_offset;
    
    -- Execute the query
    RETURN QUERY EXECUTE v_query;
END;
$$ LANGUAGE plpgsql;

-----------------------
-- DROPDOWN OPTIONS
-----------------------

-- Returns distinct values for filter dropdowns
CREATE OR REPLACE FUNCTION public.sp_sales_lead_dropdown_options()
RETURNS TABLE (
    territories TEXT[],
    customers TEXT[],
    statuses TEXT[],
    scores TEXT[],
    lead_types TEXT[]
) AS
$$
BEGIN
    -- Get distinct values for territories
    SELECT ARRAY_AGG(DISTINCT territory) FILTER (WHERE territory IS NOT NULL) INTO territories 
    FROM public.sales_lead 
    WHERE isactive = true;
    
    -- Get distinct values for customers
    SELECT ARRAY_AGG(DISTINCT customer_name) FILTER (WHERE customer_name IS NOT NULL) INTO customers 
    FROM public.sales_lead 
    WHERE isactive = true;
    
    -- Get distinct values for statuses
    SELECT ARRAY_AGG(DISTINCT status) FILTER (WHERE status IS NOT NULL) INTO statuses 
    FROM public.sales_lead 
    WHERE isactive = true;
    
    -- Get distinct values for scores
    SELECT ARRAY_AGG(DISTINCT score) FILTER (WHERE score IS NOT NULL) INTO scores 
    FROM public.sales_lead 
    WHERE isactive = true;
    
    -- Get distinct values for lead types
    SELECT ARRAY_AGG(DISTINCT lead_type) FILTER (WHERE lead_type IS NOT NULL) INTO lead_types 
    FROM public.sales_lead 
    WHERE isactive = true;
    
    RETURN NEXT;
END;
$$ LANGUAGE plpgsql;
