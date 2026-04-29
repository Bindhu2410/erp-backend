CREATE TABLE public.bom_name (
    id SERIAL PRIMARY KEY,
    user_created INT REFERENCES public.users(userid),
    date_created TIMESTAMP DEFAULT now(),
    user_updated INT REFERENCES public.users(userid),
    date_updated TIMESTAMP,
    name VARCHAR(255) NOT NULL,
    type VARCHAR(255)[]  
);