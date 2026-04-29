-- public.order_acceptance definition

-- Drop table

-- DROP TABLE public.order_acceptance;

CREATE TABLE public.order_acceptance (
	id serial4 NOT NULL,
	order_acceptance_id varchar(50) NULL,
	user_created int4 NULL,
	date_created timestamp NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	subject varchar(255) NULL,
	purchase_order_id varchar(50) NULL,
	"comments" text NULL,
	fileurl varchar(255) NULL,
	filename varchar(255) NULL,
	quotation_id int4 NULL,
	sales_order_id varchar NULL,
	CONSTRAINT order_acceptance_pkey PRIMARY KEY (id)
);


-- public.order_acceptance foreign keys

ALTER TABLE public.order_acceptance ADD CONSTRAINT fk_order_acceptance_quotation FOREIGN KEY (quotation_id) REFERENCES public.sales_quotations(id);
ALTER TABLE public.order_acceptance ADD CONSTRAINT fk_order_acceptance_sales_order FOREIGN KEY (sales_order_id) REFERENCES public.sales_orders(order_id);