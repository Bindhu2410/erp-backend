CREATE OR REPLACE PROCEDURE sp_update_sales_product_status(
    p_id INTEGER,
    p_isactive BOOLEAN,
    p_user_updated INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE sales_products 
    SET 
        isactive = p_isactive,
        user_updated = p_user_updated,
        date_updated = CURRENT_TIMESTAMP
    WHERE id = p_id;
END;
$$;
