DROP TABLE IF EXISTS public.sales_temp_lead CASCADE;

CREATE TABLE public.sales_temp_lead (
    id SERIAL PRIMARY KEY,
    user_created INT4 NULL,
    date_created TIMESTAMP NULL,
    user_updated INT4 NULL,
    date_updated TIMESTAMP NULL,
    customer_name VARCHAR(255) NULL,
    lead_source VARCHAR(255) NULL,
    lead_id VARCHAR(255) NULL,
    status VARCHAR(255) NULL,
    score VARCHAR(255) NULL,
    isactive BOOLEAN DEFAULT false NULL,
    comments TEXT NULL,
    lead_type VARCHAR(255) NULL,
    contact_name VARCHAR(100) NULL,
    salutation VARCHAR(10) NULL,
    contact_mobile_no VARCHAR(20) NULL,
    land_line_no VARCHAR(15) NULL,
    email VARCHAR(100) NULL,
    door_no VARCHAR(20) NULL,
    street VARCHAR(50) NULL,
    landmark VARCHAR(50) NULL,
    website VARCHAR(100) NULL,
    area VARCHAR(255) NULL,
    city VARCHAR(255) NULL,
    pincode VARCHAR(255) NULL,
    district VARCHAR(255) NULL,
    state VARCHAR(255) NULL,
    country VARCHAR(255) NULL
);

ALTER TABLE public.sales_temp_lead 
    ADD CONSTRAINT fk_sales_temp_lead_user_created 
    FOREIGN KEY (user_created) REFERENCES public.users(userid);

ALTER TABLE public.sales_temp_lead 
    ADD CONSTRAINT fk_sales_temp_lead_user_updated 
    FOREIGN KEY (user_updated) REFERENCES public.users(userid);
