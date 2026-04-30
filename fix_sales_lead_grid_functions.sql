-- Fix sales_lead_grid functions to ensure both versions exist
-- This script creates both the user-based and non-user-based versions

-- Drop existing functions to avoid conflicts
DROP FUNCTION IF EXISTS sales_lead_grid(text,text[],text[],text[],text[],text[],integer,integer,text,text);
DROP FUNCTION IF EXISTS sales_lead_grid(integer,text,text[],text[],text[],text[],text[],integer,integer,text,text);

-- 1. Create the non-user-based version (for development mode when currentUserId == 0)
CREATE OR REPLACE FUNCTION public.sales_lead_grid(
  p_search_text text DEFAULT NULL::text,
  p_customer_names text[] DEFAULT NULL::text[],
  p_statuses text[] DEFAULT NULL::text[],
  p_scores text[] DEFAULT NULL::text[],
  p_lead_types text[] DEFAULT NULL::text[],
  p_selected_lead_ids text[] DEFAULT NULL::text[],
  p_page_number integer DEFAULT 1,
  p_page_size integer DEFAULT 10,
  p_order_by text DEFAULT 'id'::text,
  p_order_direction text DEFAULT 'DESC'::text
)
RETURNS TABLE (
  "TotalRecords" integer,
  "Id" integer,
  "LeadId" character varying,
  "CustomerName" character varying,
  "LeadSource" character varying,
  "ReferralSourceName" character varying,
  "HospitalOfReferral" character varying,
  "DepartmentOfReferral" character varying,
  "CityOfReferral" character varying,
  "SocialMedia" character varying,
  "EventDate" timestamp without time zone,
  "EventName" character varying,
  "Status" character varying,
  "Score" character varying,
  "LeadType" character varying,
  "ContactName" character varying,
  "Salutation" character varying,
  "ContactMobileNo" character varying,
  "LandLineNo" character varying,
  "Email" character varying,
  "Website" character varying,
  "AreaId" integer,
  "AreaName" character varying,
  "CityId" integer,
  "CityName" character varying,
  "PincodeId" integer,
  "Pincode" character varying,
  "StateId" integer,
  "StateName" character varying,
  "DistrictId" integer,
  "DistrictName" character varying,
  "DateCreated" timestamp without time zone,
  "DateUpdated" timestamp without time zone,
  "UserCreated" integer,
  "UserUpdated" integer,
  "IsActive" boolean
)
LANGUAGE plpgsql
AS $function$
DECLARE
  v_offset INTEGER;
  v_where_clause TEXT;
  v_total_records INTEGER;
  v_valid_page_size INTEGER;
  v_valid_page_number INTEGER;
  v_order_by TEXT;
  v_order_direction TEXT;
