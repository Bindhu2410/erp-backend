-- Stored Procedures for CRUD operations on sales_temp_lead

-- 1. Insert
CREATE OR REPLACE FUNCTION insert_sales_temp_lead(
    p_user_created INT,
    p_customer_name VARCHAR,
    p_lead_source VARCHAR,
    p_lead_id VARCHAR,
    p_status VARCHAR,
    p_score VARCHAR,
    p_isactive BOOLEAN,
    p_comments TEXT,
    p_lead_type VARCHAR,
    p_contact_name VARCHAR,
    p_salutation VARCHAR,
    p_contact_mobile_no VARCHAR,
    p_land_line_no VARCHAR,
    p_email VARCHAR,
    p_door_no VARCHAR,
    p_street VARCHAR,
    p_landmark VARCHAR,
    p_website VARCHAR,
    p_area VARCHAR,
    p_city VARCHAR,
    p_pincode VARCHAR,
    p_district VARCHAR,
    p_state VARCHAR,
    p_country VARCHAR
)
RETURNS INTEGER AS $$
DECLARE
    new_id INTEGER;
BEGIN
    INSERT INTO public.sales_temp_lead (
        user_created, date_created, customer_name, lead_source, lead_id, status, score, isactive, comments, lead_type,
        contact_name, salutation, contact_mobile_no, land_line_no, email, door_no, street, landmark, website, area,
        city, pincode, district, state, country
    ) VALUES (
        p_user_created, NOW(), p_customer_name, p_lead_source, p_lead_id, p_status, p_score, p_isactive, p_comments, p_lead_type,
        p_contact_name, p_salutation, p_contact_mobile_no, p_land_line_no, p_email, p_door_no, p_street, p_landmark, p_website, p_area,
        p_city, p_pincode, p_district, p_state, p_country
    ) RETURNING id INTO new_id;
    RETURN new_id;
END;
$$ LANGUAGE plpgsql;

-- 2. Update
CREATE OR REPLACE FUNCTION update_sales_temp_lead(
    p_id INT,
    p_user_updated INT,
    p_customer_name VARCHAR,
    p_lead_source VARCHAR,
    p_lead_id VARCHAR,
    p_status VARCHAR,
    p_score VARCHAR,
    p_isactive BOOLEAN,
    p_comments TEXT,
    p_lead_type VARCHAR,
    p_contact_name VARCHAR,
    p_salutation VARCHAR,
    p_contact_mobile_no VARCHAR,
    p_land_line_no VARCHAR,
    p_email VARCHAR,
    p_door_no VARCHAR,
    p_street VARCHAR,
    p_landmark VARCHAR,
    p_website VARCHAR,
    p_area VARCHAR,
    p_city VARCHAR,
    p_pincode VARCHAR,
    p_district VARCHAR,
    p_state VARCHAR,
    p_country VARCHAR
)
RETURNS VOID AS $$
BEGIN
    UPDATE public.sales_temp_lead SET
        user_updated = p_user_updated,
        date_updated = NOW(),
        customer_name = p_customer_name,
        lead_source = p_lead_source,
        lead_id = p_lead_id,
        status = p_status,
        score = p_score,
        isactive = p_isactive,
        comments = p_comments,
        lead_type = p_lead_type,
        contact_name = p_contact_name,
        salutation = p_salutation,
        contact_mobile_no = p_contact_mobile_no,
        land_line_no = p_land_line_no,
        email = p_email,
        door_no = p_door_no,
        street = p_street,
        landmark = p_landmark,
        website = p_website,
        area = p_area,
        city = p_city,
        pincode = p_pincode,
        district = p_district,
        state = p_state,
        country = p_country
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

-- 3. Delete
CREATE OR REPLACE FUNCTION delete_sales_temp_lead(p_id INT)
RETURNS VOID AS $$
BEGIN
    DELETE FROM public.sales_temp_lead WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

-- 4. Get by ID
CREATE OR REPLACE FUNCTION get_sales_temp_lead_by_id(p_id INT)
RETURNS TABLE (
    id INT,
    user_created INT,
    date_created TIMESTAMP,
    user_updated INT,
    date_updated TIMESTAMP,
    customer_name VARCHAR,
    lead_source VARCHAR,
    lead_id VARCHAR,
    status VARCHAR,
    score VARCHAR,
    isactive BOOLEAN,
    comments TEXT,
    lead_type VARCHAR,
    contact_name VARCHAR,
    salutation VARCHAR,
    contact_mobile_no VARCHAR,
    land_line_no VARCHAR,
    email VARCHAR,
    door_no VARCHAR,
    street VARCHAR,
    landmark VARCHAR,
    website VARCHAR,
    area VARCHAR,
    city VARCHAR,
    pincode VARCHAR,
    district VARCHAR,
    state VARCHAR,
    country VARCHAR
) AS $$
BEGIN
    RETURN QUERY SELECT 
        sales_temp_lead.id,
        sales_temp_lead.user_created,
        sales_temp_lead.date_created,
        sales_temp_lead.user_updated,
        sales_temp_lead.date_updated,
        sales_temp_lead.customer_name,
        sales_temp_lead.lead_source,
        sales_temp_lead.lead_id,
        sales_temp_lead.status,
        sales_temp_lead.score,
        sales_temp_lead.isactive,
        sales_temp_lead.comments,
        sales_temp_lead.lead_type,
        sales_temp_lead.contact_name,
        sales_temp_lead.salutation,
        sales_temp_lead.contact_mobile_no,
        sales_temp_lead.land_line_no,
        sales_temp_lead.email,
        sales_temp_lead.door_no,
        sales_temp_lead.street,
        sales_temp_lead.landmark,
        sales_temp_lead.website,
        sales_temp_lead.area,
        sales_temp_lead.city,
        sales_temp_lead.pincode,
        sales_temp_lead.district,
        sales_temp_lead.state,
        sales_temp_lead.country
    FROM public.sales_temp_lead WHERE sales_temp_lead.id = p_id;
END;
$$ LANGUAGE plpgsql;

-- 5. Get All
CREATE OR REPLACE FUNCTION get_all_sales_temp_leads()
RETURNS SETOF public.sales_temp_lead AS $$
BEGIN
    RETURN QUERY SELECT * FROM public.sales_temp_lead;
END;
$$ LANGUAGE plpgsql;
