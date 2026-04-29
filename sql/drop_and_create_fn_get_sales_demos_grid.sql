DROP FUNCTION IF EXISTS public.sales_demo_grid(
  integer,text,text[],text[],text[],text[],integer[],integer,integer,text,text
);

CREATE OR REPLACE FUNCTION public.sales_demo_grid(
  p_current_user_id integer,
  p_search_text text DEFAULT NULL::text,
  p_customer_names text[] DEFAULT NULL::text[],
  p_statuses text[] DEFAULT NULL::text[],
  p_demo_approaches text[] DEFAULT NULL::text[],
  p_demo_outcomes text[] DEFAULT NULL::text[],
  p_selected_demo_ids integer[] DEFAULT NULL::integer[],
  p_page_number integer DEFAULT 1,
  p_page_size integer DEFAULT 10,
  p_order_by text DEFAULT 'date_created'::text,
  p_order_direction text DEFAULT 'DESC'::text
)
RETURNS TABLE(
  "TotalRecords" integer,
  "Id" integer,
  "UserCreated" integer,
  "UserCreatedUsername" varchar,
  "UserCreatedRolename" varchar,
  "DateCreated" timestamp,
  "UserUpdated" integer,
  "DateUpdated" timestamp,
  "UserId" integer,
  "DemoDate" timestamp,
  "Status" varchar(100),
  "OpportunityId" varchar(255),
  "CustomerId" integer,
  "DemoContact" varchar(255),
  "CustomerName" varchar(255),
  "DemoName" varchar(255),
  "DemoApproach" varchar(255),
  "DemoOutcome" varchar(255),
  "DemoFeedback" varchar(255),
  "Comments" varchar(255),
  "LeadId" text,
  "ContactMobileNum" varchar(20),
  "Address" varchar(100),
  "PresenterIds" integer[],
  "IsActive" boolean
)
LANGUAGE plpgsql
AS $function$
DECLARE
  v_offset INTEGER;
  v_valid_page_size INTEGER;
  v_valid_page_number INTEGER;
  v_order_by TEXT;
  v_order_direction TEXT;
  v_where_clause TEXT;
  v_total_records INTEGER;
  v_user_role TEXT;
BEGIN
  -- ORDER BY mapping
  IF lower(p_order_by) = 'id' THEN
    v_order_by := 'sd.id';
  ELSIF lower(p_order_by) = 'date_created' THEN
    v_order_by := 'sd.date_created';
  ELSIF lower(p_order_by) = 'date_updated' THEN
    v_order_by := 'sd.date_updated';
  ELSE
    v_order_by := 'sd.id';
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

  -- Determine user role
  SELECT r.rolename INTO v_user_role
  FROM public.userroles ur
    JOIN public.roles r ON ur.roleid = r.roleid
  WHERE ur.userid = p_current_user_id
  ORDER BY ur.id DESC LIMIT 1;

  -- Role-based access logic
  IF v_user_role IN ('Managing Director','Admin','Manager','Marketing Coordinator','Sales Coordinator') THEN
    v_where_clause := 'WHERE sd.isactive = true';
  ELSIF v_user_role = 'Sales Manager' THEN
  v_where_clause := 'WHERE sd.isactive = true AND (sd.user_created IN (SELECT t.userid FROM public.get_salesmanager_child_userids(' || p_current_user_id || ') t) OR sd.user_id = ' || p_current_user_id || ')';
  ELSIF v_user_role IN ('Territory Manager','Field Service Technician') THEN
  v_where_clause := 'WHERE sd.isactive = true AND (sd.user_created IN (SELECT t.userid FROM public.get_salesmanager_child_userids(' || p_current_user_id || ') t) OR sd.user_id = ' || p_current_user_id || ')';
  ELSIF v_user_role = 'Sales Representative' THEN
  v_where_clause := 'WHERE sd.isactive = true AND (sd.user_created = ' || p_current_user_id || ' OR sd.user_id = ' || p_current_user_id || ')';
  ELSE
  v_where_clause := 'WHERE sd.isactive = true AND (sd.user_created = ' || p_current_user_id || ' OR sd.user_id = ' || p_current_user_id || ')';
  END IF;

  -- Dynamic filters
  IF p_selected_demo_ids IS NOT NULL AND array_length(p_selected_demo_ids, 1) > 0 THEN
    v_where_clause := v_where_clause || ' AND sd.id = ANY($7::int[])';
  END IF;

  IF p_search_text IS NOT NULL AND p_search_text != '' THEN
    v_where_clause := v_where_clause || ' AND (
      LOWER(sd.customer_name) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sd.demo_name) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sd.demo_contact) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sd.status) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sd.demo_approach) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sd.demo_outcome) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sd.demo_feedback) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sd.comments) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sd.leadid) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sd.contact_mobile_num) LIKE ''%'' || LOWER($2) || ''%'' OR
      LOWER(sd.address) LIKE ''%'' || LOWER($2) || ''%''
    )';
  END IF;

  IF p_customer_names IS NOT NULL AND array_length(p_customer_names, 1) > 0 THEN
    v_where_clause := v_where_clause || ' AND sd.customer_name = ANY($3::varchar[])';
  END IF;

  IF p_statuses IS NOT NULL AND array_length(p_statuses, 1) > 0 THEN
    v_where_clause := v_where_clause || ' AND sd.status = ANY($4::varchar[])';
  END IF;

  IF p_demo_approaches IS NOT NULL AND array_length(p_demo_approaches, 1) > 0 THEN
    v_where_clause := v_where_clause || ' AND sd.demo_approach = ANY($5::varchar[])';
  END IF;

  IF p_demo_outcomes IS NOT NULL AND array_length(p_demo_outcomes, 1) > 0 THEN
    v_where_clause := v_where_clause || ' AND sd.demo_outcome = ANY($6::varchar[])';
  END IF;

  -- Total record count
  EXECUTE 'SELECT COUNT(*) FROM public.sales_demos sd ' || v_where_clause
  INTO v_total_records
  USING p_current_user_id, p_search_text, p_customer_names, p_statuses, p_demo_approaches, p_demo_outcomes, p_selected_demo_ids;

  -- Main query
  RETURN QUERY EXECUTE 'WITH base_query AS (
    SELECT
      sd.*,
      u.username AS user_created_username,
      (
        SELECT r.rolename FROM public.userroles ur2
        JOIN public.roles r ON ur2.roleid = r.roleid
        WHERE ur2.userid = sd.user_created
        ORDER BY ur2.id DESC LIMIT 1
      ) AS user_created_role
    FROM public.sales_demos sd
    LEFT JOIN public.users u ON sd.user_created = u.userid
    ' || v_where_clause || '
    ORDER BY ' || v_order_by || ' ' || v_order_direction || ', sd.id DESC
    LIMIT ' || v_valid_page_size || ' OFFSET ' || v_offset || '
  )
  SELECT
    ' || v_total_records || '::INTEGER AS "TotalRecords",
    bq.id::INTEGER AS "Id",
    COALESCE(bq.user_created,0)::INTEGER AS "UserCreated",
    CAST(bq.user_created_username AS VARCHAR) AS "UserCreatedUsername",
    CAST(bq.user_created_role AS VARCHAR) AS "UserCreatedRolename",
    CAST(bq.date_created AS TIMESTAMP) AS "DateCreated",
    COALESCE(bq.user_updated,0)::INTEGER AS "UserUpdated",
    CAST(bq.date_updated AS TIMESTAMP) AS "DateUpdated",
  bq.user_id::INTEGER AS "UserId",
    CAST(bq.demo_date AS TIMESTAMP) AS "DemoDate",
    CAST(bq.status AS VARCHAR) AS "Status",
    CAST(bq.opportunity_id AS VARCHAR) AS "OpportunityId",
    bq.customer_id::INTEGER AS "CustomerId",
    CAST(bq.demo_contact AS VARCHAR) AS "DemoContact",
    CAST(bq.customer_name AS VARCHAR) AS "CustomerName",
    CAST(bq.demo_name AS VARCHAR) AS "DemoName",
    CAST(bq.demo_approach AS VARCHAR) AS "DemoApproach",
    CAST(bq.demo_outcome AS VARCHAR) AS "DemoOutcome",
    CAST(bq.demo_feedback AS VARCHAR) AS "DemoFeedback",
    CAST(bq.comments AS VARCHAR) AS "Comments",
    CAST(bq.leadid AS TEXT) AS "LeadId",
    CAST(bq.contact_mobile_num AS VARCHAR) AS "ContactMobileNum",
    CAST(bq.address AS VARCHAR) AS "Address",
    bq.presenter_ids::INTEGER[] AS "PresenterIds",
    COALESCE(bq.isactive,false)::BOOLEAN AS "IsActive"
  FROM base_query bq'
  USING p_current_user_id, p_search_text, p_customer_names, p_statuses, p_demo_approaches, p_demo_outcomes, p_selected_demo_ids;

