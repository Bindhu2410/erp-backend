CREATE TABLE public.quotation_products (
    id SERIAL PRIMARY KEY NOT NULL,
    user_created INTEGER,
    date_created TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    quotation_id INTEGER NOT NULL,
    inventory_items_id INTEGER NOT NULL,
    qty INTEGER NOT NULL,
    amount FLOAT,
    isactive BOOLEAN DEFAULT TRUE,
    CONSTRAINT fk_quotation FOREIGN KEY (quotation_id) REFERENCES sales_quotations(id) ON DELETE CASCADE,
    CONSTRAINT fk_inventory_item FOREIGN KEY (inventory_items_id) REFERENCES inventory_items(id),
    CONSTRAINT fk_user_created FOREIGN KEY (user_created) REFERENCES sales_employees(id),
    CONSTRAINT fk_user_updated FOREIGN KEY (user_updated) REFERENCES sales_employees(id)
);
