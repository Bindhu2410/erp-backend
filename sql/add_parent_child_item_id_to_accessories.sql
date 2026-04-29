ALTER TABLE sales_product_accessories
ADD COLUMN IF NOT EXISTS parent_child_item_id INTEGER NULL;
