-- Function: fn_getopportunitiesbyleadid
-- Returns all opportunities for a given sales lead (by integer PK id)

CREATE OR REPLACE FUNCTION fn_getopportunitiesbyleadid(p_lead_id INT)
RETURNS TABLE (
    id INT,
    user_created INT,
    date_created TIMESTAMP,
    user_updated INT,
    date_updated TIMESTAMP,
    status VARCHAR,
    expected_completion DATE,
    opportunity_type VARCHAR,
    opportunity_for VARCHAR,
    customer_id VARCHAR,
    customer_name VARCHAR,
    customer_type VARCHAR,
    opportunity_name VARCHAR,
    opportunity_id VARCHAR,
    comments TEXT,
    isactive BOOLEAN,
    lead_id VARCHAR,
    sales_representative_id INT,
    contact_name VARCHAR,
    contact_mobile_no VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        o.id,
        o.user_created,
        o.date_created,
        o.user_updated,
        o.date_updated,
        o.status,
        o.expected_completion,
        o.opportunity_type,
        o.opportunity_for,
        o.customer_id,
        o.customer_name,
        o.customer_type,
        o.opportunity_name,
        o.opportunity_id,
        o.comments,
        o.isactive,
        o.lead_id,
        o.sales_representative_id,
        o.contact_name,
        o.contact_mobile_no
    FROM public.sales_opportunities o
    WHERE o.lead_id = p_lead_id::text;
END;
$$ LANGUAGE plpgsql;
