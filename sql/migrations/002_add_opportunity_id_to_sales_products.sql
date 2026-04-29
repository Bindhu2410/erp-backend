-- Add opportunity_id column to sales_products table
ALTER TABLE public.sales_products
ADD COLUMN opportunity_id INTEGER;

-- Add foreign key constraint
ALTER TABLE public.sales_products
ADD CONSTRAINT fk_sales_products_opportunity FOREIGN KEY (opportunity_id)
    REFERENCES public.sales_opportunities (id);

-- Migrate existing data from stage_item_id to opportunity_id where possible
UPDATE sales_products sp
SET opportunity_id = CAST(stage_item_id AS INTEGER)
WHERE stage_item_id SIMILAR TO '[0-9]+'
AND stage_item_id IS NOT NULL;

-- Update stored procedure to use opportunity_id 
DROP FUNCTION IF EXISTS get_sales_products_by_opportunity;
CREATE OR REPLACE FUNCTION get_sales_products_by_opportunity(p_opportunity_id INTEGER)
RETURNS TABLE (
    id INTEGER,
    userCreated INTEGER,
    dateCreated TIMESTAMP,
    userUpdated INTEGER,
    dateUpdated TIMESTAMP,
    makeId INTEGER,
    makeName VARCHAR(255),
    modelId INTEGER,
    modelName VARCHAR(255),
    productId INTEGER,
    productName VARCHAR(255),
    categoryId INTEGER,
    categoryName VARCHAR(255),
    itemCode VARCHAR(255),
    itemName VARCHAR(255),
    qty INTEGER,
    amount NUMERIC(18,2),
    stage VARCHAR(255),
    stageItemId BIGINT,
    inventoryItemsId BIGINT,
    isactive BOOLEAN
) AS $$
BEGIN
    RETURN QUERY 
    SELECT
        sp.id,
        COALESCE(sp.user_created, 0) as userCreated,
        sp.date_created as dateCreated,
        COALESCE(sp.user_updated, 0) as userUpdated,
        sp.date_updated as dateUpdated,
        COALESCE(m.id, 0) as makeId,
        COALESCE(m.name, '') as makeName,
        COALESCE(md.id, 0) as modelId,
        COALESCE(md.name, '') as modelName,
        COALESCE(p.id, 0) as productId,
        COALESCE(p.name, '') as productName,
        COALESCE(c.id, 0) as categoryId,
        COALESCE(c.name, '') as categoryName,
        COALESCE(i.item_code, '') as itemCode,
        COALESCE(i.item_name, '') as itemName,
        COALESCE(sp.qty, 0) as qty,
        CAST(COALESCE(sp.amount, 0) AS NUMERIC(18,2)) as amount,
        COALESCE(sp.stage, '') as stage,
        sp.stage_item_id as stageItemId,
        sp.inventory_items_id as inventoryItemsId,
        sp.isactive
    FROM sales_products sp
    LEFT JOIN inventory_items i ON sp.inventory_items_id = i.id
    LEFT JOIN makes m ON i.make_id = m.id
    LEFT JOIN models md ON i.model_id = md.id
    LEFT JOIN products p ON i.product_id = p.id
    LEFT JOIN categories c ON i.category_id = c.id
    WHERE sp.opportunity_id = p_opportunity_id
    AND sp.isactive = true 
    AND (i.isactive IS NULL OR i.isactive = true)
    ORDER BY sp.id;
END;
$$ LANGUAGE plpgsql;
