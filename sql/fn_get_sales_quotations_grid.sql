
DROP FUNCTION IF EXISTS fn_get_sales_quotations_grid(json);
CREATE OR REPLACE FUNCTION fn_get_sales_quotations_grid(
    p_request json
)
RETURNS TABLE (
    "TotalRecords" integer,
    "Id" integer,
    "QuotationId" character varying,
    "CustomerName" character varying,
    "QuotationFor" character varying,
    "Status" character varying,
    "QuotationType" character varying,
    "OrderType" character varying,
    "QuotationDate" timestamp without time zone,
    "ValidTill" timestamp without time zone,
    "Version" character varying,
    "Terms" character varying,
    "Comments" character varying,
    "DeliveryWithin" character varying,
    "DeliveryAfter" character varying,
    "IsActive" boolean,
    "UserCreated" integer,
    "DateCreated" timestamp without time zone,
    "UserUpdated" integer,
    "DateUpdated" timestamp without time zone,
    "CustomerId" integer,
    "LostReason" character varying,
    "OpportunityId" text,
    "LeadId" text,
    "Taxes" character varying,
    "Delivery" character varying,
    "Payment" character varying,
    "Warranty" character varying,
    "FreightCharge" character varying,
    "IsCurrent" boolean,
    "ParentSalesQuotationsId" integer,
    "Products" json
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
    -- Extract parameters
    v_valid_page_size := LEAST(COALESCE((p_request->>'pageSize')::INTEGER, 10), 1000);
    v_valid_page_number := COALESCE((p_request->>'pageNumber')::INTEGER, 1);
    v_offset := (v_valid_page_number - 1) * v_valid_page_size;

    v_order_by := COALESCE(NULLIF(LOWER(p_request->>'orderBy'), ''), 'id');
    v_order_direction := COALESCE(NULLIF(UPPER(p_request->>'orderDirection'), ''), 'DESC');
    IF v_order_by NOT IN ('id', 'date_created', 'date_updated') THEN
        v_order_by := 'id';
    END IF;
    IF v_order_direction NOT IN ('ASC', 'DESC') THEN
        v_order_direction := 'DESC';
    END IF;

    v_where_clause := 'WHERE sq.is_active = true';

    -- UserCreated filter
    IF (p_request->>'userCreated') IS NOT NULL AND (p_request->>'userCreated') != '' AND (p_request->>'userCreated')::INTEGER > 0 THEN
        v_where_clause := v_where_clause || ' AND sq.user_created = ' || quote_literal((p_request->>'userCreated')::INTEGER);
    END IF;

    IF (p_request->'quotationIds') IS NOT NULL AND jsonb_array_length((p_request->'quotationIds')::jsonb) > 0 THEN
        v_where_clause := v_where_clause || ' AND sq.quotation_id = ANY($4::text[])';
    END IF;

    IF (p_request->>'searchText') IS NOT NULL AND p_request->>'searchText' != '' AND p_request->>'searchText' != 'string' THEN
        v_where_clause := v_where_clause || ' AND (' ||
            'LOWER(sq.quotation_for) LIKE ''%'' || LOWER($1) || ''%'' OR ' ||
            'LOWER(sq.quotation_id) LIKE ''%'' || LOWER($1) || ''%'' OR ' ||
            'LOWER(sq.status) LIKE ''%'' || LOWER($1) || ''%'' OR ' ||
            'LOWER(sq.customer_name) LIKE ''%'' || LOWER($1) || ''%'' OR ' ||
            'LOWER(sq.lead_id) LIKE ''%'' || LOWER($1) || ''%'' OR ' ||
            'LOWER(sq.opportunity_id) LIKE ''%'' || LOWER($1) || ''%'' ' ||
        ')';
    END IF;

    IF (p_request->'customerNames') IS NOT NULL AND jsonb_array_length((p_request->'customerNames')::jsonb) > 0 THEN
        v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($2::varchar[]) AS cn WHERE LOWER(sq.customer_name) LIKE ''%'' || LOWER(cn) || ''%'')';
    END IF;

    IF (p_request->'statuses') IS NOT NULL AND jsonb_array_length((p_request->'statuses')::jsonb) > 0 THEN
        v_where_clause := v_where_clause || ' AND EXISTS (SELECT 1 FROM unnest($3::varchar[]) AS s WHERE LOWER(sq.status) LIKE ''%'' || LOWER(s) || ''%'')';
    END IF;

    -- Total count
    EXECUTE 'SELECT COUNT(*) FROM sales_quotations sq ' || v_where_clause
    INTO v_total_records
    USING p_request->>'searchText',
          ARRAY(SELECT jsonb_array_elements_text((p_request->'customerNames')::jsonb)),
          ARRAY(SELECT jsonb_array_elements_text((p_request->'statuses')::jsonb)),
          ARRAY(SELECT jsonb_array_elements_text((p_request->'quotationIds')::jsonb));

    -- Main query
    RETURN QUERY EXECUTE 'WITH base_query AS (
        SELECT
            sq.*
        FROM sales_quotations sq
        ' || v_where_clause || '
        ORDER BY sq.' || v_order_by || ' ' || v_order_direction || ', sq.id DESC
        LIMIT ' || v_valid_page_size || ' OFFSET ' || v_offset || '
    )
    SELECT
        ' || v_total_records || '::INTEGER AS "TotalRecords",
        bq.id::INTEGER AS "Id",
        CAST(bq.quotation_id AS VARCHAR) AS "QuotationId",
        CAST(bq.customer_name AS VARCHAR) AS "CustomerName",
        CAST(bq.quotation_for AS VARCHAR) AS "QuotationFor",
        CAST(bq.status AS VARCHAR) AS "Status",
        CAST(bq.quotation_type AS VARCHAR) AS "QuotationType",
        CAST(bq.order_type AS VARCHAR) AS "OrderType",
        CAST(bq.quotation_date AS TIMESTAMP) AS "QuotationDate",
        CAST(bq.valid_till AS TIMESTAMP) AS "ValidTill",
        CAST(bq.version AS VARCHAR) AS "Version",
        CAST(bq.terms AS VARCHAR) AS "Terms",
        CAST(bq.comments AS VARCHAR) AS "Comments",
        CAST(bq.delivery_within AS VARCHAR) AS "DeliveryWithin",
        CAST(bq.delivery_after AS VARCHAR) AS "DeliveryAfter",
        COALESCE(bq.is_active, false)::BOOLEAN AS "IsActive",
        COALESCE(bq.user_created, 0)::INTEGER AS "UserCreated",
        CAST(bq.date_created AS TIMESTAMP) AS "DateCreated",
        COALESCE(bq.user_updated, 0)::INTEGER AS "UserUpdated",
        CAST(bq.date_updated AS TIMESTAMP) AS "DateUpdated",
        COALESCE(bq.customer_id, 0)::INTEGER AS "CustomerId",
        CAST(bq.lost_reason AS VARCHAR) AS "LostReason",
        CAST(bq.opportunity_id AS TEXT) AS "OpportunityId",
        CAST(bq.lead_id AS TEXT) AS "LeadId",
        CAST(bq.taxes AS VARCHAR) AS "Taxes",
        CAST(bq.delivery AS VARCHAR) AS "Delivery",
        CAST(bq.payment AS VARCHAR) AS "Payment",
        CAST(bq.warranty AS VARCHAR) AS "Warranty",
        CAST(bq.freight_charge AS VARCHAR) AS "FreightCharge",
        COALESCE(bq.is_current, false)::BOOLEAN AS "IsCurrent",
        COALESCE(bq.parent_sales_quotations_id, 0)::INTEGER AS "ParentSalesQuotationsId",
        NULL::json AS "Products"
    FROM base_query bq'
    USING p_request->>'searchText',
          ARRAY(SELECT jsonb_array_elements_text((p_request->'customerNames')::jsonb)),
          ARRAY(SELECT jsonb_array_elements_text((p_request->'statuses')::jsonb)),
          ARRAY(SELECT jsonb_array_elements_text((p_request->'quotationIds')::jsonb));
END;
$function$;