BEGIN
  -- ORDER BY clause
  IF lower(p_order_by) = 'id' THEN
    v_order_by := 'sl.id';
  ELSIF lower(p_order_by) = 'date_created' THEN
    v_order_by := 'sl.date_created';
  ELSIF lower(p_order_by) = 'date_updated' THEN
    v_order_by := 'sl.date_updated';
  ELSE
    v_order_by := 'sl.id';
  END IF;

  IF upper(p_order_direction) = 'ASC' THEN
    v_order_direction := 'ASC';
  ELSE
    v_order_direction := 'DESC';
  END IF;

  -- Pagination
  v_valid_page_size := LEAST(COALESCE(NULLIF(p_page_size, 0), 10), 1000);
  v_valid_page_number := COALESCE(NULLIF(p_page_number, 0), 1);
  v_offset := (v_valid_page_number - 1) * v_valid_page_size;

  -- WHERE clause (without user filtering)
  v_where_clause := 'WHERE sl.isactive = true';

  IF p_selected_lead_ids IS NOT NULL AND array_length(p_selected_lead_ids, 1) > 0
     AND NOT (array_length(p_selected_lead_ids, 1) = 1 AND p_selected_lead_ids[1] = 'string') THEN
    v_where_clause := v_where_clause || ' AND sl.lead_id = ANY($6::text[])';
  END IF;

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
      LOWER(sl.website) LIKE ''%'' || LOWER($1) || ''%''
    )';
  END IF;

  IF p_customer_names IS NOT NULL AND array_length(p_customer_names, 1) > 0
     AND NOT (array_length(p_customer_names, 1) = 1 AND p_customer_names[1] = 'string') THEN
    v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($2::varchar[]) AS cn WHERE LOWER(sl.customer_name) LIKE ''%'' || LOWER(cn) || ''%'')';
  END IF;

  IF p_statuses IS NOT NULL AND array_length(p_statuses, 1) > 0
     AND NOT (array_length(p_statuses, 1) = 1 AND p_statuses[1] = 'string') THEN
    v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($3::varchar[]) AS s WHERE LOWER(sl.status) LIKE ''%'' || LOWER(s) || ''%'')';
  END IF;

  IF p_scores IS NOT NULL AND array_length(p_scores, 1) > 0
     AND NOT (array_length(p_scores, 1) = 1 AND p_scores[1] = 'string') THEN
    v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($4::varchar[]) AS sc WHERE LOWER(sl.score) LIKE ''%'' || LOWER(sc) || ''%'')';
  END IF;

  IF p_lead_types IS NOT NULL AND array_length(p_lead_types, 1) > 0
     AND NOT (array_length(p_lead_types, 1) = 1 AND p_lead_types[1] = 'string') THEN
    v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($5::varchar[]) AS lt WHERE LOWER(sl.lead_type) LIKE ''%'' || LOWER(lt) || ''%'')';
  END IF;

  -- Total count
  EXECUTE 'SELECT COUNT(*) FROM sales_lead sl ' || v_where_clause
  INTO v_total_records
  USING p_search_text, p_customer_names, p_statuses, p_scores, p_lead_types, p_selected_lead_ids;

  -- Main query
  RETURN QUERY EXECUTE 'WITH base_query AS (
    SELECT sl.*
    FROM sales_lead sl
    ' || v_where_clause || '
    ORDER BY ' || v_order_by || ' ' || v_order_direction || ', sl.id DESC
    LIMIT ' || v_valid_page_size || ' OFFSET ' || v_offset || '
  )
  SELECT
    ' || v_total_records || '::INTEGER AS "TotalRecords",
    bq.id::INTEGER AS "Id",
    CAST(bq.lead_id AS VARCHAR) AS "LeadId",
    CAST(bq.customer_name AS VARCHAR) AS "CustomerName",
    CAST(bq.lead_source AS VARCHAR) AS "LeadSource",
    CAST(bq.lead_source AS VARCHAR) AS "ReferralSourceName",
    CAST(bq.hospital_of_referral AS VARCHAR) AS "HospitalOfReferral",
    CAST(bq.department_of_referral AS VARCHAR) AS "DepartmentOfReferral",
    CAST(bq.city AS VARCHAR) AS "CityOfReferral",
    CAST(bq.social_media AS VARCHAR) AS "SocialMedia",
    CAST(bq.event_date AS TIMESTAMP) AS "EventDate",
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
    0::INTEGER AS "AreaId",
    CAST(bq.area AS VARCHAR) AS "AreaName",
    0::INTEGER AS "CityId",
    CAST(bq.city AS VARCHAR) AS "CityName",
    0::INTEGER AS "PincodeId",
    CAST(bq.pincode AS VARCHAR) AS "Pincode",
    0::INTEGER AS "StateId",
    CAST(bq.state AS VARCHAR) AS "StateName",
    0::INTEGER AS "DistrictId",
    CAST(bq.district AS VARCHAR) AS "DistrictName",
    CAST(bq.date_created AS TIMESTAMP) AS "DateCreated",
    CAST(bq.date_updated AS TIMESTAMP) AS "DateUpdated",
    COALESCE(bq.user_created, 0)::INTEGER AS "UserCreated",
    COALESCE(bq.user_updated, 0)::INTEGER AS "UserUpdated",
    COALESCE(bq.isactive, false)::BOOLEAN AS "IsActive"
  FROM base_query bq'
  USING p_search_text, p_customer_names, p_statuses, p_scores, p_lead_types, p_selected_lead_ids;

END;
$function$;

