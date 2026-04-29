-- Stored Procedure: sp_get_quotation_info_by_po_id
-- Returns the full sales_quotations row for a given po_id

CREATE OR REPLACE FUNCTION sp_get_quotation_info_by_po_id(p_po_id TEXT)
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
    is_active BOOL,
    quotation_id VARCHAR(255),
    customer_name VARCHAR(255),
    taxes VARCHAR(255),
    delivery VARCHAR(255),
    payment VARCHAR(255),
    warranty VARCHAR(255),
    freight_charge VARCHAR(255),
    is_current BOOL,
    parent_sales_quotations_id INT,
    lead_id TEXT,
    opportunity_id TEXT,
    tax INT,
    discount INT,
    freight_charges INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        sq.id,
        sq.user_created,
        sq.date_created,
        sq.user_updated,
        sq.date_updated,
        sq."version",
        sq.terms,
        sq.valid_till,
        sq.quotation_for,
        sq.status,
        sq.lost_reason,
        sq.customer_id,
        sq.quotation_type,
        sq.quotation_date,
        sq.order_type,
        sq."comments",
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
        sq.freight_charges
    FROM purchase_order po
    JOIN sales_quotations sq ON po.quotation_id = sq.id
    WHERE po.po_id = p_po_id
    LIMIT 1;
END;
$$ LANGUAGE plpgsql;

-- Test Script
-- Replace 'PO123' with a real po_id from your data
SELECT * FROM sp_get_quotation_info_by_po_id('PO-2025-03');
