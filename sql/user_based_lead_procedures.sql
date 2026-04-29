-- User-based Sales Lead Management Procedures
-- This script creates functions and updates constraints for user-based lead filtering

-- 1. Update foreign key constraints to reference users.userid instead of users.user_id
ALTER TABLE public.sales_lead DROP CONSTRAINT IF EXISTS fk_sales_lead_user_created;
ALTER TABLE public.sales_lead DROP CONSTRAINT IF EXISTS fk_sales_lead_user_updated;

-- Add new constraints referencing users.userid
ALTER TABLE public.sales_lead ADD CONSTRAINT fk_sales_lead_user_created 
    FOREIGN KEY (user_created) REFERENCES public.users(userid);
ALTER TABLE public.sales_lead ADD CONSTRAINT fk_sales_lead_user_updated 
    FOREIGN KEY (user_updated) REFERENCES public.users(userid);

-- 2. Create indexes for better performance on user-based queries
CREATE INDEX IF NOT EXISTS idx_sales_lead_user_created ON public.sales_lead(user_created);
CREATE INDEX IF NOT EXISTS idx_sales_lead_user_created_lead_id ON public.sales_lead(user_created, lead_id);
CREATE INDEX IF NOT EXISTS idx_sales_lead_lead_id ON public.sales_lead(lead_id);

-- 3. User-based Sales Lead Grid Function
-- This function filters leads by the current user (user_created field)
CREATE OR REPLACE FUNCTION sales_lead_grid_by_user(
    p_current_user_id INTEGER,
    p_search_text TEXT DEFAULT NULL,
    p_customer_names TEXT[] DEFAULT NULL,
    p_statuses TEXT[] DEFAULT NULL,
    p_scores TEXT[] DEFAULT NULL,
    p_lead_types TEXT[] DEFAULT NULL,
    p_selected_lead_ids TEXT[] DEFAULT NULL,
    p_page_number INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 10,
    p_order_by TEXT DEFAULT 'id',
    p_order_direction TEXT DEFAULT 'DESC'
)
RETURNS TABLE (
    id INTEGER,
    lead_id TEXT,
    customer_name TEXT,
    lead_source TEXT,
    referral_source_name TEXT,
    hospital_of_referral TEXT,
    department_of_referral TEXT,
    social_media TEXT,
    event_date DATE,
    event_name TEXT,
    status TEXT,
    score TEXT,
    isactive BOOLEAN,
    comments TEXT,
    lead_type TEXT,
    contact_name TEXT,
    salutation TEXT,
    contact_mobile_no TEXT,
    land_line_no TEXT,
    email TEXT,
    fax TEXT,
    door_no TEXT,
    street TEXT,
    landmark TEXT,
    website TEXT,
    territory TEXT,
    area TEXT,
    city TEXT,
    pincode TEXT,
    district TEXT,
    state TEXT,
    country TEXT,
    user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    total_records BIGINT
) AS $$
DECLARE
    where_conditions TEXT[] := ARRAY[]::TEXT[];
    where_clause TEXT := '';
    order_clause TEXT;
    final_query TEXT;
    offset_val INTEGER;
BEGIN
    -- Calculate offset
    offset_val := (p_page_number - 1) * p_page_size;
    
    -- Add search text condition
    IF p_search_text IS NOT NULL AND trim(p_search_text) != '' THEN
        where_conditions := where_conditions || ARRAY[
            format('(sl.customer_name ILIKE %L OR sl.lead_source ILIKE %L OR sl.contact_name ILIKE %L OR sl.contact_mobile_no ILIKE %L OR sl.email ILIKE %L OR sl.lead_id ILIKE %L)', 
                   '%' || p_search_text || '%', '%' || p_search_text || '%', '%' || p_search_text || '%', 
                   '%' || p_search_text || '%', '%' || p_search_text || '%', '%' || p_search_text || '%')
        ];
    END IF;
    
    -- Add filter conditions
    IF p_customer_names IS NOT NULL AND array_length(p_customer_names, 1) > 0 THEN
        where_conditions := where_conditions || ARRAY[format('sl.customer_name = ANY(%L)', p_customer_names)];
    END IF;
    
    IF p_statuses IS NOT NULL AND array_length(p_statuses, 1) > 0 THEN
        where_conditions := where_conditions || ARRAY[format('sl.status = ANY(%L)', p_statuses)];
    END IF;
    
    IF p_scores IS NOT NULL AND array_length(p_scores, 1) > 0 THEN
        where_conditions := where_conditions || ARRAY[format('sl.score = ANY(%L)', p_scores)];
    END IF;
    
    IF p_lead_types IS NOT NULL AND array_length(p_lead_types, 1) > 0 THEN
        where_conditions := where_conditions || ARRAY[format('sl.lead_type = ANY(%L)', p_lead_types)];
    END IF;
    
    IF p_selected_lead_ids IS NOT NULL AND array_length(p_selected_lead_ids, 1) > 0 THEN
        where_conditions := where_conditions || ARRAY[format('sl.lead_id = ANY(%L)', p_selected_lead_ids)];
    END IF;
    
    -- Build WHERE clause
    IF array_length(where_conditions, 1) > 0 THEN
        where_clause := ' AND ' || array_to_string(where_conditions, ' AND ');
    END IF;
    
    -- Build ORDER clause
    order_clause := format(' ORDER BY sl.%I %s', 
                          COALESCE(p_order_by, 'id'), 
                          CASE WHEN upper(COALESCE(p_order_direction, 'DESC')) = 'ASC' THEN 'ASC' ELSE 'DESC' END);
    
    -- Build final query
    final_query := format('
        SELECT sl.*, 
               COUNT(*) OVER() as total_records 
        FROM sales_lead sl 
        WHERE sl.user_created = %s %s %s
        LIMIT %s OFFSET %s',
        p_current_user_id,
        where_clause,
        order_clause,
        p_page_size,
        offset_val
    );
    
    -- Execute and return
    RETURN QUERY EXECUTE final_query;
