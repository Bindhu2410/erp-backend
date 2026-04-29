-- public.sales_quotations definition
 
-- Drop table
 
-- DROP TABLE public.sales_quotations;
 
CREATE TABLE public.sales_quotations (
	id serial4 NOT NULL,
	user_created int4 NULL,
	date_created timestamp NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	"version" varchar(255) NULL,
	terms varchar(255) NULL,
	valid_till timestamp NULL,
	quotation_for varchar(255) NULL,
	status varchar(255) NULL,
	lost_reason varchar(255) NULL,
	customer_id int4 NULL,
	quotation_type varchar(255) NULL,
	quotation_date timestamp NULL,
	order_type varchar(255) NULL,
	"comments" varchar(255) NULL,
	delivery_within varchar(255) NULL,
	delivery_after varchar(255) NULL,
	is_active bool DEFAULT false NOT NULL,
	quotation_id varchar(255) NULL,
	customer_name varchar(255) NULL,
	taxes varchar(255) NULL,
	delivery varchar(255) NULL,
	payment varchar(255) NULL,
	warranty varchar(255) NULL,
	freight_charge varchar(255) NULL,
	is_current bool NULL,
	parent_sales_quotations_id int4 NULL,
	lead_id text NULL,
	opportunity_id text NULL,
	tax int4 NULL,
	discount int4 NULL,
	freight_charges int4 NULL,
	CONSTRAINT sales_quotations_pkey PRIMARY KEY (id)
);
 
 
-- public.sales_quotations foreign keys
 
ALTER TABLE public.sales_quotations ADD CONSTRAINT fk_sales_quotation_customer FOREIGN KEY (customer_id) REFERENCES public.sales_customers(id) ON DELETE SET NULL;
ALTER TABLE public.sales_quotations ADD CONSTRAINT fk_sales_quotation_parent FOREIGN KEY (parent_sales_quotations_id) REFERENCES public.sales_quotations(id) ON DELETE SET NULL;
ALTER TABLE public.sales_quotations ADD CONSTRAINT fk_sales_quotation_user_created FOREIGN KEY (user_created) REFERENCES public.users(user_id) ON DELETE SET NULL;
ALTER TABLE public.sales_quotations ADD CONSTRAINT fk_sales_quotation_user_updated FOREIGN KEY (user_updated) REFERENCES public.users(user_id) ON DELETE SET NULL;
 
 alter table sales_quotations 
add column contact_name varchar(200),
add column contact_mobile_no varchar(100);

ALTER TABLE public.sales_quotations 
    ADD COLUMN assigned_to INT;

-- Add foreign key constraint
ALTER TABLE public.sales_quotations 
    ADD CONSTRAINT fk_sales_quotation_assigned_to
    FOREIGN KEY (assigned_to) REFERENCES public.users(userid);
-- Function: fn_get_sales_quotations_grid


