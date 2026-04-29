-- Drop existing functions if they exist
DROP FUNCTION IF EXISTS sales_lead_grid(TEXT, TEXT[], TEXT[], TEXT[], TEXT[], TEXT[], INT, INT, TEXT, TEXT) CASCADE;
DROP FUNCTION IF EXISTS sales_lead_grid(INT, TEXT, TEXT[], TEXT[], TEXT[], TEXT[], TEXT[], INT, INT, TEXT, TEXT) CASCADE;

-- Function 1: sales_lead_grid without user filter (10 parameters)
CREATE OR REPLACE FUNCTION sales_lead_grid(
    p_search_text TEXT DEFAULT NULL,
    p_customer_names TEXT[] DEFAULT NULL,
    p_statuses TEXT[] DEFAULT NULL,
    p_scores TEXT[] DEFAULT NULL,
    p_lead_types TEXT[] DEFAULT NULL,
    p_selected_lead_ids TEXT[] DEFAULT NULL,
    p_page_number INT DEFAULT 1,
    p_page_size INT DEFAULT 10,
    p_order_by TEXT DEFAULT 'id',
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
    "EventDate" DATE,
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
    "AreaId" INT,
    "PincodeId" INT,
    "Area" TEXT,
    "Territory" TEXT,
    "City" TEXT,
    "Pincode" TEXT,
    "District" TEXT,
    "State" TEXT,
    "Country" TEXT
)
LANGUAGE SQL
STABLE
AS $$
SELECT 
    COUNT(*) OVER() AS "TotalRecords",
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
    sl.event_name AS "EventName",
    sl.lead_id AS "LeadId",
    sl.status AS "Status",
    sl.score AS "Score",
    sl.isactive AS "IsActive",
    sl.comments AS "Comments",
    sl.lead_type AS "LeadType",
    sl.contact_name AS "ContactName",
    sl.salutation AS "Salutation",
    COALESCE(sl.contact_mobile_no::TEXT, '') AS "ContactMobileNo",
    sl.land_line_no AS "LandLineNo",
    sl.email AS "Email",
    sl.fax AS "Fax",
    sl.door_no AS "DoorNo",
    sl.street AS "Street",
    sl.landmark AS "Landmark",
    sl.website AS "Website",
    sl.area_id AS "AreaId",
    sl.pincode_id AS "PincodeId",
    sl.area AS "Area",
    sl.territory AS "Territory",
    sl.city AS "City",
    sl.pincode AS "Pincode",
    sl.district AS "District",
    sl.state AS "State",
    sl.country AS "Country"
FROM sales_lead sl
WHERE 1=1
    AND (p_search_text IS NULL OR p_search_text = '' OR (
        sl.customer_name ILIKE '%' || p_search_text || '%' OR
        sl.contact_name ILIKE '%' || p_search_text || '%' OR
        sl.lead_id ILIKE '%' || p_search_text || '%' OR
        sl.email ILIKE '%' || p_search_text || '%' OR
        COALESCE(sl.contact_mobile_no::TEXT, '') ILIKE '%' || p_search_text || '%'
    ))
    AND (p_customer_names IS NULL OR array_length(p_customer_names, 1) IS NULL OR sl.customer_name = ANY(p_customer_names))
    AND (p_statuses IS NULL OR array_length(p_statuses, 1) IS NULL OR sl.status = ANY(p_statuses))
    AND (p_scores IS NULL OR array_length(p_scores, 1) IS NULL OR sl.score = ANY(p_scores))
    AND (p_lead_types IS NULL OR array_length(p_lead_types, 1) IS NULL OR sl.lead_type = ANY(p_lead_types))
    AND (p_selected_lead_ids IS NULL OR array_length(p_selected_lead_ids, 1) IS NULL OR sl.id::TEXT = ANY(p_selected_lead_ids))
ORDER BY 
    sl.id DESC
LIMIT p_page_size OFFSET ((p_page_number - 1) * p_page_size);
$$;

-- Function 2: sales_lead_grid with user filter (11 parameters)
CREATE OR REPLACE FUNCTION sales_lead_grid(
    p_current_user_id INT,
    p_search_text TEXT DEFAULT NULL,
    p_customer_names TEXT[] DEFAULT NULL,
    p_statuses TEXT[] DEFAULT NULL,
    p_scores TEXT[] DEFAULT NULL,
    p_lead_types TEXT[] DEFAULT NULL,
    p_selected_lead_ids TEXT[] DEFAULT NULL,
    p_page_number INT DEFAULT 1,
    p_page_size INT DEFAULT 10,
    p_order_by TEXT DEFAULT 'id',
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
    "EventDate" DATE,
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
    "AreaId" INT,
    "PincodeId" INT,
    "Area" TEXT,
    "Territory" TEXT,
    "City" TEXT,
    "Pincode" TEXT,
    "District" TEXT,
    "State" TEXT,
    "Country" TEXT
)
LANGUAGE SQL
STABLE
AS $$
SELECT 
    COUNT(*) OVER() AS "TotalRecords",
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
    sl.event_name AS "EventName",
    sl.lead_id AS "LeadId",
    sl.status AS "Status",
    sl.score AS "Score",
    sl.isactive AS "IsActive",
    sl.comments AS "Comments",
    sl.lead_type AS "LeadType",
    sl.contact_name AS "ContactName",
    sl.salutation AS "Salutation",
    COALESCE(sl.contact_mobile_no::TEXT, '') AS "ContactMobileNo",
    sl.land_line_no AS "LandLineNo",
    sl.email AS "Email",
    sl.fax AS "Fax",
    sl.door_no AS "DoorNo",
    sl.street AS "Street",
    sl.landmark AS "Landmark",
    sl.website AS "Website",
    sl.area_id AS "AreaId",
    sl.pincode_id AS "PincodeId",
    sl.area AS "Area",
    sl.territory AS "Territory",
    sl.city AS "City",
    sl.pincode AS "Pincode",
    sl.district AS "District",
    sl.state AS "State",
    sl.country AS "Country"
FROM sales_lead sl
WHERE sl.user_created = p_current_user_id
    AND (p_search_text IS NULL OR p_search_text = '' OR (
        sl.customer_name ILIKE '%' || p_search_text || '%' OR
        sl.contact_name ILIKE '%' || p_search_text || '%' OR
        sl.lead_id ILIKE '%' || p_search_text || '%' OR
        sl.email ILIKE '%' || p_search_text || '%' OR
        COALESCE(sl.contact_mobile_no::TEXT, '') ILIKE '%' || p_search_text || '%'
    ))
    AND (p_customer_names IS NULL OR array_length(p_customer_names, 1) IS NULL OR sl.customer_name = ANY(p_customer_names))
    AND (p_statuses IS NULL OR array_length(p_statuses, 1) IS NULL OR sl.status = ANY(p_statuses))
    AND (p_scores IS NULL OR array_length(p_scores, 1) IS NULL OR sl.score = ANY(p_scores))
    AND (p_lead_types IS NULL OR array_length(p_lead_types, 1) IS NULL OR sl.lead_type = ANY(p_lead_types))
    AND (p_selected_lead_ids IS NULL OR array_length(p_selected_lead_ids, 1) IS NULL OR sl.id::TEXT = ANY(p_selected_lead_ids))
ORDER BY 
    sl.id DESC
LIMIT p_page_size OFFSET ((p_page_number - 1) * p_page_size);
$$;
