-- public.sales_lead definition

-- Drop table

-- DROP TABLE public.sales_lead;

CREATE TABLE public.sales_lead (
	id serial4 NOT NULL,
	user_created int4 NULL,
	date_created timestamp NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	customer_name varchar(255) NULL,
	lead_source varchar(255) NULL,
	referral_source_name varchar(255) NULL,
	hospital_of_referral varchar(255) NULL,
	department_of_referral varchar(255) NULL,
	social_media varchar(255) NULL,
	event_date date NULL,
	event_name varchar(255) NULL,
	lead_id varchar(255) NULL,
	status varchar(255) NULL,
	score varchar(255) NULL,
	isactive bool DEFAULT false NULL,
	"comments" text NULL,
	lead_type varchar(255) NULL,
	contact_name varchar(100) NULL,
	salutation varchar(10) NULL,
	contact_mobile_no varchar(20) NULL,
	land_line_no varchar(15) NULL,
	email varchar(100) NULL,
	fax varchar(15) NULL,
	door_no varchar(5) NULL,
	street varchar(50) NULL,
	landmark varchar(50) NULL,
	website varchar(100) NULL,
	geo_divisions_id int4 NULL,
	territory varchar(255) NULL,
	area_id int4 NULL,
	area varchar(255) NULL,
	city varchar(255) NULL,
	pincode_id int4 NULL,
	pincode varchar(255) NULL,
	district varchar(255) NULL,
	state varchar(255) NULL,
	country varchar(255) NULL,
	converted_customer_id varchar(255) NULL,
	CONSTRAINT pk_sales_lead PRIMARY KEY (id)
);


-- public.sales_lead foreign keys

ALTER TABLE public.sales_lead ADD CONSTRAINT fk_sales_lead_user_created FOREIGN KEY (user_created) REFERENCES public.users(userid);
ALTER TABLE public.sales_lead ADD CONSTRAINT fk_sales_lead_user_updated FOREIGN KEY (user_updated) REFERENCES public.users(userid);


ALTER TABLE public.sales_lead
    ADD COLUMN assigned_to INT;

-- Add foreign key constraint
ALTER TABLE public.sales_lead
    ADD CONSTRAINT fk_sales_lead_assigned_to
    FOREIGN KEY (assigned_to) REFERENCES public.users(userid);
