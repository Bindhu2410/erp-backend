CREATE TABLE public.uom (
    id SERIAL PRIMARY KEY,
    user_created INT REFERENCES public.users(userid),
    date_created TIMESTAMP DEFAULT now(),
    user_updated INT REFERENCES public.users(userid),
    date_updated TIMESTAMP,
    code VARCHAR(255) NOT NULL,
    description VARCHAR(255) 
);