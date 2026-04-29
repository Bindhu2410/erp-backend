DROP FUNCTION fn_get_sales_opportunities_grid_by_user(jsonb) ;
DROP FUNCTION get_sales_products_by_opportunity(integer,integer) ;

-- Main function: returns full grid including item columns
DROP FUNCTION IF EXISTS fn_get_sales_opportunities_grid(jsonb);
CREATE OR REPLACE FUNCTION fn_get_sales_opportunities_grid(p_request jsonb)
RETURNS TABLE(
    id varchar(255),
    user_created int4,
    date_created timestamp,
    user_updated int4,
    date_updated timestamp,
    status varchar(255),
    expected_completion date,
    opportunity_type varchar(255),
    opportunity_for varchar(255),
    customer_id varchar(255),
    customer_name varchar(255),
    customer_type varchar(255),
    opportunity_name varchar(255),
    opportunity_id varchar(255),
    comments text,
    isactive boolean,
    lead_id varchar(255),
    sales_representative_id int4,
    contact_name varchar(255),
    contact_mobile_no varchar(255),
    item_id int4,
    item_code varchar(255),
    item_name varchar(255),
    make varchar(255),
    model varchar(255),
    brand varchar(255),
    category_id int4,
    category_name varchar(255),
    uom_id int4,
    qty int4,
    amount numeric(18,2),
    unit_price numeric(12,2),
    item_is_active boolean,
    totalrecords integer
) AS $$
DECLARE
    v_searchText text := p_request->>'SearchText';
    v_pageNumber integer := COALESCE((p_request->>'PageNumber')::integer, 1);
    v_pageSize integer := COALESCE((p_request->>'PageSize')::integer, 10);
    v_orderBy text := COALESCE(p_request->>'OrderBy', 'date_created');
    v_orderDirection text := COALESCE(p_request->>'OrderDirection', 'DESC');
    v_currentUserId integer := (p_request->>'CurrentUserId')::integer;

    v_statuses text[];
    v_customerNames text[];
    v_opportunityTypes text[];
    v_leadIds text[];
    v_user_role text;
    v_where_clause text := '';  -- start empty, will be populated below
    v_valid_page_size integer;
    v_valid_page_number integer;
    v_offset integer;
