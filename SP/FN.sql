CREATE TABLE IF NOT EXISTS trigger_event_queue (
    id SERIAL PRIMARY KEY,
    table_name VARCHAR(255) NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    payload JSONB NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE OR REPLACE FUNCTION fn_get_leads_with_pagination(
    p_lead_id INTEGER DEFAULT NULL,
    p_search_text VARCHAR DEFAULT NULL,
    p_zones VARCHAR[] DEFAULT NULL,
    p_customer_names VARCHAR[] DEFAULT NULL,
    p_territories VARCHAR[] DEFAULT NULL,
    p_statuses VARCHAR[] DEFAULT NULL,
    p_scores VARCHAR[] DEFAULT NULL,
    p_lead_types VARCHAR[] DEFAULT NULL,
    p_page_number INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 10,
    p_order_by VARCHAR DEFAULT 'sales_leads.id',
    p_order_direction VARCHAR DEFAULT 'ASC'
)
RETURNS TABLE (
	total_records INTEGER,
    id INTEGER,
    user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    customer_name VARCHAR(255),
    lead_source VARCHAR(255),
    referral_source_name VARCHAR(255),
    hospital_of_referral VARCHAR(255),
    department_of_referral VARCHAR(255),
    social_media VARCHAR(255),
    event_date DATE,
    qualification_status VARCHAR(255),
    event_name VARCHAR(255),
    lead_id VARCHAR(255),
    status VARCHAR(255),
    score VARCHAR(255),
    isactive BOOLEAN,
    comments TEXT,
    lead_type VARCHAR(255),
    contact_name VARCHAR(100),
    salutation VARCHAR(10),
    contact_mobile_no BIGINT,
    land_line_no VARCHAR(15),
    email VARCHAR(100),
    fax VARCHAR(15),
    door_no VARCHAR(5),
    street VARCHAR(50),
    landmark VARCHAR(50),
    website VARCHAR(100),
    territory_id INTEGER,
    area_id INTEGER,
    city_id INTEGER,
    pincode_id INTEGER,
    city_of_referral_id INTEGER,
    district_id INTEGER,
    state_id INTEGER,
	city_name VARCHAR(100),
    area_name VARCHAR(100),
    pincode VARCHAR(15),
    state_name VARCHAR(100),
    district_name VARCHAR(100),
    territory_name VARCHAR(100)
)
AS $$
DECLARE
    v_offset INTEGER;
    v_where_clause TEXT := 'sales_leads.isactive = true';
    v_order_clause TEXT;
    v_query TEXT;
    v_total_records INTEGER;
BEGIN
    -- Pagination offset
    v_offset := (p_page_number - 1) * p_page_size;

    -- Build WHERE clause dynamically
    IF p_lead_id IS NOT NULL THEN
        v_where_clause := v_where_clause || ' AND sales_leads.id = ' || p_lead_id;
    END IF;

    IF p_search_text IS NOT NULL AND p_search_text != '' THEN
        v_where_clause := v_where_clause || ' AND (' ||
            'sales_leads.customer_name ILIKE ' || quote_literal('%' || p_search_text || '%') || ' OR ' ||
            'sales_cities.name ILIKE ' || quote_literal('%' || p_search_text || '%') || ' OR ' ||
            'sales_areas.name ILIKE ' || quote_literal('%' || p_search_text || '%') || ' OR ' ||
            'pincodes.pincode::TEXT ILIKE ' || quote_literal('%' || p_search_text || '%') || ' OR ' ||
            'sales_states.name ILIKE ' || quote_literal('%' || p_search_text || '%') || ' OR ' ||
            'sales_districts.name ILIKE ' || quote_literal('%' || p_search_text || '%') || ' OR ' ||
            'sales_territories.name ILIKE ' || quote_literal('%' || p_search_text || '%') || ')';
    END IF;

    IF p_zones IS NOT NULL AND array_length(p_zones, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sales_areas.name = ANY(''' || array_to_string(p_zones, ''',''') || ''')';
    END IF;

    IF p_customer_names IS NOT NULL AND array_length(p_customer_names, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sales_leads.customer_name = ANY(''' || array_to_string(p_customer_names, ''',''') || ''')';
    END IF;

    IF p_territories IS NOT NULL AND array_length(p_territories, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sales_territories.name = ANY(''' || array_to_string(p_territories, ''',''') || ''')';
    END IF;

    IF p_statuses IS NOT NULL AND array_length(p_statuses, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sales_leads.status = ANY(''' || array_to_string(p_statuses, ''',''') || ''')';
    END IF;

    IF p_scores IS NOT NULL AND array_length(p_scores, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sales_leads.score = ANY(''' || array_to_string(p_scores, ''',''') || ''')';
    END IF;

    IF p_lead_types IS NOT NULL AND array_length(p_lead_types, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sales_leads.lead_type = ANY(''' || array_to_string(p_lead_types, ''',''') || ''')';
    END IF;

    v_order_clause := ' ORDER BY ' || p_order_by || ' ' || p_order_direction;

    -- Get total record count
    EXECUTE format('
        SELECT COUNT(DISTINCT sales_leads.id)
        FROM sales_leads
        LEFT JOIN sales_cities ON sales_leads.city_id = sales_cities.id
        LEFT JOIN sales_areas ON sales_leads.area_id = sales_areas.id
        LEFT JOIN pincodes ON sales_leads.pincode_id = pincodes.id
        LEFT JOIN sales_states ON sales_leads.state_id = sales_states.id
        LEFT JOIN sales_districts ON sales_leads.district_id = sales_districts.id
        LEFT JOIN sales_territories ON sales_leads.territory_id = sales_territories.id
        WHERE %s', v_where_clause)
    INTO v_total_records;

    -- Return paginated result set with total_records attached to each row
    RETURN QUERY EXECUTE format('
        SELECT
            %s AS total_records,
            sales_leads.*,
            sales_cities.name AS city_name,
            sales_areas.name AS area_name,
            pincodes.pincode::VARCHAR AS pincode,
            sales_states.name AS state_name,
            sales_districts.name AS district_name,
            sales_territories.name AS territory_name
        FROM sales_leads
        LEFT JOIN sales_cities ON sales_leads.city_id = sales_cities.id
        LEFT JOIN sales_areas ON sales_leads.area_id = sales_areas.id
        LEFT JOIN pincodes ON sales_leads.pincode_id = pincodes.id
        LEFT JOIN sales_states ON sales_leads.state_id = sales_states.id
        LEFT JOIN sales_districts ON sales_leads.district_id = sales_districts.id
        LEFT JOIN sales_territories ON sales_leads.territory_id = sales_territories.id
        WHERE %s
        %s
        LIMIT %s OFFSET %s',
        v_total_records,
        v_where_clause,
        v_order_clause,
        p_page_size,
        v_offset
    );
END;
$$ LANGUAGE plpgsql;


-- Update the trigger function to handle NULLs and case sensitivity
CREATE OR REPLACE FUNCTION public.fn_queue_event()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
BEGIN
    -- Handle different event types
    IF TG_OP = 'DELETE' THEN
        INSERT INTO trigger_event_queue (table_name, event_type, payload)
        VALUES (TG_TABLE_NAME, TG_OP, to_jsonb(OLD));
        RETURN OLD;
    ELSE
        -- For INSERT and UPDATE, properly handle the columns
        INSERT INTO trigger_event_queue (table_name, event_type, payload)
        VALUES (TG_TABLE_NAME, TG_OP, to_jsonb(NEW));
        RETURN NEW;
    END IF;
END;
$BODY$;


-- Function to set up triggers on specific tables
CREATE OR REPLACE FUNCTION fn_setup_trigger_specific_tables(tables TEXT[])
RETURNS void AS $$
DECLARE
    tbl TEXT;
    trigger_name TEXT;
BEGIN
    FOREACH tbl IN ARRAY tables LOOP
        trigger_name := format('trigger_workflow_notify_%s', tbl);

        IF NOT EXISTS (
            SELECT 1 FROM pg_trigger
            WHERE tgname = trigger_name
              AND tgrelid = tbl::regclass
        ) THEN
            RAISE NOTICE 'Creating trigger % on table: %', trigger_name, tbl;

            EXECUTE format('
                CREATE TRIGGER %I
                AFTER INSERT OR UPDATE OR DELETE ON %I
                FOR EACH ROW
                EXECUTE FUNCTION fn_queue_event()',
                trigger_name, tbl
            );
        ELSE
            RAISE NOTICE 'Trigger % already exists on table: %', trigger_name, tbl;
        END IF;
    END LOOP;
END;
$$ LANGUAGE plpgsql;

