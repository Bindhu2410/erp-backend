CREATE TABLE public.sales_addresses (
	id serial4 NOT NULL,
	user_created int4 NULL,
	date_created timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	contact_name varchar(50) NULL,
	"type" varchar(50) NULL,
	city varchar(10) NULL,
	state varchar(30) NULL,
	pincode int NULL,
	isactive bool DEFAULT false NOT NULL,
	block varchar(10) NULL,
	department varchar(100) NULL,
	area varchar(100) NULL,
	opportunity_id varchar(100) NULL,
	door_no varchar(10) NULL,
	street varchar(100) NULL,
	land_mark varchar(100) NULL,
	is_default bool DEFAULT false NULL,
	sales_lead_id integer NULL,
	CONSTRAINT pk_sales_addresses PRIMARY KEY (id)
);


-- public.sales_addresses foreign keys

ALTER TABLE public.sales_addresses ADD CONSTRAINT fk_sales_addresses_user_created FOREIGN KEY (user_created) REFERENCES public.users(user_id);
ALTER TABLE public.sales_addresses ADD CONSTRAINT fk_sales_addresses_user_updated FOREIGN KEY (user_updated) REFERENCES public.users(user_id);
ALTER TABLE public.sales_addresses ADD CONSTRAINT fk_sales_addresses_sales_lead FOREIGN KEY (sales_lead_id) REFERENCES public.sales_lead(id);