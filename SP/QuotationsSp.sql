-- Create Stored Procedures for Sales Quotations

-- Get all quotations
CREATE OR REPLACE FUNCTION get_all_quotations()
RETURNS TABLE (
    id INT,
    user_created INT,
    date_created TIMESTAMP,
    user_updated INT,
    date_updated TIMESTAMP,
    version VARCHAR(255),
    terms VARCHAR(255),
    valid_till TIMESTAMP,
    quotation_for VARCHAR(255),
    status VARCHAR(255),
    lost_reason VARCHAR(255),
    customer_id INT,
    quotation_type VARCHAR(255),
    quotation_date TIMESTAMP,
    order_type VARCHAR(255),
    comments VARCHAR(255),
    delivery_within VARCHAR(255),
    delivery_after VARCHAR(255),
    is_active BOOLEAN,
    quotation_id VARCHAR(255),
    opportunity_id INT,
    lead_id INT,
    customer_name VARCHAR(255),
    taxes VARCHAR(255),
    delivery VARCHAR(255),
    payment VARCHAR(255),
    warranty VARCHAR(255),
    freight_charge VARCHAR(255),
    is_current BOOLEAN,
    parent_sales_quotations_id INT
) AS $$
BEGIN    
    RETURN QUERY 
    SELECT 
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
        sq.opportunity_id,
        sq.lead_id,
        sq.customer_name,
        sq.taxes,
        sq.delivery,
        sq.payment,
        sq.warranty,
        sq.freight_charge,
        sq.is_current,
        sq.parent_sales_quotations_id
    FROM public.sales_quotations sq 
    WHERE sq.is_active = true
    ORDER BY sq.date_created DESC;
END;
$$ LANGUAGE plpgsql;

-- Get quotation by ID
CREATE OR REPLACE FUNCTION get_quotation_by_id(p_id INT)
RETURNS TABLE (
    id INT,
    user_created INT,
    date_created TIMESTAMP,
    user_updated INT,
    date_updated TIMESTAMP,
    version VARCHAR(255),
    terms VARCHAR(255),
    valid_till TIMESTAMP,
    quotation_for VARCHAR(255),
    status VARCHAR(255),
    lost_reason VARCHAR(255),
    customer_id INT,
    quotation_type VARCHAR(255),
    quotation_date TIMESTAMP,
    order_type VARCHAR(255),
    comments VARCHAR(255),
    delivery_within VARCHAR(255),
    delivery_after VARCHAR(255),
    is_active BOOLEAN,
    quotation_id VARCHAR(255),
    opportunity_id INT,
    lead_id INT,
    customer_name VARCHAR(255),
    taxes VARCHAR(255),
    delivery VARCHAR(255),
    payment VARCHAR(255),
    warranty VARCHAR(255),
    freight_charge VARCHAR(255),
    is_current BOOLEAN,
    parent_sales_quotations_id INT
) AS $$
BEGIN    
    RETURN QUERY 
    SELECT 
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
        sq.opportunity_id,
        sq.lead_id,
        sq.customer_name,
        sq.taxes,
        sq.delivery,
        sq.payment,
        sq.warranty,
        sq.freight_charge,
        sq.is_current,
        sq.parent_sales_quotations_id
    FROM public.sales_quotations sq 
    WHERE sq.id = p_id AND sq.is_active = true;
END;
$$ LANGUAGE plpgsql;

-- Create new quotation
CREATE OR REPLACE FUNCTION create_quotation(
    p_user_created INT,
    p_version TEXT,
    p_terms TEXT,
    p_valid_till TIMESTAMP WITH TIME ZONE,
    p_quotation_for TEXT,
    p_status TEXT,
    p_lost_reason TEXT,
    p_customer_id INT,
    p_quotation_type TEXT,
    p_quotation_date TIMESTAMP WITH TIME ZONE,
    p_order_type TEXT,
    p_comments TEXT,
    p_delivery_within TEXT,
    p_delivery_after TEXT,
    p_quotation_id TEXT,
    p_opportunity_id INT,
    p_customer_name TEXT,
    p_taxes TEXT,
    p_delivery TEXT,
    p_payment TEXT,
    p_warranty TEXT,
    p_freight_charge TEXT,
    p_parent_sales_quotations_id INT
)
RETURNS INT AS $$
DECLARE
    new_id INT;
BEGIN
    INSERT INTO public.sales_quotations (
        user_created,
        date_created,
        version,
        terms,
        valid_till,
        quotation_for,
        status,
        lost_reason,
        customer_id,
        quotation_type,
        quotation_date,
        order_type,
        comments,
        delivery_within,
        delivery_after,
        is_active,
        quotation_id,
        opportunity_id,
        customer_name,
        taxes,
        delivery,
        payment,
        warranty,
        freight_charge,
        is_current,
        parent_sales_quotations_id
    )
    VALUES (
        p_user_created,
        CURRENT_TIMESTAMP,
        p_version,
        p_terms,
        p_valid_till,
        p_quotation_for,
        p_status,
        p_lost_reason,
        p_customer_id,
        p_quotation_type,
        p_quotation_date,
        p_order_type,
        p_comments,
        p_delivery_within,
        p_delivery_after,
        true,
        p_quotation_id,
        p_opportunity_id,
        p_customer_name,
        p_taxes,
        p_delivery,
        p_payment,
        p_warranty,
        p_freight_charge,
        true,
        p_parent_sales_quotations_id
    )
    RETURNING id INTO new_id;

    RETURN new_id;
END;
$$ LANGUAGE plpgsql;

