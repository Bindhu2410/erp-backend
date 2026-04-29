-- Create function instead of procedure
CREATE OR REPLACE FUNCTION public.fn_getopportunitiesbyleadid(p_lead_id INT)
RETURNS TABLE (
    id INT,
    status VARCHAR,
    expected_completion DATE,
    lost_reason TEXT,
    opportunity_type VARCHAR,
    opportunity_for VARCHAR,
    customer_id VARCHAR,
    customer_name VARCHAR,
    customer_type VARCHAR,
    opportunity_name VARCHAR,
    opportunity_id VARCHAR,
    comments TEXT,
    isactive BOOLEAN,
    lead_id VARCHAR,
    sales_leads_id INT,
    sales_representative_id INT,
    contact_name VARCHAR,
    contact_mobile_no VARCHAR,
    email VARCHAR,
    door_no VARCHAR,
    street VARCHAR,
    landmark VARCHAR,
    territory_id INT,
    area_id INT,
    city_id INT,
    district_id INT,
    state_id INT,
    pincode_id INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        so.id,
        so.status,
        so.expected_completion,
        so.lost_reason,
        so.opportunity_type,
        so.opportunity_for,
        so.customer_id,
        so.customer_name,
        so.customer_type,
        so.opportunity_name,
        so.opportunity_id,
        so.comments,
        so.isactive,
        so.lead_id,
        so.sales_leads_id,
        so.sales_representative_id,
        so.contact_name,
        so.contact_mobile_no,
        so.email,
        so.door_no,
        so.street,
        so.landmark,
        so.territory_id,
        so.area_id,
        so.city_id,
        so.district_id,
        so.state_id,
        so.pincode_id
    FROM 
        sales_opportunities so
    INNER JOIN 
        sales_leads sl ON so.sales_leads_id = sl.id
    WHERE 
        so.sales_leads_id = p_lead_id
        AND so.isactive = true;
END;
$$ LANGUAGE plpgsql;



-- public.sales_lead_interestedproducts definition

-- Drop table

-- DROP TABLE public.sales_lead_interestedproducts;

CREATE TABLE public.sales_lead_interestedproducts (
	id serial4 NOT NULL,
	leadid int4 NOT NULL,
	make text NOT NULL,
	model text NOT NULL,
	category text NOT NULL,
	notes text NULL,
	productname text NOT NULL,
	productcode text NOT NULL,
	CONSTRAINT sales_lead_interestedproducts_pkey PRIMARY KEY (id)
);


-- public.sales_lead_interestedproducts foreign keys

ALTER TABLE public.sales_lead_interestedproducts ADD CONSTRAINT fk_sales_leads FOREIGN KEY (leadid) REFERENCES public.sales_leads(id) ON DELETE CASCADE;



-- public.sales_deals definition

-- Drop table

-- DROP TABLE public.sales_deals;

CREATE TABLE public.sales_deals (
	id serial4 NOT NULL,
	user_created int4 NULL,
	date_created timestamp NULL,
	user_updated int4 NULL,
	date_updated timestamp NULL,
	status varchar(255) NULL,
	deal_name varchar(255) NULL,
	amount float8 NULL,
	expected_revenue float8 NULL,
	deal_age varchar(255) NULL,
	deal_for varchar(255) NULL,
	close_date date NULL,
	isactive bool DEFAULT false NULL,
	"comments" text NULL,
	opportunities_id int4 NULL,
	sales_representative_id int4 NULL,
	territory_id int4 NULL,
	area_id int4 NULL,
	city_id int4 NULL,
	district_id int4 NULL,
	state_id int4 NULL,
	pincode_id int4 NULL
);