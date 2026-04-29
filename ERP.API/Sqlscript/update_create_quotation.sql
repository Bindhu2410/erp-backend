-- Update or create the function with correct parameters
DROP FUNCTION IF EXISTS create_quotation;

CREATE OR REPLACE FUNCTION create_quotation(
    p_user_created INT,
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
RETURNS INT AS $$
DECLARE
    new_id INT;
BEGIN
    INSERT INTO sales_quotations (
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
