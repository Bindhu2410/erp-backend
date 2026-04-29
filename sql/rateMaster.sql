CREATE TABLE public.rate_master (
	id serial4 NOT NULL,
	user_created int4 NULL,
	date_created timestamp NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	rate_master_id varchar(100),
	item_id int references item_master(id),
	supplier_id int references suppliers(id),
	purchase_rate decimal(18,2),
	sale_rate decimal(18,2),
	quote_rate decimal(18,2),
	hsn_code int,
	tax_percentage int,
	description varchar(500) NULL,
	effective_date date NULL
);