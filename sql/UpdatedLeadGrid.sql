DROP FUNCTION IF EXISTS fn_get_sales_leads_grid;
 
CREATE OR REPLACE FUNCTION fn_get_sales_leads_grid(
    p_search_text TEXT DEFAULT NULL,
    p_zones TEXT[] DEFAULT NULL,
    p_customer_names TEXT[] DEFAULT NULL,
    p_territories TEXT[] DEFAULT NULL,
    p_statuses TEXT[] DEFAULT NULL,
    p_scores TEXT[] DEFAULT NULL,
    p_lead_types TEXT[] DEFAULT NULL,
    p_selected_lead_ids TEXT[] DEFAULT NULL,
    p_page_number INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 10,
    p_order_by TEXT DEFAULT 'date_created',
    p_order_direction TEXT DEFAULT 'DESC'
)
RETURNS TABLE (
    "TotalRecords" INTEGER,
    "Id" INTEGER,
    "LeadId" VARCHAR,
    "CustomerName" VARCHAR,
    "LeadSource" VARCHAR,
    "ReferralSourceName" VARCHAR,
    "HospitalOfReferral" VARCHAR,
    "DepartmentOfReferral" VARCHAR,
    "CityOfReferral" VARCHAR,
    "SocialMedia" VARCHAR,
    "EventDate" TIMESTAMP,
    "QualificationStatus" VARCHAR,
    "EventName" VARCHAR,
    "Status" VARCHAR,
    "Score" VARCHAR,
    "LeadType" VARCHAR,
    "ContactName" VARCHAR,
    "Salutation" VARCHAR,
    "ContactMobileNo" VARCHAR,
    "LandLineNo" VARCHAR,
    "Email" VARCHAR,
    "Website" VARCHAR,
    "TerritoryId" INTEGER,
    "TerritoryName" VARCHAR,
    "AreaId" INTEGER,
    "AreaName" VARCHAR,
    "CityId" INTEGER,
    "CityName" VARCHAR,
    "PincodeId" INTEGER,
    "Pincode" VARCHAR,
    "StateId" INTEGER,
    "StateName" VARCHAR,
    "DistrictId" INTEGER,
    "DistrictName" VARCHAR,
    "DateCreated" TIMESTAMP,
    "DateUpdated" TIMESTAMP,
    "UserCreated" INTEGER,
    "UserUpdated" INTEGER,
    "IsActive" BOOLEAN
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INTEGER;
    v_where_clause TEXT;
    v_total_records INTEGER;
    v_valid_page_size INTEGER;
    v_valid_page_number INTEGER;
BEGIN
    -- Validate and set default values for pagination
    v_valid_page_size := LEAST(COALESCE(NULLIF(p_page_size, 0), 10), 1000);  -- Default 10, max 1000
    v_valid_page_number := COALESCE(NULLIF(p_page_number, 0), 1);  -- Default to 1 if 0 or null
    
    -- Calculate offset for pagination
    v_offset := (v_valid_page_number - 1) * v_valid_page_size;

    -- Initialize WHERE clause with isactive condition
    v_where_clause := 'WHERE sl.isactive = true';

    -- Add filter for selected lead IDs, ignore if it only contains 'string'
    IF p_selected_lead_ids IS NOT NULL AND array_length(p_selected_lead_ids, 1) > 0 
       AND NOT (array_length(p_selected_lead_ids, 1) = 1 AND p_selected_lead_ids[1] = 'string') THEN
        v_where_clause := v_where_clause || ' AND sl.lead_id = ANY($8::text[])';
    END IF;

    -- Add enhanced search filter with more searchable fields, ignore if it's 'string'
    IF p_search_text IS NOT NULL AND p_search_text != '' AND p_search_text != 'string' THEN
        v_where_clause := v_where_clause || ' AND (
            LOWER(sl.customer_name) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sl.lead_source) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sl.lead_id) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sl.contact_name) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sl.email) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sl.contact_mobile_no) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sl.land_line_no) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sl.status) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sl.lead_type) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sl.qualification_status) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sl.website) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sl.lead_source) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sc.name) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(ss.name) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sd.name) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(st.name) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sct.name) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sa.name) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(p.pincode) LIKE ''%'' || LOWER($1) || ''%''
        )';
    END IF;

    -- Add array filters with partial matching support, ignore if array only contains 'string'
    IF p_zones IS NOT NULL AND array_length(p_zones, 1) > 0 
       AND NOT (array_length(p_zones, 1) = 1 AND p_zones[1] = 'string') THEN
        v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($2::varchar[]) AS z WHERE LOWER(sa.name) LIKE ''%'' || LOWER(z) || ''%'')';
    END IF;
 
    IF p_customer_names IS NOT NULL AND array_length(p_customer_names, 1) > 0 
       AND NOT (array_length(p_customer_names, 1) = 1 AND p_customer_names[1] = 'string') THEN
        v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($3::varchar[]) AS cn WHERE LOWER(sl.customer_name) LIKE ''%'' || LOWER(cn) || ''%'')';
    END IF;
 
    IF p_territories IS NOT NULL AND array_length(p_territories, 1) > 0 
       AND NOT (array_length(p_territories, 1) = 1 AND p_territories[1] = 'string') THEN
        v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($4::varchar[]) AS t WHERE LOWER(st.name) LIKE ''%'' || LOWER(t) || ''%'')';
    END IF;
 
    IF p_statuses IS NOT NULL AND array_length(p_statuses, 1) > 0 
       AND NOT (array_length(p_statuses, 1) = 1 AND p_statuses[1] = 'string') THEN
        v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($5::varchar[]) AS s WHERE LOWER(sl.status) LIKE ''%'' || LOWER(s) || ''%'')';
    END IF;
 
    IF p_scores IS NOT NULL AND array_length(p_scores, 1) > 0 
       AND NOT (array_length(p_scores, 1) = 1 AND p_scores[1] = 'string') THEN
        v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($6::varchar[]) AS sc WHERE LOWER(sl.score) LIKE ''%'' || LOWER(sc) || ''%'')';
    END IF;
 
    IF p_lead_types IS NOT NULL AND array_length(p_lead_types, 1) > 0 
       AND NOT (array_length(p_lead_types, 1) = 1 AND p_lead_types[1] = 'string') THEN
        v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($7::varchar[]) AS lt WHERE LOWER(sl.lead_type) LIKE ''%'' || LOWER(lt) || ''%'')';
    END IF;
 
    -- Get total record count    
    EXECUTE 'SELECT COUNT(*) FROM sales_lead sl
        LEFT JOIN sales_states ss ON sl.state = ss.name
        LEFT JOIN sales_countries sc ON ss.sales_countries_id = sc.id
        LEFT JOIN sales_districts sd ON sl.district = sd.name
        LEFT JOIN sales_territories st ON sl.territory = st.name
        LEFT JOIN sales_cities sct ON sl.city = sct.name
        LEFT JOIN sales_areas sa ON sl.area = sa.name
        LEFT JOIN pincodes p ON sl.pincode = p.pincode ' || v_where_clause
    INTO v_total_records
    USING p_search_text, p_zones, p_customer_names, p_territories, p_statuses, p_scores, p_lead_types, p_selected_lead_ids;    
 
    -- Return main query results    
    RETURN QUERY EXECUTE 'WITH base_query AS (
        SELECT
            sl.*,
            sc.name AS country_name,
            ss.name AS state_name,
            sd.name AS district_name,
            st.name AS territory_name,
            sct.name AS city_name,
            sa.name AS area_name,
            p.pincode AS pincode_value
        FROM sales_lead sl
        LEFT JOIN sales_states ss ON sl.state = ss.name
        LEFT JOIN sales_countries sc ON ss.sales_countries_id = sc.id
        LEFT JOIN sales_districts sd ON sl.district = sd.name
        LEFT JOIN sales_territories st ON sl.territory = st.name
        LEFT JOIN sales_cities sct ON sl.city = sct.name
        LEFT JOIN sales_areas sa ON sl.area = sa.name
        LEFT JOIN pincodes p ON sl.pincode = p.pincode        
        ' || v_where_clause || '        
        ORDER BY ' ||
            CASE
                WHEN p_order_by LIKE 'sl.%' THEN
                    substring(p_order_by from 4)
                WHEN p_order_by LIKE 'sales_leads.%' THEN
                    substring(p_order_by from 13)
                ELSE
                    p_order_by
            END
        || ' ' || p_order_direction || '
        LIMIT ' || v_valid_page_size || ' OFFSET ' || v_offset || '
    )    SELECT
        ' || v_total_records || '::INTEGER AS "TotalRecords",
        bq.id::INTEGER AS "Id",
        CAST(bq.lead_id AS VARCHAR) AS "LeadId",
        CAST(bq.customer_name AS VARCHAR) AS "CustomerName",
        CAST(bq.lead_source AS VARCHAR) AS "LeadSource",
        CAST(bq.lead_source AS VARCHAR) AS "ReferralSourceName",
        CAST(NULL AS VARCHAR) AS "HospitalOfReferral",
        CAST(NULL AS VARCHAR) AS "DepartmentOfReferral",
        CAST(bq.city AS VARCHAR) AS "CityOfReferral",
        CAST(bq.social_media AS VARCHAR) AS "SocialMedia",
        CAST(bq.event_date AS TIMESTAMP) AS "EventDate",
        CAST(bq.qualification_status AS VARCHAR) AS "QualificationStatus",
        CAST(bq.event_name AS VARCHAR) AS "EventName",
        CAST(bq.status AS VARCHAR) AS "Status",
        CAST(bq.score AS VARCHAR) AS "Score",
        CAST(bq.lead_type AS VARCHAR) AS "LeadType",
        CAST(bq.contact_name AS VARCHAR) AS "ContactName",
        CAST(bq.salutation AS VARCHAR) AS "Salutation",
        CAST(bq.contact_mobile_no AS VARCHAR) AS "ContactMobileNo",
        CAST(bq.land_line_no AS VARCHAR) AS "LandLineNo",
        CAST(bq.email AS VARCHAR) AS "Email",
        CAST(bq.website AS VARCHAR) AS "Website",
        COALESCE(st.id, 0)::INTEGER AS "TerritoryId",
        CAST(st.name AS VARCHAR) AS "TerritoryName",
        COALESCE(sa.id, 0)::INTEGER AS "AreaId",
        CAST(sa.name AS VARCHAR) AS "AreaName",
        COALESCE(sct.id, 0)::INTEGER AS "CityId",
        CAST(sct.name AS VARCHAR) AS "CityName",
        COALESCE(p.id, 0)::INTEGER AS "PincodeId",
        CAST(p.pincode AS VARCHAR) AS "Pincode",
        COALESCE(ss.id, 0)::INTEGER AS "StateId",
        CAST(ss.name AS VARCHAR) AS "StateName",
        COALESCE(sd.id, 0)::INTEGER AS "DistrictId",
        CAST(sd.name AS VARCHAR) AS "DistrictName",
        CAST(bq.date_created AS TIMESTAMP) AS "DateCreated",
        CAST(bq.date_updated AS TIMESTAMP) AS "DateUpdated",
        COALESCE(bq.user_created, 0)::INTEGER AS "UserCreated",
        COALESCE(bq.user_updated, 0)::INTEGER AS "UserUpdated",
        COALESCE(bq.isactive, false)::BOOLEAN AS "IsActive"FROM base_query bq
        LEFT JOIN sales_states ss ON bq.state = ss.name
        LEFT JOIN sales_countries sc ON ss.sales_countries_id = sc.id
        LEFT JOIN sales_districts sd ON bq.district = sd.name
        LEFT JOIN sales_territories st ON bq.territory = st.name
        LEFT JOIN sales_cities sct ON bq.city = sct.name
        LEFT JOIN sales_areas sa ON bq.area = sa.name
        LEFT JOIN pincodes p ON bq.pincode = p.pincode'
    USING p_search_text, p_zones, p_customer_names, p_territories, p_statuses, p_scores, p_lead_types, p_selected_lead_ids;
END;
$$;