END;
$function$;

---------------------=====================--------------------------------------------

DROP FUNCTION IF EXISTS sp_get_salesdemo_cards_count_by_user(integer);

CREATE OR REPLACE FUNCTION sp_get_salesdemo_cards_count_by_user(p_user_id integer)
RETURNS TABLE (
  "demoRequested" BIGINT,
  "demoScheduled" BIGINT,
  "demoCompleted" BIGINT,
  "demoCancelled" BIGINT
) AS $$
DECLARE
  v_user_role TEXT;
  v_user_filter TEXT;
BEGIN
  -- Determine user role
  SELECT r.rolename INTO v_user_role
  FROM public.userroles ur
    JOIN public.roles r ON ur.roleid = r.roleid
  WHERE ur.userid = p_user_id
  ORDER BY ur.id DESC LIMIT 1;

  -- Set user filter based on role
  IF v_user_role IN ('Managing Director', 'Admin', 'Manager', 'Marketing Coordinator', 'Sales Coordinator') THEN
    v_user_filter := '1=1';
  ELSIF v_user_role = 'Sales Manager' THEN
    v_user_filter := '(sd.user_created IN (SELECT t.userid FROM public.get_salesmanager_child_userids(' || p_user_id || ') t) OR sd.user_id = ' || p_user_id || ')';
  ELSE
    v_user_filter := '(sd.user_created = ' || p_user_id || ' OR sd.user_id = ' || p_user_id || ')';
  END IF;

  -- Return role-filtered counts
  RETURN QUERY EXECUTE '
    SELECT
      COUNT(*) FILTER (WHERE sd.status IN (''Requested'', ''Demo Requested''))::BIGINT AS "demoRequested",
      COUNT(*) FILTER (WHERE sd.status IN (''Scheduled'', ''Demo Scheduled''))::BIGINT AS "demoScheduled",
      COUNT(*) FILTER (WHERE sd.status IN (''Completed'', ''Demo Completed''))::BIGINT AS "demoCompleted",
      COUNT(*) FILTER (WHERE sd.status IN (''Cancelled'', ''Demo Cancelled''))::BIGINT AS "demoCancelled"
    FROM public.sales_demos sd
    WHERE ' || v_user_filter;

END;
$$ LANGUAGE plpgsql;

GRANT EXECUTE ON FUNCTION sp_get_salesdemo_cards_count_by_user TO PUBLIC;