-- 2. Create the user-based version (for authenticated users)
CREATE OR REPLACE FUNCTION public.sales_lead_grid(
  p_current_user_id integer,
  p_search_text text DEFAULT NULL::text,
  p_customer_names text[] DEFAULT NULL::text[],
  p_statuses text[] DEFAULT NULL::text[],
  p_scores text[] DEFAULT NULL::text[],
  p_lead_types text[] DEFAULT NULL::text[],
  p_selected_lead_ids text[] DEFAULT NULL::text[],
  p_page_number integer DEFAULT 1,
  p_page_size integer DEFAULT 10,
  p_order_by text DEFAULT 'id'::text,
  p_order_direction text DEFAULT 'DESC'::text
)
RETURNS TABLE (
  "TotalRecords" integer,
  "Id" integer,
  "LeadId" character varying,
  "CustomerName" character varying,
  "LeadSource" character varying,
  "ReferralSourceName" character varying,
  "HospitalOfReferral" character varying,
  "DepartmentOfReferral" character varying,
  "CityOfReferral" character varying,
  "SocialMedia" character varying,
  "EventDate" timestamp without time zone,
  "EventName" character varying,
  "Status" character varying,
  "Score" character varying,
  "LeadType" character varying,
  "ContactName" character varying,
  "Salutation" character varying,
  "ContactMobileNo" character varying,
  "LandLineNo" character varying,
  "Email" character varying,
  "Website" character varying,
  "AreaId" integer,
  "AreaName" character varying,
  "CityId" integer,
  "CityName" character varying,
  "PincodeId" integer,
  "Pincode" character varying,
  "StateId" integer,
  "StateName" character varying,
  "DistrictId" integer,
  "DistrictName" character varying,
  "DateCreated" timestamp without time zone,
  "DateUpdated" timestamp without time zone,
  "UserCreated" integer,
  "UserUpdated" integer,
  "IsActive" boolean
)
LANGUAGE plpgsql
AS $function$
DECLARE
  v_offset INTEGER;
  v_where_clause TEXT;
  v_total_records INTEGER;
  v_valid_page_size INTEGER;
  v_valid_page_number INTEGER;
  v_order_by TEXT;
  v_order_direction TEXT;
