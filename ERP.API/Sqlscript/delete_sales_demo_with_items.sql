-- =============================================
-- Author:    AI Generated
-- Create date: 2025-07-08
-- Description: Deletes a sales demo and its related items by demo id
-- =============================================
CREATE OR REPLACE PROCEDURE delete_sales_demo_with_items(IN p_id integer)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Delete related items
    DELETE FROM sales_demo_items WHERE demo_id = p_id;
    DELETE FROM sales_demo_presenters WHERE demo_id = p_id;
    -- Delete the sales demo itself
    DELETE FROM sales_demos WHERE id = p_id;
END;
$$;
