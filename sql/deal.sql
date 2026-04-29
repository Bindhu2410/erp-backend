CREATE TABLE public.sales_deals (
    id serial4 NOT NULL,
    user_created int4 NULL,
    date_created timestamp NULL,
    user_updated int4 NULL,
    date_updated timestamp NULL,
    status varchar(255) NULL,
    deal_name varchar(255) NULL,
    amount float8 NULL,
    expected_revenue float8 NULL,
    start_date date,
    deal_for varchar(255) NULL,
    close_date date NULL,
    isactive bool DEFAULT false NULL,
    comments text NULL,
    opportunity_id int4 NULL,
    customer_id int,
    sales_representative_id int4 NULL
);

-- Add constraints (with unique names)
ALTER TABLE public.sales_deals 
    ADD CONSTRAINT fk_sales_deal_sales_rep FOREIGN KEY (sales_representative_id) REFERENCES public.sales_employees(id),
    ADD CONSTRAINT fk_sales_deal_opportunity FOREIGN KEY (opportunity_id) REFERENCES public.sales_opportunities(id),
    ADD CONSTRAINT fk_sales_deal_customer FOREIGN KEY (customer_id) REFERENCES public.sales_customers(id),
    ADD CONSTRAINT fk_sales_deal_user_created FOREIGN KEY (user_created) REFERENCES public.sales_employees(id),
    ADD CONSTRAINT fk_sales_deal_user_updated FOREIGN KEY (user_updated) REFERENCES public.sales_employees(id);
