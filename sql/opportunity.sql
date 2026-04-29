CREATE SEQUENCE sales_opportunities_id_seq START 1;

CREATE TABLE public.sales_opportunities (
    id varchar(255) DEFAULT nextval('sales_opportunities_id_seq')::text NOT NULL,
    user_created int4 NULL,
    date_created timestamp NULL,
    user_updated int4 NULL,
    date_updated timestamp NULL,
    status varchar(255) NULL,
    expected_completion date NULL,
    opportunity_type varchar(255) NULL,
    opportunity_for varchar(255) NULL,
    customer_id varchar(255) NULL,
    customer_name varchar(255) NULL,
    customer_type varchar(255) NULL,
    opportunity_name varchar(255) NULL,
    opportunity_id varchar(255) NOT NULL,
    comments text NULL,
    isactive bool DEFAULT false NOT NULL,
    lead_id varchar(255) NULL,
    sales_representative_id int4 NULL,
    contact_name varchar(255) NULL,
    contact_mobile_no varchar(255) NULL,
    CONSTRAINT sales_opportunities_opportunity_id_key UNIQUE (opportunity_id),
    CONSTRAINT sales_opportunities_pkey PRIMARY KEY (id)
);