BEGIN
    -- Validate that CurrentUserId is provided
    IF v_currentUserId IS NULL OR v_currentUserId = 0 THEN
        RAISE EXCEPTION 'CurrentUserId parameter is required and must be greater than 0';
    END IF;

    -- Pagination validation
    v_valid_page_size := LEAST(COALESCE(NULLIF(v_pageSize, 0), 10), 1000);
    v_valid_page_number := COALESCE(NULLIF(v_pageNumber, 0), 1);
    v_offset := (v_valid_page_number - 1) * v_valid_page_size;

    -- Determine user role (if no role found, default behavior below applies)
    SELECT r.rolename INTO v_user_role
    FROM public.userroles ur
    JOIN public.roles r ON ur.roleid = r.roleid
    WHERE ur.userid = v_currentUserId
    ORDER BY ur.id DESC LIMIT 1;

    -- Set base WHERE clause based on role
    IF v_user_role IN ('Managing Director', 'Admin', 'Manager', 'Marketing Coordinator', 'Sales Coordinator') THEN
        v_where_clause := 'WHERE so.isactive = true';
    ELSIF v_user_role = 'Sales Manager' THEN
        v_where_clause := 'WHERE so.isactive = true AND (so.user_created IN (SELECT t.userid FROM public.get_salesmanager_child_userids(' || v_currentUserId || ') t) OR so.sales_representative_id = ' || v_currentUserId || ')';
    ELSIF v_user_role IN ('Territory Manager', 'Field Service Technician') THEN
        v_where_clause := 'WHERE so.isactive = true AND (so.user_created IN (SELECT t.userid FROM public.get_salesmanager_child_userids(' || v_currentUserId || ') t) OR so.sales_representative_id = ' || v_currentUserId || ')';
    ELSIF v_user_role = 'Sales Representative' THEN
        v_where_clause := 'WHERE so.isactive = true AND (so.user_created = ' || v_currentUserId || ' OR so.sales_representative_id = ' || v_currentUserId || ')';
    ELSE
        v_where_clause := 'WHERE so.isactive = true AND (so.user_created = ' || v_currentUserId || ' OR so.sales_representative_id = ' || v_currentUserId || ')';
    END IF;

    -- Safely extract arrays from JSON (array elements text)
    BEGIN
        IF p_request->'Statuses' IS NOT NULL AND jsonb_typeof(p_request->'Statuses') = 'array' THEN
            v_statuses := ARRAY(SELECT jsonb_array_elements_text(p_request->'Statuses'));
        ELSE
            v_statuses := ARRAY[]::text[];
        END IF;
    EXCEPTION WHEN OTHERS THEN v_statuses := ARRAY[]::text[]; END;

    BEGIN
        IF p_request->'CustomerNames' IS NOT NULL AND jsonb_typeof(p_request->'CustomerNames') = 'array' THEN
            v_customerNames := ARRAY(SELECT jsonb_array_elements_text(p_request->'CustomerNames'));
        ELSE
            v_customerNames := ARRAY[]::text[];
        END IF;
    EXCEPTION WHEN OTHERS THEN v_customerNames := ARRAY[]::text[]; END;

    BEGIN
        IF p_request->'OpportunityTypes' IS NOT NULL AND jsonb_typeof(p_request->'OpportunityTypes') = 'array' THEN
            v_opportunityTypes := ARRAY(SELECT jsonb_array_elements_text(p_request->'OpportunityTypes'));
        ELSE
            v_opportunityTypes := ARRAY[]::text[];
        END IF;
    EXCEPTION WHEN OTHERS THEN v_opportunityTypes := ARRAY[]::text[]; END;

    BEGIN
        IF p_request->'LeadIds' IS NOT NULL AND jsonb_typeof(p_request->'LeadIds') = 'array' THEN
            v_leadIds := ARRAY(SELECT jsonb_array_elements_text(p_request->'LeadIds'));
        ELSE
            v_leadIds := ARRAY[]::text[];
        END IF;
    EXCEPTION WHEN OTHERS THEN v_leadIds := ARRAY[]::text[]; END;

    -- Enhanced search filter (append to v_where_clause)
    IF v_searchText IS NOT NULL AND v_searchText != '' AND v_searchText != 'string' THEN
        v_where_clause := v_where_clause || ' AND ('
            || 'LOWER(so.customer_name) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(so.opportunity_name) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(so.contact_name) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(so.status) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(so.opportunity_type) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(so.customer_type) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(so.comments) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(so.lead_id) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(so.contact_mobile_no) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(so.opportunity_id) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'''
            || ')';
    END IF;

    -- Array filters with partial matching (append to v_where_clause)
    IF array_length(v_customerNames, 1) > 0 AND NOT (array_length(v_customerNames, 1) = 1 AND v_customerNames[1] = 'string') THEN
        v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest(''' || array_to_string(v_customerNames, ',') || '''::varchar[]) AS cn WHERE LOWER(so.customer_name) LIKE ''%'' || LOWER(cn) || ''%'')';
    END IF;

    IF array_length(v_statuses, 1) > 0 AND NOT (array_length(v_statuses, 1) = 1 AND v_statuses[1] = 'string') THEN
        v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest(''' || array_to_string(v_statuses, ',') || '''::varchar[]) AS s WHERE LOWER(so.status) LIKE ''%'' || LOWER(s) || ''%'')';
    END IF;

    IF array_length(v_opportunityTypes, 1) > 0 AND NOT (array_length(v_opportunityTypes, 1) = 1 AND v_opportunityTypes[1] = 'string') THEN
        v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest(''' || array_to_string(v_opportunityTypes, ',') || '''::varchar[]) AS ot WHERE LOWER(so.opportunity_type) LIKE ''%'' || LOWER(ot) || ''%'')';
    END IF;

    IF array_length(v_leadIds, 1) > 0 AND NOT (array_length(v_leadIds, 1) = 1 AND v_leadIds[1] = 'string') THEN
        v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest(''' || array_to_string(v_leadIds, ',') || '''::varchar[]) AS lid WHERE LOWER(so.lead_id) LIKE ''%'' || LOWER(lid) || ''%'')';
    END IF;

    -- Execute main query with item join and user filter
    -- Build an inner SQL that returns all matching rows (may contain multiple rows per opportunity)
    -- then de-duplicate by picking the latest row per opportunity (by date_created).
    -- Also compute TotalRecords as the distinct count of opportunity ids from the inner set.
    DECLARE
        v_inner_sql text;
        v_final_sql text;
        v_order_clause text;
    BEGIN
        v_order_clause := CASE 
            WHEN v_orderBy = 'date_created' AND v_orderDirection = 'ASC' THEN 'date_created ASC'
            WHEN v_orderBy = 'date_created' AND v_orderDirection = 'DESC' THEN 'date_created DESC'
            WHEN v_orderBy = 'date_updated' AND v_orderDirection = 'ASC' THEN 'date_updated ASC'
            WHEN v_orderBy = 'date_updated' AND v_orderDirection = 'DESC' THEN 'date_updated DESC'
            WHEN v_orderBy = 'id' AND v_orderDirection = 'ASC' THEN 'id ASC'
            WHEN v_orderBy = 'id' AND v_orderDirection = 'DESC' THEN 'id DESC'
            ELSE 'date_created DESC, id DESC' 
        END;

        v_inner_sql := 'SELECT
            so.id,
            so.user_created,
            so.date_created,
            so.user_updated,
            so.date_updated,
            so.status,
            so.expected_completion,
            so.opportunity_type,
            so.opportunity_for,
            so.customer_id,
            so.customer_name,
            so.customer_type,
            so.opportunity_name,
            so.opportunity_id,
            so.comments,
            so.isactive,
            so.lead_id,
            so.sales_representative_id,
            so.contact_name,
            so.contact_mobile_no,
            sp.item_id,
            im.item_code,
            im.item_name,
            m.name AS make,
            mo.name AS model,
            p.name AS product,
            im.category_id,
            cat.name as category_name,
            im.uom_id,
            sp.qty,
            CAST(sp.amount AS numeric(18,2)) AS amount,
            CAST(sp.unit_price AS numeric(12,2)) AS unit_price,
            im.is_active as item_is_active
        FROM public.sales_opportunities so
        LEFT JOIN sales_product sp ON sp.stage_item_id = so.id::varchar
        LEFT JOIN item_master im ON sp.item_id = im.id
        LEFT JOIN make m ON im.make_id = m.id
        LEFT JOIN model mo ON im.model_id = mo.id
        LEFT JOIN product p ON im.product_id = p.id
        LEFT JOIN categories cat ON im.category_id = cat.id
        ' || v_where_clause ||
        -- Keep opportunities that have no sales_product row (sp.item_id IS NULL),
        -- otherwise require active product + active item and user ownership
        ' AND ( (sp.item_id IS NULL) OR (sp.is_active = true AND im.is_active = true AND (sp.user_created = ' || v_currentUserId || ' OR sp.user_updated = ' || v_currentUserId || ')) )';

        v_final_sql := 'WITH base AS (' || v_inner_sql || '), numbered AS (
                SELECT *, ROW_NUMBER() OVER (PARTITION BY id ORDER BY date_created DESC) AS rn
                FROM base
            )
            SELECT
                id,
                user_created,
                date_created,
                user_updated,
                date_updated,
                status,
                expected_completion,
                opportunity_type,
                opportunity_for,
                customer_id,
                customer_name,
                customer_type,
                opportunity_name,
                opportunity_id,
                comments,
                isactive,
                lead_id,
                sales_representative_id,
                contact_name,
                contact_mobile_no,
                item_id,
                item_code,
                item_name,
                make,
                model,
                product,
                category_id,
                category_name,
                uom_id,
                qty,
                amount,
                unit_price,
                item_is_active,
                (SELECT COUNT(DISTINCT id) FROM base)::integer AS totalrecords
            FROM numbered
            WHERE rn = 1
            ORDER BY ' || v_order_clause || ' LIMIT ' || v_valid_page_size || ' OFFSET ' || v_offset;

        RETURN QUERY EXECUTE v_final_sql;
    END;
