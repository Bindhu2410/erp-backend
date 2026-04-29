CREATE OR REPLACE FUNCTION sp_create_sales_order_from_quotation(p_quotation_id INT, p_user_created INT)
RETURNS SETOF sales_orders AS $$
DECLARE
    v_quotation RECORD;
    v_order_id VARCHAR;
    v_sales_order sales_orders;
BEGIN
    -- Validate quotation exists and is Approved (case-insensitive)
    SELECT * INTO v_quotation
    FROM public.sales_quotations
    WHERE id = p_quotation_id AND LOWER(status) = LOWER('Approved');

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Quotation not found or not approved';
    END IF;

    -- Generate unique order ID
    v_order_id := fn_generate_order_id();

    -- Create Sales Order
    INSERT INTO public.sales_orders (
        order_id,
        customer_id,
        order_date,
        status,
        quotation_id,
        total_amount,
        notes,
        user_created,
        date_created
    )
    VALUES (
        v_order_id,
        v_quotation.customer_id,
        CURRENT_TIMESTAMP,
        'Draft',
        v_quotation.id,
        0.00, -- You may want to sum item prices here
        v_quotation.comments,
        p_user_created,
        CURRENT_TIMESTAMP
    )
    RETURNING * INTO v_sales_order;

    -- TODO: Copy item details from quotation to sales order items table if applicable

    RETURN NEXT v_sales_order;
END;
$$ LANGUAGE plpgsql;

DROP FUNCTION IF EXISTS fn_get_quotation_with_order(integer);

CREATE OR REPLACE FUNCTION fn_get_quotation_with_order(p_quotation_id INT)
RETURNS TABLE (
    id INT,
    user_created INT,
    date_created TIMESTAMP,
    user_updated INT,
    date_updated TIMESTAMP,
    version VARCHAR,
    terms VARCHAR,
    valid_till TIMESTAMP,
    quotation_for VARCHAR,
    status VARCHAR,
    lost_reason VARCHAR,
    customer_id INT,
    quotation_type VARCHAR,
    quotation_date TIMESTAMP,
    order_type VARCHAR,
    comments VARCHAR,
    delivery_within VARCHAR,
    delivery_after VARCHAR,
    is_active BOOL,
    quotation_id VARCHAR,
    customer_name VARCHAR,
    taxes VARCHAR,
    delivery VARCHAR,
    payment VARCHAR,
    warranty VARCHAR,
    freight_charge VARCHAR,
    is_current BOOL,
    parent_sales_quotations_id INT,
    lead_id VARCHAR,
    opportunity_id TEXT,
    sales_order_id INT,
    order_id VARCHAR,
    sales_order_status VARCHAR,
    total_amount NUMERIC,
    tax_amount NUMERIC,
    grand_total NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        q.id,
        q.user_created,
        q.date_created,
        q.user_updated,
        q.date_updated,
        q.version,
        q.terms,
        q.valid_till,
        q.quotation_for,
        q.status,
        q.lost_reason,
        q.customer_id,
        q.quotation_type,
        q.quotation_date,
        q.order_type,
        q.comments,
        q.delivery_within,
        q.delivery_after,
        q.is_active,
        q.quotation_id,
        q.customer_name,
        q.taxes,
        q.delivery,
        q.payment,
        q.warranty,
        q.freight_charge,
        q.is_current,
        q.parent_sales_quotations_id,
        CAST(q.lead_id AS VARCHAR),
        q.opportunity_id,
        so.id AS sales_order_id,
        so.order_id,
        so.status AS sales_order_status,
        so.total_amount,
        so.tax_amount,
        so.grand_total
    FROM public.sales_quotations q
    LEFT JOIN public.sales_orders so ON so.quotation_id = q.id
    WHERE q.id = p_quotation_id;
END;
$$ LANGUAGE plpgsql;

