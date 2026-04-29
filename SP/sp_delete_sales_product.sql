-- Create stored procedure for deleting a sales product (soft delete)
CREATE OR REPLACE PROCEDURE sp_delete_sales_product(
    IN p_id INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Perform soft delete by setting isactive to false
    UPDATE sales_products SET
        isactive = false,
        date_updated = CURRENT_TIMESTAMP
    WHERE id = p_id;
END;
$$;