BEGIN
  -- ORDER BY clause
  IF lower(p_order_by) = 'id' THEN
    v_order_by := 'sl.id';
  ELSIF lower(p_order_by) = 'date_created' THEN
    v_order_by := 'sl.date_created';
  ELSIF lower(p_order_by) = 'date_updated' THEN
    v_order_by := 'sl.date_updated';
  ELSE
    v_order_by := 'sl.id';
  END IF;

  IF upper(p_order_direction) = 'ASC' THEN
    v_order_direction := 'ASC';
  ELSE
    v_order_direction := 'DESC';
  END IF;

  -- Pagination
  v_valid_page_size := LEAST(COALESCE(NULLIF(p_page_size, 0), 10), 1000);
  v_valid_page_number := COALESCE(NULLIF(p_page_number, 0), 1);
  v_offset := (v_valid_page_number - 1) * v_valid_page_size;

  -- WHERE clause with user filtering
  v_where_clause := 'WHERE sl.isactive = true AND sl.user_created = ' || p_current_user_id;

  IF p_selected_lead_ids IS NOT NULL AND array_length(p_selected_lead_ids, 1) > 0
     AND NOT (array_length(p_selected_lead_ids, 1) = 1 AND p_selected_lead_ids[1] = 'string') THEN
    v_where_clause := v_where_clause || ' AND sl.lead_id = ANY($7::text[])';
  END IF;

  IF p_search_text IS NOT NULL AND p_search_text != '' AND p_search_text != 'string' THEN
    v_where_clause := v_where_clause || ' AND (
      LOWER(sl.customer_name) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sl.lead_source) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sl.lead_id) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sl.contact_name) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sl.email) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sl.contact_mobile_no) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sl.land_line_no) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sl.status) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sl.lead_type) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sl.website) LIKE ''%'' || LOWER($2) || ''%''
    )';
  END IF;

  IF p_customer_names IS NOT NULL AND array_length(p_customer_names, 1) > 0
     AND NOT (array_length(p_customer_names, 1) = 1 AND p_customer_names[1] = 'string') THEN
    v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($3::varchar[]) AS cn WHERE LOWER(sl.customer_name) LIKE ''%'' || LOWER(cn) || ''%'')';
  END IF;

  IF p_statuses IS NOT NULL AND array_length(p_statuses, 1) > 0
     AND NOT (array_length(p_statuses, 1) = 1 AND p_statuses[1] = 'string') THEN
    v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($4::varchar[]) AS s WHERE LOWER(sl.status) LIKE ''%'' || LOWER(s) || ''%'')';
  END IF;

  IF p_scores IS NOT NULL AND array_length(p_scores, 1) > 0
     AND NOT (array_length(p_scores, 1) = 1 AND p_scores[1] = 'string') THEN
    v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($5::varchar[]) AS sc WHERE LOWER(sl.score) LIKE ''%'' || LOWER(sc) || ''%'')';
  END IF;

  IF p_lead_types IS NOT NULL AND array_length(p_lead_types, 1) > 0
     AND NOT (array_length(p_lead_types, 1) = 1 AND p_lead_types[1] = 'string') THEN
    v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($6::varchar[]) AS lt WHERE LOWER(sl.lead_type) LIKE ''%'' || LOWER(lt) || ''%'')';
  END IF;

  -- Total count
  EXECUTE 'SELECT COUNT(*) FROM sales_lead sl ' || v_where_clause
  INTO v_total_records
  USING p_current_user_id, p_search_text, p_customer_names, p_statuses, p_scores, p_lead_types, p_selected_lead_ids;

  -- Main query
  RETURN QUERY EXECUTE 'WITH base_query AS (
    SELECT sl.*
    FROM sales_lead sl
    ' || v_where_clause || '
    ORDER BY ' || v_order_by || ' ' || v_order_direction || ', sl.id DESC
    LIMIT ' || v_valid_page_size || ' OFFSET ' || v_offset || '
  )
  SELECT
    ' || v_total_records || '::INTEGER AS "TotalRecords",
    bq.id::INTEGER AS "Id",
    CAST(bq.lead_id AS VARCHAR) AS "LeadId",
    CAST(bq.customer_name AS VARCHAR) AS "CustomerName",
    CAST(bq.lead_source AS VARCHAR) AS "LeadSource",
    CAST(bq.lead_source AS VARCHAR) AS "ReferralSourceName",
    CAST(bq.hospital_of_referral AS VARCHAR) AS "HospitalOfReferral",
    CAST(bq.department_of_referral AS VARCHAR) AS "DepartmentOfReferral",
    CAST(bq.city AS VARCHAR) AS "CityOfReferral",
    CAST(bq.social_media AS VARCHAR) AS "SocialMedia",
    CAST(bq.event_date AS TIMESTAMP) AS "EventDate",
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
    0::INTEGER AS "AreaId",
    CAST(bq.area AS VARCHAR) AS "AreaName",
    0::INTEGER AS "CityId",
    CAST(bq.city AS VARCHAR) AS "CityName",
    0::INTEGER AS "PincodeId",
    CAST(bq.pincode AS VARCHAR) AS "Pincode",
    0::INTEGER AS "StateId",
    CAST(bq.state AS VARCHAR) AS "StateName",
    0::INTEGER AS "DistrictId",
    CAST(bq.district AS VARCHAR) AS "DistrictName",
    CAST(bq.date_created AS TIMESTAMP) AS "DateCreated",
    CAST(bq.date_updated AS TIMESTAMP) AS "DateUpdated",
    COALESCE(bq.user_created, 0)::INTEGER AS "UserCreated",
    COALESCE(bq.user_updated, 0)::INTEGER AS "UserUpdated",
    COALESCE(bq.isactive, false)::BOOLEAN AS "IsActive"
  FROM base_query bq'
  USING p_current_user_id, p_search_text, p_customer_names, p_statuses, p_scores, p_lead_types, p_selected_lead_ids;

END;
$function$;

-- Grant execute permissions
GRANT EXECUTE ON FUNCTION sales_lead_grid(text,text[],text[],text[],text[],text[],integer,integer,text,text) TO PUBLIC;
GRANT EXECUTE ON FUNCTION sales_lead_grid(integer,text,text[],text[],text[],text[],text[],integer,integer,text,text) TO PUBLIC;

-- Display all sales_lead_grid function signatures for verification
SELECT 
    proname AS function_name,
    pg_get_function_identity_arguments(oid) AS function_signature,
    prosrc IS NOT NULL AS has_body
FROM pg_proc 
WHERE proname = 'sales_lead_grid'
ORDER BY function_name, oid;
