-- Table: public.sales_product_child_items

CREATE TABLE public.sales_product_child_items (
    id serial PRIMARY KEY,
    sales_product_id int4 NOT NULL,
    child_item_id int4 NOT NULL,
    quantity int4 NOT NULL DEFAULT 1,
    isactive bool NOT NULL DEFAULT true,
    date_created timestamp DEFAULT CURRENT_TIMESTAMP,
    user_created int4 NULL,
    date_updated timestamp NULL,
    user_updated int4 NULL
    -- Add more columns as needed
);

-- Foreign keys (assuming sales_products and item_master tables exist)
ALTER TABLE public.sales_product_child_items
    ADD CONSTRAINT fk_sales_product FOREIGN KEY (sales_product_id) REFERENCES public.sales_product(id),
    ADD CONSTRAINT fk_child_item FOREIGN KEY (child_item_id) REFERENCES public.item_master(id);
