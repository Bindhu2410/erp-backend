-- Create stored procedure to join sales_leads, sales_contacts, and sales_addresses with lead ID parameter
CREATE OR REPLACE PROCEDURE SP_get_sales_leads_details(
    IN p_lead_id INTEGER DEFAULT NULL,
    IN p_search_text VARCHAR DEFAULT NULL,
    IN p_zones VARCHAR[] DEFAULT NULL,
    IN p_customer_names VARCHAR[] DEFAULT NULL,
    IN p_territories VARCHAR[] DEFAULT NULL,
    IN p_statuses VARCHAR[] DEFAULT NULL,
    IN p_scores VARCHAR[] DEFAULT NULL,
    IN p_lead_types VARCHAR[] DEFAULT NULL,
    IN p_page_number INTEGER DEFAULT 1,
    IN p_page_size INTEGER DEFAULT 10,
    IN p_order_by VARCHAR DEFAULT 'Id',
    IN p_order_direction VARCHAR DEFAULT 'ASC',
    OUT p_total_records INTEGER
)
LANGUAGE plpgsql
AS $$
DECLARE 
    v_offset INTEGER;
    v_where_clause TEXT;
    v_order_clause TEXT;
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;

    -- Build the WHERE clause dynamically
    v_where_clause := 'sales_lead."inActive" = false';
    
    -- Add lead ID filter if provided
    IF p_lead_id IS NOT NULL THEN
        v_where_clause := v_where_clause || ' AND sales_leads.id = ' || p_lead_id;
    END IF;

    -- Add free text search if provided
    IF p_search_text IS NOT NULL AND p_search_text != '' THEN
        v_where_clause := v_where_clause || format($$ 
            AND (
                sales_leads."customerName" ILIKE '%%' || %L || '%%' OR
                sales_leads."leadSource" ILIKE '%%' || %L || '%%' OR
                sales_contacts."contactName" ILIKE '%%' || %L || '%%' OR
                sales_addresses.city ILIKE '%%' || %L || '%%' OR
                sales_addresses.territory ILIKE '%%' || %L || '%%'
            )$$, p_search_text, p_search_text, p_search_text, p_search_text, p_search_text
        );
    END IF;

    -- Add array filters if provided
    IF p_zones IS NOT NULL AND array_length(p_zones, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sales_addresses.area = ANY($1)';
    END IF;
    
    IF p_customer_names IS NOT NULL AND array_length(p_customer_names, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sales_leads."customerName" = ANY($2)';
    END IF;
    
    IF p_territories IS NOT NULL AND array_length(p_territories, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sales_addresses.territory = ANY($3)';
    END IF;
    
    IF p_statuses IS NOT NULL AND array_length(p_statuses, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sales_leads.status = ANY($4)';
    END IF;
    
    IF p_scores IS NOT NULL AND array_length(p_scores, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sales_leads.score = ANY($5)';
    END IF;
    
    IF p_lead_types IS NOT NULL AND array_length(p_lead_types, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sales_leads."leadType" = ANY($6)';
    END IF;

    -- Build ORDER BY clause
    v_order_clause := format(' ORDER BY %I %s', p_order_by, p_order_direction);

    -- Get total count first
    EXECUTE format('
        SELECT COUNT(DISTINCT sales_leads.id) 
        FROM public.sales_lead
        LEFT JOIN public.sales_contacts ON sales_contacts."sales_leadsID" = sales_leads.id AND sales_contacts."inActive" = false
        LEFT JOIN public.sales_addresses ON sales_addresses."sales_leadsID" = sales_leads.id AND sales_addresses."inActive" = false
        WHERE %s', v_where_clause)
    INTO p_total_records
    USING p_zones, p_customer_names, p_territories, p_statuses, p_scores, p_lead_types;

    -- Create temp table for results
    CREATE TEMP TABLE IF NOT EXISTS temp_sales_leads_details AS
    EXECUTE format('
    SELECT DISTINCT
        sales_leads.id AS Id,
        sales_leads."customerName" AS CustomerName,
        sales_leads."leadSource" AS LeadSource,
        sales_leads."leadType" AS LeadType,
        sales_leads.status AS Status,
        sales_leads.score AS Score,
        sales_leads.website AS Website,
        sales_leads.fax AS Fax,
        sales_leads.salutation AS Salutation,
        sales_leads."qualificationStatus" AS QualificationStatus,
        sales_contacts.id AS ContactId,
        sales_contacts."contactName" AS ContactName,
        sales_contacts.email AS Email,
        sales_contacts."mobileNo" AS MobileNo,
        sales_contacts."landLineNo" AS LandLineNo,
        sales_addresses.id AS AddressId,
        sales_addresses."type" AS AddressType,
        sales_addresses."doorNo" AS DoorNo,
        sales_addresses.street AS Street,
        sales_addresses."landMark" AS LandMark,
        sales_addresses.area AS Area,
        sales_addresses.city AS City,
        sales_addresses.state AS State,
        sales_addresses.pincode AS Pincode,
        sales_addresses.territory AS Territory
    FROM 
        public.sales_leads
        LEFT JOIN public.sales_contacts ON sales_contacts."sales_leadsID" = sales_leads.id AND sales_contacts."inActive" = false
        LEFT JOIN public.sales_addresses ON sales_addresses."sales_leadsID" = sales_leads.id AND sales_addresses."inActive" = false
    WHERE %s
    %s
    LIMIT %s OFFSET %s',
    v_where_clause,
    v_order_clause,
    p_page_size,
    v_offset)
    USING p_zones, p_customer_names, p_territories, p_statuses, p_scores, p_lead_types;

    -- Return the results
    RETURN QUERY SELECT * FROM temp_sales_leads_details;
    
    -- Clean up
    DROP TABLE IF EXISTS temp_sales_leads_details;
END;
$$;

-- Example of how to call the stored procedure with pagination and filters:
/*
CALL SP_get_sales_leads_details(
    p_lead_id := NULL,
    p_search_text := 'search term',
    p_zones := ARRAY['Zone1', 'Zone2'],
    p_customer_names := ARRAY['Customer1', 'Customer2'],
    p_territories := ARRAY['Territory1'],
    p_statuses := ARRAY['Active', 'Pending'],
    p_scores := ARRAY['High', 'Medium'],
    p_lead_types := ARRAY['Direct', 'Referral'],
    p_page_number := 1,
    p_page_size := 10,
    p_order_by := 'CustomerName',
    p_order_direction := 'ASC'
);
*/


CREATE OR REPLACE FUNCTION get_product_dropdown_options()
RETURNS TABLE (
    make_id INTEGER,
    make_name VARCHAR(255),
    model_id INTEGER,
    model_name VARCHAR(255),
    product_id INTEGER,
    product_name VARCHAR(255),
    category_id INTEGER,
    category_name VARCHAR(255),
    item_code VARCHAR(255),
    item_name VARCHAR(255)
) AS $$
BEGIN
    -- Debug: Look for the specific SAMSUNG monitor
    RAISE NOTICE 'Looking for SAMSUNG monitor items';
    PERFORM i.id, i.item_code, i.item_name
    FROM inventory_items i
        INNER JOIN makes m ON i.make_id = m.id AND m.name = 'SAMSUNG'
        INNER JOIN models md ON i.model_id = md.id AND md.name = '24"'
        INNER JOIN products p ON i.product_id = p.id AND p.name = 'Monitor'
    WHERE i.isactive = true;
    
    IF NOT FOUND THEN
        RAISE NOTICE 'No matching SAMSUNG monitor items found';
    ELSE 
        RAISE NOTICE 'Found matching SAMSUNG monitor items';
    END IF;

    -- Main query modified to outer join from makes to inventory_items
    RETURN QUERY
    SELECT DISTINCT
        m.id as make_id,
        m.name as make_name,
        md.id as model_id,
        md.name as model_name,
        p.id as product_id,
        p.name as product_name,
        c.id as category_id,
        c.name as category_name,
        COALESCE(i.item_code, '') as item_code,
        COALESCE(i.item_name, '') as item_name
    FROM makes m
        INNER JOIN models md ON EXISTS (
            SELECT 1 FROM inventory_items ii 
            WHERE ii.make_id = m.id AND ii.model_id = md.id AND ii.isactive = true
        )
        INNER JOIN products p ON EXISTS (
            SELECT 1 FROM inventory_items ii 
            WHERE ii.make_id = m.id AND ii.product_id = p.id AND ii.isactive = true
        )
        INNER JOIN categories c ON EXISTS (
            SELECT 1 FROM inventory_items ii 
            WHERE ii.make_id = m.id AND ii.category_id = c.id AND ii.isactive = true
        )
        LEFT JOIN inventory_items i ON i.make_id = m.id 
            AND i.model_id = md.id 
            AND i.product_id = p.id 
            AND i.category_id = c.id
            AND i.isactive = true
    ORDER BY 
        m.name,
        md.name,
        p.name,
        c.name;
END;
$$ LANGUAGE plpgsql;
