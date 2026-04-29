CREATE TABLE public.warehouse (
    id SERIAL PRIMARY KEY,
    warehouse_name VARCHAR(255) NOT NULL,        
    warehouse_type VARCHAR(50),                  
    address VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(100),
    country VARCHAR(100),
    pincode VARCHAR(20),
    contact_person VARCHAR(100),
    phone VARCHAR(50),
    email VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    parent_warehouse_id INT REFERENCES public.warehouse(id) -- for hierarchy (e.g. sub-store under main store),
    user_created INT REFERENCES public.users(userid),
    date_created TIMESTAMP DEFAULT now(),
    user_updated INT REFERENCES public.users(userid),
    date_updated TIMESTAMP
);
