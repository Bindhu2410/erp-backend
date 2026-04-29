-- SQL script to create the sales_demo_items table in PostgreSQL
CREATE TABLE public.sales_demo_items (
    id serial PRIMARY KEY,
    demo_id integer NOT NULL REFERENCES public.sales_demos(id),
    user_created integer,
    date_created timestamp,
    user_updated integer,
    date_updated timestamp,
    qty integer,
    amount decimal(18,2),
    is_active boolean DEFAULT true,
    item_id integer,
    unit_price decimal(18,2)
);
