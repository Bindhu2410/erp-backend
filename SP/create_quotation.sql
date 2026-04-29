-- Create or replace the stored procedure for creating quotations
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
    p_is_active BOOLEAN,
    p_quotation_id VARCHAR(255),
    p_opportunity_id INT,
    p_lead_id INT,
    p_customer_name VARCHAR(255),
    p_taxes VARCHAR(255),
    p_delivery VARCHAR(255),
    p_payment VARCHAR(255),
    p_warranty VARCHAR(255),
    p_freight_charge VARCHAR(255),
    p_is_current BOOLEAN,
    p_parent_sales_quotations_id INT DEFAULT NULL
)
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
    -- Handle parent quotation validation
    IF p_parent_sales_quotations_id IS NOT NULL THEN
        -- Log the parent ID we're checking for debugging
        RAISE NOTICE 'Checking for parent quotation with ID: %', p_parent_sales_quotations_id;
        
        -- First check if it's 0 or negative
        IF p_parent_sales_quotations_id <= 0 THEN
            p_parent_sales_quotations_id := NULL;
            RAISE NOTICE 'Parent ID was <= 0, setting to NULL';
        ELSE
            -- Check if the parent quotation exists
            IF NOT EXISTS (SELECT 1 FROM public.sales_quotations WHERE id = p_parent_sales_quotations_id) THEN
                RAISE EXCEPTION 'Parent quotation with ID % does not exist', p_parent_sales_quotations_id
                    USING HINT = 'Please provide a valid parent quotation ID or NULL';
            ELSE
                RAISE NOTICE 'Found parent quotation with ID: %', p_parent_sales_quotations_id;
            END IF;
        END IF;
    ELSE
        RAISE NOTICE 'No parent quotation ID provided (NULL)';
    END IF;

    BEGIN
        RAISE NOTICE 'Attempting to insert new quotation with parent_id: %', p_parent_sales_quotations_id;
        
        RETURN QUERY 
        INSERT INTO public.sales_quotations (
            user_created, date_created, version, terms, valid_till, quotation_for,
            status, lost_reason, customer_id, quotation_type, quotation_date,
            order_type, comments, delivery_within, delivery_after, is_active,
            quotation_id, opportunity_id, lead_id, customer_name, taxes,
            delivery, payment, warranty, freight_charge, is_current,
            parent_sales_quotations_id
        )
        VALUES (
            p_user_created, CURRENT_TIMESTAMP, p_version, p_terms, p_valid_till, p_quotation_for,
            p_status, p_lost_reason, p_customer_id, p_quotation_type, p_quotation_date,
            p_order_type, p_comments, p_delivery_within, p_delivery_after, p_is_active,
            p_quotation_id, p_opportunity_id, p_lead_id, p_customer_name, p_taxes,
            p_delivery, p_payment, p_warranty, p_freight_charge, p_is_current,
            p_parent_sales_quotations_id
        )
        RETURNING *;    EXCEPTION
        WHEN foreign_key_violation THEN
            RAISE EXCEPTION 'Foreign key violation - Please check that parent_id (%), customer_id (%), opportunity_id (%), and lead_id (%) exist',
                p_parent_sales_quotations_id, p_customer_id, p_opportunity_id, p_lead_id
                USING HINT = 'Make sure all referenced IDs exist in their respective tables';
        WHEN OTHERS THEN
            RAISE EXCEPTION 'An error occurred while creating quotation: %', SQLERRM
                USING HINT = 'Check all input parameters and constraints';
    END;
END;
$$ LANGUAGE plpgsql;
