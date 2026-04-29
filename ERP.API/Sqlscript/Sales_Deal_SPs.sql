
CREATE TABLE public.sales_deals (
	id serial4 NOT NULL,
	user_created int4 NULL,
	date_created timestamp NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	status varchar(255) NULL,
	deal_name varchar(255) NULL,
	amount float8 NULL,
	expected_revenue float8 NULL,
	deal_age varchar(255) NULL,
	deal_for varchar(255) NULL,
	close_date date NULL,
	isactive bool DEFAULT false NULL,
	"comments" text NULL,
	opportunities_id int4 NULL,
	sales_representative_id int4 NULL,
	territory_id int4 NULL,
	area_id int4 NULL,
	city_id int4 NULL,
	district_id int4 NULL,
	state_id int4 NULL,
	pincode_id int4 NULL
);


-- SALES DEALS CRUD OPERATIONS

-- 1. CREATE: Insert a new deal
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
    OUT p_id INT4
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO public.sales_deals (
        user_created, date_created, status, deal_name, amount, 
        expected_revenue, deal_age, deal_for, close_date, isactive, 
        comments, opportunities_id, sales_representative_id, territory_id, 
        area_id, city_id, district_id, state_id, pincode_id
    )
    VALUES (
        p_user_created, NOW(), p_status, p_deal_name, p_amount, 
        p_expected_revenue, p_deal_age, p_deal_for, p_close_date, TRUE, 
        p_comments, p_opportunities_id, p_sales_representative_id, p_territory_id, 
        p_area_id, p_city_id, p_district_id, p_state_id, p_pincode_id
    )
    RETURNING id INTO p_id;
END;
$$;

-- 2. READ: Get deal by ID
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
    pincode_id INT
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
        sd.pincode_id
    FROM public.sales_deals sd
    WHERE sd.id = p_id AND sd.isactive = TRUE;
END;
$$ LANGUAGE plpgsql;

-- 3. UPDATE: Update an existing deal
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
        pincode_id = p_pincode_id
    WHERE id = p_id AND isactive = TRUE;
    
    IF FOUND THEN
        p_success := TRUE;
    ELSE
        p_success := FALSE;
    END IF;
END;
$$;

-- 4. DELETE: Soft delete a deal (mark as inactive)
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

-- SEARCH, FILTER, AND PAGINATION

-- 1. Get deals with filters and pagination
CREATE OR REPLACE FUNCTION public.fn_get_filtered_sales_deals(
    IN p_territory_id INT DEFAULT NULL,
    IN p_zone_id INT DEFAULT NULL, 
    IN p_customer_name VARCHAR(255) DEFAULT NULL,
    IN p_status VARCHAR(255) DEFAULT NULL,
    IN p_score VARCHAR(255) DEFAULT NULL,
    IN p_lead_type VARCHAR(255) DEFAULT NULL,
    IN p_search_term VARCHAR(255) DEFAULT NULL,
    IN p_sort_column VARCHAR(255) DEFAULT 'id',
    IN p_sort_direction VARCHAR(4) DEFAULT 'ASC',
    IN p_page_number INT DEFAULT 1,
    IN p_page_size INT DEFAULT 10
)
RETURNS TABLE (
    id INT,
    deal_id INT,
    clinic_hospital_individual VARCHAR(255),
    lead_id VARCHAR(255),
    amount FLOAT8,
    status VARCHAR(255),
    customer_name VARCHAR(255),
    closing_date DATE,
    territory VARCHAR(255),
    contact_name VARCHAR(255),
    payment_status VARCHAR(255),
    expected_revenue FLOAT8,
    deal_age VARCHAR(255),
    contact_phone VARCHAR(255)
) AS $$
DECLARE
    v_offset INT;
