-- public.sales_orders definition

-- Drop table

-- DROP TABLE public.sales_orders;

CREATE TABLE public.sales_orders (
	id serial4 NOT NULL,
	order_id varchar(50) NOT NULL,
	customer_id int4 NULL,
	order_date date NOT NULL,
	expected_delivery_date date NULL,
	status varchar(50) NOT NULL,
	quotation_id int4 NULL,
	po_id varchar(50) NULL,
	acceptance_date date NULL,
	total_amount numeric(12, 2) DEFAULT 0.00 NULL,
	tax_amount numeric(12, 2) DEFAULT 0.00 NULL,
	grand_total numeric(12, 2) DEFAULT 0.00 NULL,
	notes text NULL,
	user_created int4 NULL,
	date_created timestamp NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	freight_charge numeric NULL,
	CONSTRAINT sales_orders_order_id_key UNIQUE (order_id),
	CONSTRAINT sales_orders_pkey PRIMARY KEY (id),
	CONSTRAINT uq_sales_order_id UNIQUE (order_id)
);


-- public.sales_orders foreign keys

ALTER TABLE public.sales_orders ADD CONSTRAINT sales_orders_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.sales_customers(id);
ALTER TABLE public.sales_orders ADD CONSTRAINT sales_orders_quotation_id_fkey FOREIGN KEY (quotation_id) REFERENCES public.sales_quotations(id) ON DELETE CASCADE;