-- Update quotation
CREATE OR REPLACE FUNCTION update_quotation(
    p_id INT,
    p_user_updated INT,
    p_version VARCHAR(255),
    p_terms VARCHAR(255),
    p_valid_till TIMESTAMP,
    p_quotation_for VARCHAR(255),
    p_status VARCHAR(255),
    p_lost_reason VARCHAR(255),
    p_customer_id INT,
    p_quotation_type VARCHAR(255),
    p_quotation_date TIMESTAMP,
    p_order_type VARCHAR(255),
    p_comments VARCHAR(255),
    p_delivery_within VARCHAR(255),
    p_delivery_after VARCHAR(255),
    p_quotation_id VARCHAR(255),
    p_opportunity_id INT,
    p_customer_name VARCHAR(255),
    p_taxes VARCHAR(255),
    p_delivery VARCHAR(255),
    p_payment VARCHAR(255),
    p_warranty VARCHAR(255),
    p_freight_charge VARCHAR(255),
    p_parent_sales_quotations_id INT
)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE public.sales_quotations
    SET 
        user_updated = p_user_updated,
        date_updated = CURRENT_TIMESTAMP,
        version = COALESCE(p_version, version),
        terms = COALESCE(p_terms, terms),
        valid_till = COALESCE(p_valid_till, valid_till),
        quotation_for = COALESCE(p_quotation_for, quotation_for),
        status = COALESCE(p_status, status),
        lost_reason = COALESCE(p_lost_reason, lost_reason),
        customer_id = COALESCE(p_customer_id, customer_id),
        quotation_type = COALESCE(p_quotation_type, quotation_type),
        quotation_date = COALESCE(p_quotation_date, quotation_date),
        order_type = COALESCE(p_order_type, order_type),
        comments = COALESCE(p_comments, comments),
        delivery_within = COALESCE(p_delivery_within, delivery_within),
        delivery_after = COALESCE(p_delivery_after, delivery_after),
        quotation_id = COALESCE(p_quotation_id, quotation_id),
        opportunity_id = COALESCE(p_opportunity_id, opportunity_id),
        customer_name = COALESCE(p_customer_name, customer_name),
        taxes = COALESCE(p_taxes, taxes),
        delivery = COALESCE(p_delivery, delivery),
        payment = COALESCE(p_payment, payment),
        warranty = COALESCE(p_warranty, warranty),
        freight_charge = COALESCE(p_freight_charge, freight_charge),
        parent_sales_quotations_id = COALESCE(p_parent_sales_quotations_id, parent_sales_quotations_id)
    WHERE id = p_id AND is_active = true;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- Soft delete quotation
CREATE OR REPLACE FUNCTION delete_quotation(
    p_id INT,
    p_user_updated INT
)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE public.sales_quotations
    SET 
        is_active = false,
        user_updated = p_user_updated,
        date_updated = CURRENT_TIMESTAMP
    WHERE id = p_id AND is_active = true;

    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

-- Get quotations by opportunity ID
CREATE OR REPLACE FUNCTION get_quotations_by_opportunity(p_opportunity_id INT)
RETURNS TABLE (
    id INT,
    user_created INT,
    date_created TIMESTAMP,
    user_updated INT,
    date_updated TIMESTAMP,
    version VARCHAR(255),
    terms VARCHAR(255),
    valid_till TIMESTAMP,
    quotation_for VARCHAR(255),
    status VARCHAR(255),
    lost_reason VARCHAR(255),
    customer_id INT,
    quotation_type VARCHAR(255),
    quotation_date TIMESTAMP,
    order_type VARCHAR(255),
    comments VARCHAR(255),
    delivery_within VARCHAR(255),
    delivery_after VARCHAR(255),
    is_active BOOLEAN,
    quotation_id VARCHAR(255),
    opportunity_id INT,
    lead_id INT,
    customer_name VARCHAR(255),
    taxes VARCHAR(255),
    delivery VARCHAR(255),
    payment VARCHAR(255),
    warranty VARCHAR(255),
    freight_charge VARCHAR(255),
    is_current BOOLEAN,
    parent_sales_quotations_id INT
) AS $$
BEGIN
    RETURN QUERY 
    SELECT * FROM public.sales_quotations 
    WHERE opportunity_id = p_opportunity_id 
    AND is_active = true
    ORDER BY date_created DESC;
END;
$$ LANGUAGE plpgsql;

-- Get quotations by customer ID
CREATE OR REPLACE FUNCTION get_quotations_by_customer(p_customer_id INT)
RETURNS TABLE (
    id INT,
    user_created INT,
    date_created TIMESTAMP,
    user_updated INT,
    date_updated TIMESTAMP,
    version VARCHAR(255),
    terms VARCHAR(255),
    valid_till TIMESTAMP,
    quotation_for VARCHAR(255),
    status VARCHAR(255),
    lost_reason VARCHAR(255),
    customer_id INT,
    quotation_type VARCHAR(255),
    quotation_date TIMESTAMP,
    order_type VARCHAR(255),
    comments VARCHAR(255),
    delivery_within VARCHAR(255),
    delivery_after VARCHAR(255),
    is_active BOOLEAN,
    quotation_id VARCHAR(255),
    opportunity_id INT,
    lead_id INT,
    customer_name VARCHAR(255),
    taxes VARCHAR(255),
    delivery VARCHAR(255),
    payment VARCHAR(255),
    warranty VARCHAR(255),
    freight_charge VARCHAR(255),
    is_current BOOLEAN,
    parent_sales_quotations_id INT
) AS $$
BEGIN
    RETURN QUERY 
    SELECT * FROM public.sales_quotations 
    WHERE customer_id = p_customer_id 
    AND is_active = true
    ORDER BY date_created DESC;
END;
$$ LANGUAGE plpgsql;