CREATE OR REPLACE FUNCTION fn_get_sales_quotations_grid(p_request jsonb)
RETURNS TABLE(
    id int4,
    user_created int4,
    date_created timestamp,
    user_updated int4,
    date_updated timestamp,
    version varchar(255),
    terms varchar(255),
    valid_till timestamp,
    quotation_for varchar(255),
    status varchar(255),
    lost_reason varchar(255),
    customer_id int4,
    quotation_type varchar(255),
    quotation_date timestamp,
    order_type varchar(255),
    comments varchar(255),
    delivery_within varchar(255),
    delivery_after varchar(255),
    is_active bool,
    quotation_id varchar(255),
    customer_name varchar(255),
    taxes varchar(255),
    delivery varchar(255),
    payment varchar(255),
    warranty varchar(255),
    freight_charge varchar(255),
    is_current bool,
    parent_sales_quotations_id int4,
    lead_id text,
    opportunity_id text,
    tax int4,
    discount int4,
    freight_charges int4,
    contact_name varchar(200),
    contact_mobile_no varchar(100),
    assigned_to int,
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
    v_quotationTypes text[];
    v_leadIds text[];
    v_user_role text;
    v_where_clause text;
    v_valid_page_size integer;
    v_valid_page_number integer;
    v_offset integer;
BEGIN
    IF v_currentUserId IS NULL OR v_currentUserId = 0 THEN
        RAISE EXCEPTION 'CurrentUserId parameter is required and must be greater than 0';
    END IF;

    v_valid_page_size := LEAST(COALESCE(NULLIF(v_pageSize, 0), 10), 1000);
    v_valid_page_number := COALESCE(NULLIF(v_pageNumber, 0), 1);
    v_offset := (v_valid_page_number - 1) * v_valid_page_size;

    SELECT r.rolename INTO v_user_role
    FROM public.userroles ur
        JOIN public.roles r ON ur.roleid = r.roleid
    WHERE ur.userid = v_currentUserId
    ORDER BY ur.id DESC LIMIT 1;

    IF v_user_role IN ('Managing Director', 'Admin', 'Manager', 'Marketing Coordinator', 'Sales Coordinator') THEN
        v_where_clause := 'WHERE sq.is_active = true';
    ELSIF v_user_role = 'Sales Manager' THEN
        v_where_clause := 'WHERE sq.is_active = true AND (sq.user_created IN (SELECT t.userid FROM public.get_salesmanager_child_userids(' || v_currentUserId || ') t) OR sq.assigned_to = ' || v_currentUserId || ')';
    ELSIF v_user_role IN ('Territory Manager', 'Field Service Technician') THEN
        v_where_clause := 'WHERE sq.is_active = true AND (sq.user_created IN (SELECT t.userid FROM public.get_salesmanager_child_userids(' || v_currentUserId || ') t) OR sq.assigned_to = ' || v_currentUserId || ')';
    ELSIF v_user_role = 'Sales Representative' THEN
        v_where_clause := 'WHERE sq.is_active = true AND (sq.user_created = ' || v_currentUserId || ' OR sq.assigned_to = ' || v_currentUserId || ')';
    ELSE
        v_where_clause := 'WHERE sq.is_active = true AND (sq.user_created = ' || v_currentUserId || ' OR sq.assigned_to = ' || v_currentUserId || ')';
    END IF;

    -- Parse JSON arrays safely
    BEGIN
        IF p_request->'Statuses' IS NOT NULL AND jsonb_typeof(p_request->'Statuses') = 'array' THEN
            v_statuses := ARRAY(SELECT jsonb_array_elements_text(p_request->'Statuses'));
        ELSE
            v_statuses := ARRAY[]::text[];
        END IF;
    EXCEPTION WHEN OTHERS THEN
        v_statuses := ARRAY[]::text[];
    END;

    BEGIN
        IF p_request->'CustomerNames' IS NOT NULL AND jsonb_typeof(p_request->'CustomerNames') = 'array' THEN
            v_customerNames := ARRAY(SELECT jsonb_array_elements_text(p_request->'CustomerNames'));
        ELSE
            v_customerNames := ARRAY[]::text[];
        END IF;
    EXCEPTION WHEN OTHERS THEN
        v_customerNames := ARRAY[]::text[];
    END;

    BEGIN
        IF p_request->'QuotationTypes' IS NOT NULL AND jsonb_typeof(p_request->'QuotationTypes') = 'array' THEN
            v_quotationTypes := ARRAY(SELECT jsonb_array_elements_text(p_request->'QuotationTypes'));
        ELSE
            v_quotationTypes := ARRAY[]::text[];
        END IF;
    EXCEPTION WHEN OTHERS THEN
        v_quotationTypes := ARRAY[]::text[];
    END;

    BEGIN
        IF p_request->'LeadIds' IS NOT NULL AND jsonb_typeof(p_request->'LeadIds') = 'array' THEN
            v_leadIds := ARRAY(SELECT jsonb_array_elements_text(p_request->'LeadIds'));
        ELSE
            v_leadIds := ARRAY[]::text[];
        END IF;
    EXCEPTION WHEN OTHERS THEN
        v_leadIds := ARRAY[]::text[];
    END;

    -- Search Text
    IF v_searchText IS NOT NULL AND v_searchText != '' AND v_searchText != 'string' THEN
        v_where_clause := v_where_clause || ' AND ('
            || 'LOWER(sq.customer_name) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(sq.quotation_id) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(sq.contact_name) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(sq.status) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(sq.quotation_type) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(sq.comments) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(sq.lead_id) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(sq.contact_mobile_no) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'' OR '
            || 'LOWER(sq.opportunity_id) LIKE ''%'' || LOWER(''' || replace(v_searchText, '''', '''''') || ''') || ''%'''
            || ')';
    END IF;

    -- Array Filters (Fixed with string_to_array)
    IF array_length(v_customerNames, 1) > 0 AND NOT (array_length(v_customerNames, 1) = 1 AND v_customerNames[1] = 'string') THEN
        v_where_clause := v_where_clause || 
        ' AND EXISTS (
            SELECT 1 FROM unnest(string_to_array(''' || array_to_string(v_customerNames, ',') || ''', '','')) AS cn
            WHERE LOWER(sq.customer_name) LIKE ''%'' || LOWER(cn) || ''%''
        )';
    END IF;

    IF array_length(v_statuses, 1) > 0 AND NOT (array_length(v_statuses, 1) = 1 AND v_statuses[1] = 'string') THEN
        v_where_clause := v_where_clause || 
        ' AND EXISTS (
            SELECT 1 FROM unnest(string_to_array(''' || array_to_string(v_statuses, ',') || ''', '','')) AS s
            WHERE LOWER(sq.status) LIKE ''%'' || LOWER(s) || ''%''
        )';
    END IF;

    IF array_length(v_quotationTypes, 1) > 0 AND NOT (array_length(v_quotationTypes, 1) = 1 AND v_quotationTypes[1] = 'string') THEN
        v_where_clause := v_where_clause || 
        ' AND EXISTS (
            SELECT 1 FROM unnest(string_to_array(''' || array_to_string(v_quotationTypes, ',') || ''', '','')) AS qt
            WHERE LOWER(sq.quotation_type) LIKE ''%'' || LOWER(qt) || ''%''
        )';
    END IF;

    IF array_length(v_leadIds, 1) > 0 AND NOT (array_length(v_leadIds, 1) = 1 AND v_leadIds[1] = 'string') THEN
        v_where_clause := v_where_clause || 
        ' AND EXISTS (
            SELECT 1 FROM unnest(string_to_array(''' || array_to_string(v_leadIds, ',') || ''', '','')) AS lid
            WHERE LOWER(sq.lead_id) LIKE ''%'' || LOWER(lid) || ''%''
        )';
    END IF;

    -- Final Query
    RETURN QUERY EXECUTE 'SELECT
        sq.id,
        sq.user_created,
        sq.date_created,
        sq.user_updated,
        sq.date_updated,
        sq.version,
        sq.terms,
        sq.valid_till,
        sq.quotation_for,
        sq.status,
        sq.lost_reason,
        sq.customer_id,
        sq.quotation_type,
        sq.quotation_date,
        sq.order_type,
        sq.comments,
        sq.delivery_within,
        sq.delivery_after,
        sq.is_active,
        sq.quotation_id,
        sq.customer_name,
        sq.taxes,
        sq.delivery,
        sq.payment,
        sq.warranty,
        sq.freight_charge,
        sq.is_current,
        sq.parent_sales_quotations_id,
        sq.lead_id,
        sq.opportunity_id,
        sq.tax,
        sq.discount,
        sq.freight_charges,
        sq.contact_name,
        sq.contact_mobile_no,
        sq.assigned_to,
        COUNT(*) OVER()::integer AS totalrecords
    FROM public.sales_quotations sq
    ' || v_where_clause ||
    ' ORDER BY ' ||
    CASE 
        WHEN v_orderBy = 'date_created' AND v_orderDirection = 'ASC' THEN 'sq.date_created ASC'
        WHEN v_orderBy = 'date_created' AND v_orderDirection = 'DESC' THEN 'sq.date_created DESC'
        WHEN v_orderBy = 'date_updated' AND v_orderDirection = 'ASC' THEN 'sq.date_updated ASC'
        WHEN v_orderBy = 'date_updated' AND v_orderDirection = 'DESC' THEN 'sq.date_updated DESC'
        WHEN v_orderBy = 'id' AND v_orderDirection = 'ASC' THEN 'sq.id ASC'
        WHEN v_orderBy = 'id' AND v_orderDirection = 'DESC' THEN 'sq.id DESC'
        ELSE 'sq.date_created DESC, sq.id DESC' 
    END ||
    ' LIMIT ' || v_valid_page_size || ' OFFSET ' || v_offset;
END;
$$ LANGUAGE plpgsql;
