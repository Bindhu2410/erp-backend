CREATE TABLE public.item_stock (
    id SERIAL PRIMARY KEY,
    item_id INT NOT NULL REFERENCES public.item_master(id) ON DELETE CASCADE,
    warehouse_id INT NOT NULL REFERENCES public.warehouse(id) ON DELETE CASCADE,
    location_id INT REFERENCES public.item_location(id) ON DELETE SET NULL,
    quantity_on_hand NUMERIC(12,2) DEFAULT 0,   -- Current stock qty
    allocated_qty NUMERIC(12,2) DEFAULT 0,
    stock_value NUMERIC(14,2) DEFAULT 0,        -- Current stock value
    reorder_qty NUMERIC(12,2) DEFAULT 0,        -- Minimum threshold
    last_updated TIMESTAMP DEFAULT now(),
    user_created INT REFERENCES public.users(userid),
    date_created TIMESTAMP DEFAULT now(),
    user_updated INT REFERENCES public.users(userid),
    date_updated TIMESTAMP,
    CONSTRAINT item_stock_balance_unique UNIQUE (item_id, warehouse_id, location_id)
);
