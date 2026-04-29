-- public.sales_demos definition

-- Drop table

-- DROP TABLE public.sales_demos;

CREATE TABLE public.sales_demos (
	id serial4 NOT NULL,
	user_created int4 NULL,
	date_created timestamp DEFAULT now() NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	user_id int4 NULL,
	demo_date timestamp NULL,
	status varchar(100) NULL,
	opportunity_id varchar(255) NULL,
	customer_id int4 NULL,
	demo_contact varchar(255) NULL,
	customer_name varchar(255) NULL,
	demo_approach varchar(255) NULL,
	demo_outcome varchar(255) NULL,
	demo_feedback varchar(255) NULL,
	"comments" varchar(255) NULL,
	leadid text NULL,
	contact_mobile_num varchar(20) NULL,
	address varchar(100) NULL,
	presenter_ids _int4 NULL,
	demo_time time NULL,
	demo_name varchar(100) NULL,
	isactive boolean DEFAULT true NULL,
	CONSTRAINT sales_demos_pkey PRIMARY KEY (id)
);


-- public.sales_demos foreign keys

ALTER TABLE public.sales_demos ADD CONSTRAINT fk_demo_customer FOREIGN KEY (customer_id) REFERENCES public.sales_customers(id);
ALTER TABLE public.sales_demos ADD CONSTRAINT fk_demo_user_created FOREIGN KEY (user_created) REFERENCES public.users(userid);
ALTER TABLE public.sales_demos ADD CONSTRAINT fk_demo_user_id FOREIGN KEY (user_id) REFERENCES public.users(userid);
ALTER TABLE public.sales_demos ADD CONSTRAINT fk_demo_user_updated FOREIGN KEY (user_updated) REFERENCES public.users(userid);