END;
$$ LANGUAGE plpgsql;

-- 4. Function to get next user-specific lead ID
CREATE OR REPLACE FUNCTION get_next_user_lead_id(p_user_id INTEGER)
RETURNS TEXT AS $$
DECLARE
    last_lead_id TEXT;
    last_number INTEGER := 0;
    next_number INTEGER;
BEGIN
    -- Get the last lead ID for this user
    SELECT lead_id INTO last_lead_id
    FROM sales_lead 
    WHERE user_created = p_user_id 
      AND lead_id ~ '^LD[0-9]{5}$'
    ORDER BY lead_id DESC 
    LIMIT 1;
    
    -- Extract number and increment
    IF last_lead_id IS NOT NULL THEN
        last_number := substring(last_lead_id FROM 3)::INTEGER;
    END IF;
    
    next_number := last_number + 1;
    
    -- Return formatted lead ID
    RETURN 'LD' || lpad(next_number::TEXT, 5, '0');
END;
$$ LANGUAGE plpgsql;

-- 5. Function to get lead cards count for current user
CREATE OR REPLACE FUNCTION get_user_lead_cards_count(p_user_id INTEGER)
RETURNS TABLE (
    total_leads BIGINT,
    new_this_week BIGINT,
    qualified_leads BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*) AS total_leads,
        COUNT(*) FILTER (WHERE date_created >= date_trunc('week', CURRENT_DATE)) AS new_this_week,
        COUNT(*) FILTER (WHERE status = 'Qualified') AS qualified_leads
    FROM sales_lead
    WHERE isactive = true AND user_created = p_user_id;
END;
$$ LANGUAGE plpgsql;

-- 6. Update any existing sales tables to use the same foreign key pattern
-- Sales addresses
ALTER TABLE public.sales_addresses DROP CONSTRAINT IF EXISTS fk_sales_addresses_user_created;
ALTER TABLE public.sales_addresses DROP CONSTRAINT IF EXISTS fk_sales_addresses_user_updated;
ALTER TABLE public.sales_addresses ADD CONSTRAINT fk_sales_addresses_user_created
    FOREIGN KEY (user_created) REFERENCES public.users(userid);
ALTER TABLE public.sales_addresses ADD CONSTRAINT fk_sales_addresses_user_updated
    FOREIGN KEY (user_updated) REFERENCES public.users(userid);

-- Sales contacts
ALTER TABLE public.sales_contacts DROP CONSTRAINT IF EXISTS fk_sales_contacts_user_created;
ALTER TABLE public.sales_contacts DROP CONSTRAINT IF EXISTS fk_sales_contacts_user_updated;
ALTER TABLE public.sales_contacts ADD CONSTRAINT fk_sales_contacts_user_created
    FOREIGN KEY (user_created) REFERENCES public.users(userid);
ALTER TABLE public.sales_contacts ADD CONSTRAINT fk_sales_contacts_user_updated
    FOREIGN KEY (user_updated) REFERENCES public.users(userid);

-- Sales opportunities
ALTER TABLE public.sales_opportunities DROP CONSTRAINT IF EXISTS fk_sales_opportunities_user_created;
ALTER TABLE public.sales_opportunities DROP CONSTRAINT IF EXISTS fk_sales_opportunities_user_updated;
ALTER TABLE public.sales_opportunities ADD CONSTRAINT fk_sales_opportunities_user_created
    FOREIGN KEY (user_created) REFERENCES public.users(userid);
ALTER TABLE public.sales_opportunities ADD CONSTRAINT fk_sales_opportunities_user_updated
    FOREIGN KEY (user_updated) REFERENCES public.users(userid);

-- Grant necessary permissions
GRANT EXECUTE ON FUNCTION sales_lead_grid_by_user TO PUBLIC;
GRANT EXECUTE ON FUNCTION get_next_user_lead_id TO PUBLIC;
GRANT EXECUTE ON FUNCTION get_user_lead_cards_count TO PUBLIC;
