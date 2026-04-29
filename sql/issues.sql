-- public.issues definition

-- Drop table

-- DROP TABLE public.issues;

CREATE TABLE public.issues (
	id serial4 NOT NULL,
	user_created int4 NULL,
	date_created timestamp DEFAULT now() NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	location_id varchar(250) NULL,
	bom_id varchar(255) NULL,
	iss_to varchar(100) NULL,
	issue_to varchar(250) NULL,
	customer_name varchar(250) NULL,
	sales_representative varchar(250) NULL,
	demo_from varchar(250) NULL,
	demo_report varchar(100) NULL,
	doc_id varchar(100) NULL,
	issue_date date NULL,
	booking_address varchar(250) NULL,
	booking_qty int4 NULL,
	"comments" varchar(250) NULL,
	narration varchar(250) NULL,
	CONSTRAINT chk_customer_name CHECK ((char_length((customer_name)::text) > 0)),
	CONSTRAINT chk_sales_representative CHECK ((char_length((sales_representative)::text) > 0)),
	CONSTRAINT issues_booking_qty_check CHECK ((booking_qty >= 0)),
	CONSTRAINT issues_pkey PRIMARY KEY (id),
	CONSTRAINT uq_doc_id UNIQUE (doc_id)
);


-- public.issues foreign keys

ALTER TABLE public.issues ADD CONSTRAINT issues_bom_id_fkey FOREIGN KEY (bom_id) REFERENCES <?>() ON DELETE SET NULL;
ALTER TABLE public.issues ADD CONSTRAINT issues_user_created_fkey FOREIGN KEY (user_created) REFERENCES public.users(userid) ON DELETE SET NULL;
ALTER TABLE public.issues ADD CONSTRAINT issues_user_updated_fkey FOREIGN KEY (user_updated) REFERENCES public.users(userid) ON DELETE SET NULL;

ALTER TABLE public.issues DROP CONSTRAINT issues_bom_id_fkey;

ALTER TABLE public.issues
ALTER COLUMN bom_id TYPE varchar(255)[]
USING ARRAY[bom_id];

alter table issues 
add column receipt_id varchar(250);