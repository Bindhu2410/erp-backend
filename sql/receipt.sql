CREATE TABLE public.receipt (
    id SERIAL PRIMARY KEY,

    -- Audit fields
    user_created INT,
    date_created TIMESTAMP DEFAULT now(),
    user_updated INT,
    date_updated TIMESTAMP,

    -- References
    location_id VARCHAR(250),
    bom_id VARCHAR(255),

    -- Business fields
    received_from VARCHAR(250),
    customer_name VARCHAR(250) NOT NULL,
    sales_representative VARCHAR(250) NOT NULL,
    doc_id VARCHAR(100) UNIQUE,
    receipt_date DATE,
    comments VARCHAR(250),

    -- Checks
    CONSTRAINT chk_customer_name CHECK (char_length(customer_name) > 0),
    CONSTRAINT chk_sales_representative CHECK (char_length(sales_representative) > 0)
);

-- Foreign keys
ALTER TABLE public.receipt
    ADD CONSTRAINT receipt_bom_id_fkey
    FOREIGN KEY (bom_id) REFERENCES public.bill_of_materials(bom_id) ON DELETE SET NULL;

ALTER TABLE public.receipt
    ADD CONSTRAINT receipt_user_created_fkey
    FOREIGN KEY (user_created) REFERENCES public.users(userid) ON DELETE SET NULL;

ALTER TABLE public.receipt
    ADD CONSTRAINT receipt_user_updated_fkey
    FOREIGN KEY (user_updated) REFERENCES public.users(userid) ON DELETE SET NULL;

alter table receipt 
add column issue_id varchar(250);