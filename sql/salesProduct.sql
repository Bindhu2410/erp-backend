-- public.sales_product definition

-- Drop table

-- DROP TABLE public.sales_product;

CREATE TABLE public.sales_product (
	id serial4 NOT NULL,
	user_created int4 NULL,
	date_created timestamp NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	qty int4 NULL,
	amount float8 NULL,
	is_active bool DEFAULT true NULL,
	item_id int4 NULL,
	stage varchar(255) NULL,
	unit_price numeric(12, 2) NULL,
	stage_item_id varchar(50) NULL,
	parent_id int4 NULL,
	bom_id varchar(255) NULL,
	bom_child_item_ids jsonb NULL,
	bom_accessory_item_ids jsonb NULL,
	CONSTRAINT sales_product_pkey PRIMARY KEY (id)
);