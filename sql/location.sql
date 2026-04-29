CREATE TABLE public.item_location (
    id SERIAL PRIMARY KEY,
    item_id INT NOT NULL REFERENCES public.item_master(id) ON DELETE CASCADE,
    warehouse_id INT NOT NULL REFERENCES public.warehouse(id) ON DELETE CASCADE,
    rack VARCHAR(50),
    shelf VARCHAR(50),
    column_no VARCHAR(50),
    in_place VARCHAR(100),
    user_created INT REFERENCES public.users(userid),
    date_created TIMESTAMP DEFAULT now(),
    user_updated INT REFERENCES public.users(userid),
    date_updated TIMESTAMP,
       CONSTRAINT item_location_unique UNIQUE (item_id, warehouse_id, rack, shelf, column_no)
);
