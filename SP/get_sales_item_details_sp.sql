-- =============================================
-- Author:    Auto-generated
-- Create date: 2025-06-25
-- Description: Get sales product and item details by item_id
-- =============================================
CREATE OR REPLACE FUNCTION get_sales_item_details(p_item_id INT)
RETURNS TABLE (
    -- sales_product fields
    id INT,
    user_created INT,
    date_created TIMESTAMP,
    user_updated INT,
    date_updated TIMESTAMP,
    qty INT,
    amount FLOAT8,
    is_active BOOL,
    item_id INT,
    stage VARCHAR(255),
    unit_price NUMERIC(12,2),
    stage_item_id INT,
    -- item_master fields
    make VARCHAR(255),
    model VARCHAR(255),
    category VARCHAR(255),
    product VARCHAR(255),
    item_name VARCHAR(255),
    item_code VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        sp.id, sp.user_created, sp.date_created, sp.user_updated, sp.date_updated,
        sp.qty, sp.amount, sp.is_active, sp.item_id, sp.stage, sp.unit_price, sp.stage_item_id,
        im.make, im.model, im.category, im.product, im.item_name, im.item_code
    FROM sales_product sp
    JOIN item_master im ON sp.item_id = im.id
    WHERE sp.item_id = p_item_id;
END;
$$ LANGUAGE plpgsql;
