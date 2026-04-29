-- public.sales_contacts definition

-- Drop table

-- DROP TABLE public.sales_contacts;

CREATE TABLE public.sales_contacts (
	id serial4 NOT NULL,
	user_created int4 NOT NULL,
	date_created timestamp DEFAULT CURRENT_TIMESTAMP NOT NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	contact_name text NOT NULL,
	department_name text NULL,
	specialist text NULL,
	"degree" text NULL,
	email text NULL,
	mobile_no text NULL,
	website text NULL,
	isactive bool DEFAULT true NOT NULL,
	own_clinic bool NULL,
	visiting_hours text NULL,
	clinic_visiting_hours text NULL,
	sales_lead_id_custom text NULL,
	land_line_no text NULL,
	fax text NULL,
	salutation text NULL,
	job_title text NULL,
	is_default bool DEFAULT false NULL,
	sales_lead_id int4 NULL,
	CONSTRAINT pk_sales_contacts PRIMARY KEY (id)
);


-- public.sales_contacts foreign keys

ALTER TABLE public.sales_contacts ADD CONSTRAINT fk_sales_contacts_user_created FOREIGN KEY (user_created) REFERENCES public.users(user_id);
ALTER TABLE public.sales_contacts ADD CONSTRAINT fk_sales_contacts_user_updated FOREIGN KEY (user_updated) REFERENCES public.users(user_id);