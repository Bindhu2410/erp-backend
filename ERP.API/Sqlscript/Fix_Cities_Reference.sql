-- Fix for "relation 'cities' does not exist" error in fn_get_leads_with_pagination function

-- Drop existing function to replace it
DROP FUNCTION IF EXISTS fn_get_leads_with_pagination;

-- Create function with corrected table references (sales_cities instead of cities,
-- sales_areas instead of areas, sales_states instead of states, and sales_districts instead of districts)
CREATE OR REPLACE FUNCTION fn_get_leads_with_pagination(
    p_search_text TEXT DEFAULT NULL,
    p_zones TEXT[] DEFAULT NULL, -- We keep this parameter for compatibility but won't use it
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
    
    -- Remove zone filtering since the zones table doesn't exist
    
    IF p_customer_names IS NOT NULL AND array_length(p_customer_names, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sl.customer_name = ANY($1)';
    END IF;
    
    IF p_territories IS NOT NULL AND array_length(p_territories, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND t.name = ANY($2)';
    END IF;
    
    IF p_statuses IS NOT NULL AND array_length(p_statuses, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sl.status = ANY($3)';
    END IF;
    
    IF p_scores IS NOT NULL AND array_length(p_scores, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sl.score = ANY($4)';
    END IF;
    
    IF p_lead_types IS NOT NULL AND array_length(p_lead_types, 1) > 0 THEN
        v_where_clause := v_where_clause || ' AND sl.lead_type = ANY($5)';
    END IF;
    
    -- Build ORDER BY clause
    v_order_clause := ' ORDER BY ';
    
    CASE p_order_by
        WHEN 'created_date' THEN v_order_clause := v_order_clause || 'sl.created_date ';
        WHEN 'customer_name' THEN v_order_clause := v_order_clause || 'sl.customer_name ';
        WHEN 'contact_name' THEN v_order_clause := v_order_clause || 'sl.contact_name ';
        WHEN 'lead_id' THEN v_order_clause := v_order_clause || 'sl.lead_id ';
        WHEN 'status' THEN v_order_clause := v_order_clause || 'sl.status ';
        WHEN 'score' THEN v_order_clause := v_order_clause || 'sl.score ';
        WHEN 'lead_type' THEN v_order_clause := v_order_clause || 'sl.lead_type ';
        ELSE v_order_clause := v_order_clause || 'sl.created_date ';
    END CASE;
    
    IF p_order_direction = 'ASC' THEN
        v_order_clause := v_order_clause || 'ASC';
    ELSE
        v_order_clause := v_order_clause || 'DESC';
    END IF;
    
    -- Count query to get total records - Fix table references
    v_count_query := '
        SELECT COUNT(*) 
        FROM sales_leads sl
        LEFT JOIN sales_territories t ON sl.territory = t.name
        ' || v_where_clause;
        
    -- Execute count query with adjusted parameter positions
    EXECUTE v_count_query 
    INTO v_total_records
    USING 
        p_customer_names,
        p_territories,
        p_statuses,
        p_scores,
        p_lead_types;
    
    -- Main query to get paginated results - Fix table references
    v_main_query := '
        SELECT 
            ' || v_total_records || ' AS "TotalRecords",
            sl.id AS "Id",
            sl.created_by AS "UserCreated",
            sl.created_date AS "DateCreated",
            sl.updated_by AS "UserUpdated",
            sl.updated_date AS "DateUpdated",
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
            sl.territory AS "Territory",
            sl.area_id AS "AreaId",
            sl.city_id AS "CityId",
            sl.pincode_id AS "PincodeId",
            sl.city_of_referral_id AS "CityOfReferralId",
            sl.district_id AS "DistrictId",
            sl.state_id AS "StateId",
            c.name AS "CityName",
            a.name AS "AreaName",
            p.pincode AS "Pincode",
            s.name AS "StateName",
            d.name AS "DistrictName",
            t.name AS "TerritoryName"
        FROM sales_leads sl
        LEFT JOIN sales_territories t ON sl.territory = t.name
        LEFT JOIN sales_cities c ON sl.city_id = c.id
        LEFT JOIN sales_areas a ON sl.area_id = a.id
        LEFT JOIN pincodes p ON sl.pincode_id = p.id
        LEFT JOIN sales_states s ON sl.state_id = s.id
        LEFT JOIN sales_districts d ON sl.district_id = d.id
        ' || v_where_clause || v_order_clause || '
        LIMIT ' || p_page_size || ' OFFSET ' || v_offset;
    
    -- Execute main query and return results with adjusted parameter positions
    RETURN QUERY EXECUTE v_main_query
    USING 
        p_customer_names,
        p_territories,
        p_statuses,
        p_scores,
        p_lead_types;
END;
$$;
