-- Fix for "structure of query does not match function result type" error 
-- in fn_get_leads_with_pagination function

-- Drop existing function to replace it
DROP FUNCTION IF EXISTS fn_get_leads_with_pagination;

-- Recreate the function with the correct structure that matches the C# SalesLeadGridResult class
CREATE OR REPLACE FUNCTION fn_get_leads_with_pagination(
    p_search_text TEXT DEFAULT NULL,
    p_zones TEXT[] DEFAULT NULL,
    p_customer_names TEXT[] DEFAULT NULL,
    p_territories TEXT[] DEFAULT NULL,
    p_statuses TEXT[] DEFAULT NULL,
    p_scores TEXT[] DEFAULT NULL,
    p_lead_types TEXT[] DEFAULT NULL,
    p_page_number INT DEFAULT 1,    p_page_size INT DEFAULT 10,
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
    
    -- Get total count first
    EXECUTE '
        SELECT COUNT(*) 
        FROM sales_leads sl
        LEFT JOIN sales_territories t ON sl.territory = t.name'
        || v_where_clause
    INTO v_total_records
    USING 
        p_customer_names,
        p_territories,
        p_statuses,
        p_scores,
        p_lead_types;
        
    -- Use RETURN QUERY EXECUTE with proper casting to match the function's return type
    RETURN QUERY EXECUTE '
        WITH results AS (
            SELECT 
                sl.id,
                sl.user_created,
                sl.date_created,
                sl.user_updated,
                sl.date_updated,
                sl.customer_name,
                sl.lead_source,
                sl.referral_source_name,
                sl.hospital_of_referral,
                sl.department_of_referral,
                sl.social_media,
                sl.event_date,
                sl.qualification_status,
                sl.event_name,
                sl.lead_id,
                sl.status,
                sl.score,
                sl.isactive,
                sl.comments,
                sl.lead_type,
                sl.contact_name,
                sl.salutation,
                sl.contact_mobile_no,
                sl.land_line_no,
                sl.email,
                sl.fax,
                sl.door_no,
                sl.street,
                sl.landmark,
                sl.website,
                sl.territory,
                sl.area_id,
                sl.city_id,
                sl.pincode_id,
                sl.city_of_referral_id,
                sl.district_id,
                sl.state_id,
                c.name AS city_name,
                a.name AS area_name,
                p.pincode,
                s.name AS state_name,
                d.name AS district_name,
                t.name AS territory_name
            FROM sales_leads sl
            LEFT JOIN sales_territories t ON sl.territory = t.name
            LEFT JOIN sales_cities c ON sl.city_id = c.id
            LEFT JOIN sales_areas a ON sl.area_id = a.id
            LEFT JOIN pincodes p ON sl.pincode_id = p.id
            LEFT JOIN sales_states s ON sl.state_id = s.id
            LEFT JOIN sales_districts d ON sl.district_id = d.id
            ' || v_where_clause || '
            ORDER BY sl.' || quote_ident(p_order_by) || ' ' || p_order_direction || '
            LIMIT ' || p_page_size || ' OFFSET ' || v_offset || '
        )
        SELECT 
            ' || v_total_records || '::INT AS "TotalRecords",
            id::INT AS "Id",
            user_created::INT AS "UserCreated",
            date_created::TIMESTAMP AS "DateCreated",
            user_updated::INT AS "UserUpdated",
            date_updated::TIMESTAMP AS "DateUpdated",
            customer_name::TEXT AS "CustomerName",
            lead_source::TEXT AS "LeadSource",
            referral_source_name::TEXT AS "ReferralSourceName",
            hospital_of_referral::TEXT AS "HospitalOfReferral",
            department_of_referral::TEXT AS "DepartmentOfReferral",
            social_media::TEXT AS "SocialMedia",
            event_date::TIMESTAMP AS "EventDate",
            qualification_status::TEXT AS "QualificationStatus",
            event_name::TEXT AS "EventName",
            lead_id::TEXT AS "LeadId",
            status::TEXT AS "Status",
            score::TEXT AS "Score",
            isactive::BOOLEAN AS "IsActive",
            comments::TEXT AS "Comments",
            lead_type::TEXT AS "LeadType",
            contact_name::TEXT AS "ContactName",
            salutation::TEXT AS "Salutation",
            contact_mobile_no::TEXT AS "ContactMobileNo",
            land_line_no::TEXT AS "LandLineNo",
            email::TEXT AS "Email",
            fax::TEXT AS "Fax",
            door_no::TEXT AS "DoorNo",
            street::TEXT AS "Street",
            landmark::TEXT AS "Landmark",
            website::TEXT AS "Website",
            territory AS "Territory",
            area_id::INT AS "AreaId",
            city_id::INT AS "CityId",
            pincode_id::INT AS "PincodeId",
            city_of_referral_id::INT AS "CityOfReferralId",
            district_id::INT AS "DistrictId",
            state_id::INT AS "StateId",
            city_name::TEXT AS "CityName",
            area_name::TEXT AS "AreaName",
            pincode::TEXT AS "Pincode",
            state_name::TEXT AS "StateName",
            district_name::TEXT AS "DistrictName",
            territory_name::TEXT AS "TerritoryName"
        FROM results
    '
    USING 
        p_customer_names,
        p_territories,
        p_statuses,
        p_scores,
        p_lead_types;
END;
$$;