END;
$$ LANGUAGE plpgsql;
-----------------------

-- Wrapper function: returns the smaller shape (no item columns) — used by API
DROP FUNCTION IF EXISTS fn_get_sales_opportunities_grid_by_user(jsonb);
CREATE OR REPLACE FUNCTION fn_get_sales_opportunities_grid_by_user(p_request jsonb)
RETURNS TABLE(
    id varchar(255),
    user_created int4,
    date_created timestamp,
    user_updated int4,
    date_updated timestamp,
    status varchar(255),
    expected_completion date,
    opportunity_type varchar(255),
    opportunity_for varchar(255),
    customer_id varchar(255),
    customer_name varchar(255),
    customer_type varchar(255),
    opportunity_name varchar(255),
    opportunity_id varchar(255),
    comments text,
    isactive boolean,
    lead_id varchar(255),
    sales_representative_id int4,
    contact_name varchar(255),
    contact_mobile_no varchar(255),
    TotalRecords integer
) AS $$
DECLARE
    v_userCreated integer := (p_request->>'UserCreated')::integer;
    v_updated_request jsonb;
BEGIN
    IF v_userCreated IS NULL OR v_userCreated = 0 THEN
        RAISE EXCEPTION 'UserCreated parameter is required and must be greater than 0';
    END IF;

    -- Build a request compatible with the main function
    v_updated_request := p_request || jsonb_build_object('CurrentUserId', v_userCreated);

    -- Explicit projection ensures the wrapper RETURNS TABLE shape matches the selected columns
    RETURN QUERY
    SELECT
        f.id,
        f.user_created,
        f.date_created,
        f.user_updated,
        f.date_updated,
        f.status,
        f.expected_completion,
        f.opportunity_type,
        f.opportunity_for,
        f.customer_id,
        f.customer_name,
        f.customer_type,
        f.opportunity_name,
        f.opportunity_id,
        f.comments,
        f.isactive,
        f.lead_id,
        f.sales_representative_id,
        f.contact_name,
        f.contact_mobile_no,
        f.totalrecords::integer AS TotalRecords
    FROM fn_get_sales_opportunities_grid(v_updated_request) f;
END;
$$ LANGUAGE plpgsql;