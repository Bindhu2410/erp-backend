CREATE TABLE public.inventory_method (
    id SERIAL PRIMARY KEY,
    user_created INT REFERENCES public.users(userid),
    date_created TIMESTAMP DEFAULT now(),
    user_updated INT REFERENCES public.users(userid),
    date_updated TIMESTAMP,
    name VARCHAR(255) NOT NULL
);