BEGIN
    v_offset := (p_page_number - 1) * p_page_size;
    
    RETURN QUERY
    SELECT 
        sd.id,
        sd.id AS deal_id,
        sd.deal_for AS clinic_hospital_individual,
        COALESCE(so.lead_id, 'LD-' || so.sales_leads_id::TEXT) AS lead_id,
        sd.amount,
        sd.status,
        COALESCE(so.customer_name, 'N/A') AS customer_name,
        sd.close_date AS closing_date,
        COALESCE(t.name, 'N/A') AS territory,
        COALESCE(so.contact_name, 'N/A') AS contact_name,
        'N/A' AS payment_status, -- Placeholder for actual payment status if available
        sd.expected_revenue,
        sd.deal_age,
        COALESCE(so.contact_mobile_no, 'N/A') AS contact_phone
    FROM 
        public.sales_deals sd
    LEFT JOIN 
        public.sales_opportunities so ON sd.opportunities_id = so.id
    LEFT JOIN
        public.territory t ON sd.territory_id = t.id
    WHERE 
        sd.isactive = TRUE
        AND (p_territory_id IS NULL OR sd.territory_id = p_territory_id)
        AND (p_status IS NULL OR sd.status = p_status)
        AND (p_lead_type IS NULL OR sd.deal_for = p_lead_type)
        AND (p_customer_name IS NULL OR 
             COALESCE(so.customer_name, '') ILIKE '%' || p_customer_name || '%')
        AND (p_search_term IS NULL OR 
             sd.deal_name ILIKE '%' || p_search_term || '%' OR
             COALESCE(sd.comments, '') ILIKE '%' || p_search_term || '%')
    ORDER BY
        CASE WHEN p_sort_column = 'id' AND p_sort_direction = 'ASC' THEN sd.id END ASC,
        CASE WHEN p_sort_column = 'id' AND p_sort_direction = 'DESC' THEN sd.id END DESC,
        CASE WHEN p_sort_column = 'deal_name' AND p_sort_direction = 'ASC' THEN sd.deal_name END ASC,
        CASE WHEN p_sort_column = 'deal_name' AND p_sort_direction = 'DESC' THEN sd.deal_name END DESC,
        CASE WHEN p_sort_column = 'amount' AND p_sort_direction = 'ASC' THEN sd.amount END ASC,
        CASE WHEN p_sort_column = 'amount' AND p_sort_direction = 'DESC' THEN sd.amount END DESC,
        CASE WHEN p_sort_column = 'close_date' AND p_sort_direction = 'ASC' THEN sd.close_date END ASC,
        CASE WHEN p_sort_column = 'close_date' AND p_sort_direction = 'DESC' THEN sd.close_date END DESC
    LIMIT p_page_size OFFSET v_offset;
END;
$$ LANGUAGE plpgsql;

-- 2. Count total deals matching filters (for pagination)
CREATE OR REPLACE FUNCTION public.fn_count_filtered_sales_deals(
    IN p_territory_id INT DEFAULT NULL,
    IN p_zone_id INT DEFAULT NULL,
    IN p_customer_name VARCHAR(255) DEFAULT NULL,
    IN p_status VARCHAR(255) DEFAULT NULL,
    IN p_score VARCHAR(255) DEFAULT NULL,
    IN p_lead_type VARCHAR(255) DEFAULT NULL,
    IN p_search_term VARCHAR(255) DEFAULT NULL
)
RETURNS INT AS $$
DECLARE
    v_count INT;
BEGIN
    SELECT 
        COUNT(*)
    INTO 
        v_count
    FROM 
        public.sales_deals sd
    LEFT JOIN 
        public.sales_opportunities so ON sd.opportunities_id = so.id
    WHERE 
        sd.isactive = TRUE
        AND (p_territory_id IS NULL OR sd.territory_id = p_territory_id)
        AND (p_status IS NULL OR sd.status = p_status)
        AND (p_lead_type IS NULL OR sd.deal_for = p_lead_type)
        AND (p_customer_name IS NULL OR 
             COALESCE(so.customer_name, '') ILIKE '%' || p_customer_name || '%')
        AND (p_search_term IS NULL OR 
             sd.deal_name ILIKE '%' || p_search_term || '%' OR
             COALESCE(sd.comments, '') ILIKE '%' || p_search_term || '%');
    
    RETURN v_count;
END;
$$ LANGUAGE plpgsql;

-- SUMMARY STATISTICS FOR DASHBOARD CARDS

-- Function to get deal summary statistics
CREATE OR REPLACE FUNCTION public.fn_get_deals_summary()
RETURNS TABLE (
    current_deals INT,
    won_deals INT,
    lost_deals INT,
    on_hold_deals INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        (SELECT COUNT(*) FROM public.sales_deals 
         WHERE status = 'In Progress' AND isactive = TRUE) AS current_deals,
        
        (SELECT COUNT(*) FROM public.sales_deals 
         WHERE status = 'Won' AND isactive = TRUE) AS won_deals,
        
        (SELECT COUNT(*) FROM public.sales_deals 
         WHERE status = 'Lost' AND isactive = TRUE) AS lost_deals,
        
        (SELECT COUNT(*) FROM public.sales_deals 
         WHERE status = 'On Hold' AND isactive = TRUE) AS on_hold_deals;
END;
$$ LANGUAGE plpgsql;




CREATE OR REPLACE FUNCTION public.fn_get_sales_deal_by_id(p_id integer)
RETURNS SETOF public.sales_deals
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT * FROM public.sales_deals WHERE id = p_id;
END;
$function$
;