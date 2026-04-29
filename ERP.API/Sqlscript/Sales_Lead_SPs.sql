-- Function to get leads with pagination
CREATE OR REPLACE FUNCTION fn_get_leads_with_pagination(
    p_search_text TEXT DEFAULT NULL,
    p_zones TEXT[] DEFAULT NULL,
    p_customer_names TEXT[] DEFAULT NULL,
    p_territories TEXT[] DEFAULT NULL,
    p_statuses TEXT[] DEFAULT NULL,
    p_scores TEXT[] DEFAULT NULL,
    p_lead_types TEXT[] DEFAULT NULL,
    p_page_number INT DEFAULT 1,
    p_page_size INT DEFAULT 10,
    p_order_by TEXT DEFAULT 'created_date',
    p_order_direction TEXT DEFAULT 'DESC'
)
RETURNS TABLE (
    "TotalRecords" INT,
    "Id" INT,
    "UserCreated" INT,
    "DateCreated" TIMESTAMP,
    "UserUpdated" INT,
    "DateUpdated" TIMESTAMP,
    "CustomerName" TEXT,
    "LeadSource" TEXT,
    "ReferralSourceName" TEXT,
    "HospitalOfReferral" TEXT,
    "DepartmentOfReferral" TEXT,
    "SocialMedia" TEXT,
    "EventDate" TIMESTAMP,
    "QualificationStatus" TEXT,
    "EventName" TEXT,
    "LeadId" TEXT,
    "Status" TEXT,
    "Score" TEXT,
    "IsActive" BOOLEAN,
    "Comments" TEXT,
    "LeadType" TEXT,
    "ContactName" TEXT,
    "Salutation" TEXT,
    "ContactMobileNo" TEXT,
    "LandLineNo" TEXT,
    "Email" TEXT,
    "Fax" TEXT,
    "DoorNo" TEXT,
    "Street" TEXT,
    "Landmark" TEXT,
    "Website" TEXT,
    "TerritoryId" INT,
    "AreaId" INT,
    "CityId" INT,
    "PincodeId" INT,
    "CityOfReferralId" INT,
    "DistrictId" INT,
    "StateId" INT,
    "CityName" TEXT,
    "AreaName" TEXT,
    "Pincode" TEXT,
    "StateName" TEXT,
    "DistrictName" TEXT,
    "TerritoryName" TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_total_records INT;
    v_offset INT;
    v_where_clause TEXT := ' WHERE 1=1 ';
    v_count_query TEXT;
    v_main_query TEXT;
    v_order_clause TEXT;
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Build WHERE clause based on filters
    IF p_search_text IS NOT NULL AND p_search_text <> '' THEN
        v_where_clause := v_where_clause || ' AND (
            sl.customer_name ILIKE ''%' || p_search_text || '%'' OR
            sl.contact_name ILIKE ''%' || p_search_text || '%'' OR
            sl.lead_id ILIKE ''%' || p_search_text || '%'' OR
            sl.email ILIKE ''%' || p_search_text || '%'' OR
            sl.contact_mobile_no ILIKE ''%' || p_search_text || '%''
        )';
    END IF;
    
    IF p_zones IS NOT NULL AND array_length(p_zones, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND z.zone_name = ANY($1)';
    END IF;
    
    IF p_customer_names IS NOT NULL AND array_length(p_customer_names, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sl.customer_name = ANY($2)';
    END IF;      IF p_territories IS NOT NULL AND array_length(p_territories, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sl.territory = ANY($3)';
    END IF;
    
    IF p_statuses IS NOT NULL AND array_length(p_statuses, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sl.status = ANY($4)';
    END IF;
    
    IF p_scores IS NOT NULL AND array_length(p_scores, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sl.score = ANY($5)';
    END IF;
    
    IF p_lead_types IS NOT NULL AND array_length(p_lead_types, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sl.lead_type = ANY($6)';
    END IF;
    
    -- Build ORDER BY clause
    v_order_clause := ' ORDER BY ';
    
    IF p_order_by = 'customer_name' THEN
        v_order_clause := v_order_clause || 'sl.customer_name ';
    ELSIF p_order_by = 'lead_id' THEN
        v_order_clause := v_order_clause || 'sl.lead_id ';
    ELSIF p_order_by = 'status' THEN
        v_order_clause := v_order_clause || 'sl.status ';
    ELSIF p_order_by = 'score' THEN
        v_order_clause := v_order_clause || 'sl.score ';
    ELSIF p_order_by = 'lead_type' THEN
        v_order_clause := v_order_clause || 'sl.lead_type ';    ELSIF p_order_by = 'territory' THEN
        v_order_clause := v_order_clause || 'sl.territory ';
    ELSE
        v_order_clause := v_order_clause || 'sl.created_date ';
    END IF;
    
    IF p_order_direction = 'ASC' THEN
        v_order_clause := v_order_clause || 'ASC';
    ELSE
        v_order_clause := v_order_clause || 'DESC';
    END IF;
      -- Count query to get total records
    v_count_query := '        SELECT COUNT(*) 
        FROM sales_leads sl
        LEFT JOIN sales_territories t ON sl.territory = t.name
        ' || v_where_clause;
        
    -- Execute count query
    EXECUTE v_count_query 
    INTO v_total_records
    USING 
        p_zones,
        p_customer_names,
        p_territories,
        p_statuses,
        p_scores,
        p_lead_types;
    
    -- Main query to get paginated results
    v_main_query := '
    SELECT 
        ' || v_total_records || ' AS "TotalRecords",
        sl.id AS "Id",
        sl.user_created AS "UserCreated",
        sl.date_created AS "DateCreated",
        sl.user_updated AS "UserUpdated",
        sl.date_updated AS "DateUpdated",
        sl.customer_name AS "CustomerName",
        sl.territory AS "Territory",
        sl.lead_source AS "LeadSource",
        sl.referral_source_name AS "ReferralSourceName",
            sl.hospital_of_referral AS "HospitalOfReferral",
            sl.department_of_referral AS "DepartmentOfReferral",
            sl.social_media AS "SocialMedia",
            sl.event_date AS "EventDate",
            sl.qualification_status AS "QualificationStatus",
            sl.event_name AS "EventName",
            sl.lead_id AS "LeadId",
            sl.status AS "Status",
            sl.score AS "Score",
            sl.is_active AS "IsActive",
            sl.comments AS "Comments",
            sl.lead_type AS "LeadType",
            sl.contact_name AS "ContactName",
            sl.salutation AS "Salutation",
            sl.contact_mobile_no AS "ContactMobileNo",
            sl.land_line_no AS "LandLineNo",
            sl.email AS "Email",
            sl.fax AS "Fax",
            sl.door_no AS "DoorNo",
            sl.street AS "Street",
            sl.landmark AS "Landmark",
            sl.website AS "Website",
            sl.territory_id AS "TerritoryId",
            sl.area_id AS "AreaId",
            sl.city_id AS "CityId",
            sl.pincode_id AS "PincodeId",
            sl.city_of_referral_id AS "CityOfReferralId",
            sl.district_id AS "DistrictId",
            sl.state_id AS "StateId",
            c.city_name AS "CityName",
            a.area_name AS "AreaName",
            p.pincode AS "Pincode",            s.state_name AS "StateName",
            d.district_name AS "DistrictName",
            t.name AS "TerritoryName"        FROM sales_leads sl
        LEFT JOIN sales_territories t ON sl.territory_id = t.id
        LEFT JOIN zones z ON t.zone_id = z.id
        LEFT JOIN cities c ON sl.city_id = c.id
        LEFT JOIN areas a ON sl.area_id = a.id
        LEFT JOIN pincodes p ON sl.pincode_id = p.id
        LEFT JOIN states s ON sl.state_id = s.id
        LEFT JOIN districts d ON sl.district_id = d.id
        ' || v_where_clause || v_order_clause || '
        LIMIT ' || p_page_size || ' OFFSET ' || v_offset;
    
    -- Execute main query and return results
    RETURN QUERY EXECUTE v_main_query
    USING 
        p_zones,
        p_customer_names,
        p_territories,
        p_statuses,
        p_scores,
        p_lead_types;
END;
$$;

-- Function to get a sales lead by id
CREATE OR REPLACE FUNCTION fn_get_sales_lead_by_id(p_id INT)
RETURNS TABLE (
    "Id" INT,
    "UserCreated" INT,
    "DateCreated" TIMESTAMP,
    "UserUpdated" INT,
    "DateUpdated" TIMESTAMP,
    "CustomerName" TEXT,
    "LeadSource" TEXT,
    "ReferralSourceName" TEXT,
    "HospitalOfReferral" TEXT,
    "DepartmentOfReferral" TEXT,
    "SocialMedia" TEXT,
    "EventDate" TIMESTAMP,
    "QualificationStatus" TEXT,
    "EventName" TEXT,
    "LeadId" TEXT,
    "Status" TEXT,
    "Score" TEXT,
    "IsActive" BOOLEAN,
    "Comments" TEXT,
    "LeadType" TEXT,
    "ContactName" TEXT,
    "Salutation" TEXT,
    "ContactMobileNo" TEXT,
    "LandLineNo" TEXT,
    "Email" TEXT,
    "Fax" TEXT,
    "DoorNo" TEXT,
    "Street" TEXT,
    "Landmark" TEXT,
    "Website" TEXT,
    "Territory" TEXT,
    "AreaId" INT,
    "CityId" INT,
    "PincodeId" INT,
    "CityOfReferralId" INT,
    "DistrictId" INT,
    "StateId" INT,
    "CityName" TEXT,
    "AreaName" TEXT,
    "Pincode" TEXT,
    "StateName" TEXT,
    "DistrictName" TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        sl.id AS "Id",
        sl.user_created AS "UserCreated",
        sl.date_created AS "DateCreated",
        sl.user_updated AS "UserUpdated", 
        sl.date_updated AS "DateUpdated",
        sl.customer_name AS "CustomerName",
        sl.lead_source AS "LeadSource",
        sl.referral_source_name AS "ReferralSourceName",
        sl.hospital_of_referral AS "HospitalOfReferral",
        sl.department_of_referral AS "DepartmentOfReferral",
        sl.social_media AS "SocialMedia",
        sl.event_date AS "EventDate",
        sl.qualification_status AS "QualificationStatus",
        sl.event_name AS "EventName",
        sl.lead_id AS "LeadId",
        sl.status AS "Status",
        sl.score AS "Score",
        sl.isactive AS "IsActive",
        sl.comments AS "Comments",
        sl.lead_type AS "LeadType",
        sl.contact_name AS "ContactName",
        sl.salutation AS "Salutation",
        sl.contact_mobile_no AS "ContactMobileNo",
        sl.land_line_no AS "LandLineNo",
        sl.email AS "Email",
        sl.fax AS "Fax",
        sl.door_no AS "DoorNo",
        sl.street AS "Street",
        sl.landmark AS "Landmark",
        sl.website AS "Website", 
        sl.territory AS "Territory",
        sl.area_id AS "AreaId",
        sl.city_id AS "CityId",
        sl.pincode_id AS "PincodeId",
        sl.city_of_referral_id AS "CityOfReferralId",
        sl.district_id AS "DistrictId",
        sl.state_id AS "StateId",
        c.name AS "CityName",
        a.name AS "AreaName", 
        p.name AS "Pincode",
        s.name AS "StateName",
        d.name AS "DistrictName"
    FROM sales_lead sl
    LEFT JOIN sales_cities c ON sl.city_id = c.id
    LEFT JOIN sales_areas a ON sl.area_id = a.id  
    LEFT JOIN pincodes p ON sl.pincode_id = p.id
    LEFT JOIN sales_states s ON sl.state_id = s.id
    LEFT JOIN sales_districts d ON sl.district_id = d.id
    WHERE sl.id = p_id;
END;
$$;

-- Function to create a sales lead
CREATE OR REPLACE FUNCTION fn_create_sales_lead(
    p_user_created INT,
    p_customer_name TEXT,
    p_lead_source TEXT DEFAULT NULL,
    p_referral_source_name TEXT DEFAULT NULL,
    p_hospital_of_referral TEXT DEFAULT NULL,
    p_department_of_referral TEXT DEFAULT NULL,
    p_social_media TEXT DEFAULT NULL,
    p_event_date TIMESTAMP DEFAULT NULL,
    p_qualification_status TEXT DEFAULT NULL,
    p_event_name TEXT DEFAULT NULL,
    p_lead_id TEXT DEFAULT NULL,
    p_status TEXT DEFAULT NULL,
    p_score TEXT DEFAULT NULL,
    p_is_active BOOLEAN DEFAULT TRUE,
    p_comments TEXT DEFAULT NULL,
    p_lead_type TEXT DEFAULT NULL,
    p_contact_name TEXT DEFAULT NULL,
    p_salutation TEXT DEFAULT NULL,
    p_contact_mobile_no TEXT DEFAULT NULL,
    p_land_line_no TEXT DEFAULT NULL,
    p_email TEXT DEFAULT NULL,
    p_fax TEXT DEFAULT NULL,
    p_door_no TEXT DEFAULT NULL,
    p_street TEXT DEFAULT NULL,
    p_landmark TEXT DEFAULT NULL,
    p_website TEXT DEFAULT NULL,
    p_territory TEXT DEFAULT NULL,
    p_area_id INT DEFAULT NULL,
    p_city_id INT DEFAULT NULL,
    p_pincode_id INT DEFAULT NULL,
    p_city_of_referral_id INT DEFAULT NULL,
    p_district_id INT DEFAULT NULL,
    p_state_id INT DEFAULT NULL
)
RETURNS INT
LANGUAGE plpgsql
AS $$
DECLARE
    v_id INT;
BEGIN
    INSERT INTO sales_lead (
        user_created, date_created, customer_name, lead_source, referral_source_name,
        hospital_of_referral, department_of_referral, social_media, event_date,
        qualification_status, event_name, lead_id, status, score, isactive,
        comments, lead_type, contact_name, salutation, contact_mobile_no,
        land_line_no, email, fax, door_no, street, landmark, website,
        territory, area_id, city_id, pincode_id, city_of_referral_id,
        district_id, state_id
    ) VALUES (
        p_user_created, CURRENT_TIMESTAMP, p_customer_name, p_lead_source, p_referral_source_name,
        p_hospital_of_referral, p_department_of_referral, p_social_media, p_event_date,
        p_qualification_status, p_event_name, p_lead_id, p_status, p_score, p_is_active,
        p_comments, p_lead_type, p_contact_name, p_salutation, p_contact_mobile_no,
        p_land_line_no, p_email, p_fax, p_door_no, p_street, p_landmark, p_website,
        p_territory, p_area_id, p_city_id, p_pincode_id, p_city_of_referral_id,
        p_district_id, p_state_id
    )
    RETURNING id INTO v_id;

    RETURN v_id;
END;
$$;

-- Function to update a sales lead
CREATE OR REPLACE FUNCTION fn_update_sales_lead(
    p_id INT,
    p_user_updated INT,
    p_customer_name TEXT,
    p_lead_source TEXT DEFAULT NULL,
    p_referral_source_name TEXT DEFAULT NULL,
    p_hospital_of_referral TEXT DEFAULT NULL,
    p_department_of_referral TEXT DEFAULT NULL,
    p_social_media TEXT DEFAULT NULL,
    p_event_date TIMESTAMP DEFAULT NULL,
    p_qualification_status TEXT DEFAULT NULL,
    p_event_name TEXT DEFAULT NULL,
    p_lead_id TEXT DEFAULT NULL,
    p_status TEXT DEFAULT NULL,
    p_score TEXT DEFAULT NULL,
    p_is_active BOOLEAN DEFAULT TRUE,
    p_comments TEXT DEFAULT NULL,
    p_lead_type TEXT DEFAULT NULL,
    p_contact_name TEXT DEFAULT NULL,
    p_salutation TEXT DEFAULT NULL,
    p_contact_mobile_no TEXT DEFAULT NULL,
    p_land_line_no TEXT DEFAULT NULL,
    p_email TEXT DEFAULT NULL,
    p_fax TEXT DEFAULT NULL,
    p_door_no TEXT DEFAULT NULL,
    p_street TEXT DEFAULT NULL,
    p_landmark TEXT DEFAULT NULL,
    p_website TEXT DEFAULT NULL,
    p_territory TEXT DEFAULT NULL,
    p_door_no TEXT DEFAULT NULL,
    p_street TEXT DEFAULT NULL,
    p_landmark TEXT DEFAULT NULL,
    p_website TEXT DEFAULT NULL,
    p_territory TEXT DEFAULT NULL,
    p_area_id INT DEFAULT NULL,
    p_city_id INT DEFAULT NULL,
    p_pincode_id INT DEFAULT NULL,
    p_city_of_referral_id INT DEFAULT NULL,
    p_district_id INT DEFAULT NULL,
    p_state_id INT DEFAULT NULL
)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE sales_lead
    SET
        user_updated = p_user_updated,
        date_updated = CURRENT_TIMESTAMP,
        customer_name = p_customer_name,
        lead_source = p_lead_source,
        referral_source_name = p_referral_source_name,
        hospital_of_referral = p_hospital_of_referral,
        department_of_referral = p_department_of_referral,
        social_media = p_social_media,
        event_date = p_event_date,
        qualification_status = p_qualification_status,
        event_name = p_event_name,
        lead_id = p_lead_id,
        status = p_status,
        score = p_score,
        isactive = p_is_active,
        comments = p_comments,
        lead_type = p_lead_type,
        contact_name = p_contact_name,
        salutation = p_salutation,
        contact_mobile_no = p_contact_mobile_no,
        land_line_no = p_land_line_no,
        email = p_email,
        fax = p_fax,
        door_no = p_door_no,
        street = p_street,
        landmark = p_landmark,
        website = p_website,
        territory = p_territory,
        area_id = p_area_id,
        city_id = p_city_id,
        pincode_id = p_pincode_id,
        city_of_referral_id = p_city_of_referral_id,
        district_id = p_district_id,
        state_id = p_state_id
    WHERE id = p_id;
    
    RETURN FOUND;
END;
$$;

-- Function to delete a sales lead
CREATE OR REPLACE FUNCTION fn_delete_sales_lead(p_id INT)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM sales_leads WHERE id = p_id;
    RETURN FOUND;
END;
$$;