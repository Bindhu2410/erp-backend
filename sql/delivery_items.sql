DROP TABLE IF EXISTS public.delivery_items CASCADE;

CREATE TABLE public.delivery_items (
    item_id SERIAL PRIMARY KEY,
    delivery_id VARCHAR(50) NOT NULL REFERENCES public.deliveries(delivery_id) ON DELETE CASCADE,
    product_id INT NOT NULL REFERENCES item_master(id),
    user_created INT REFERENCES public.users(user_id),
    date_created TIMESTAMP DEFAULT now(),
    user_updated INT REFERENCES public.users(user_id),
    date_updated TIMESTAMP,
    qty INT NOT NULL,
    amount NUMERIC(18,2) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    unit_price NUMERIC(18,2) NOT NULL,
    included_child_item_ids INT[],
    accessories_ids INT[]
);
