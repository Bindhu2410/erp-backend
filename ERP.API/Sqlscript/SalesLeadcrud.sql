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
	qualification_status varchar(255) NULL,
	event_name varchar(255) NULL,
	lead_id varchar(255) NULL,
	status varchar(255) NULL,
	score varchar(255) NULL,
	isactive bool DEFAULT false NULL,
	"comments" text NULL,
	lead_type varchar(255) NULL,
	contact_name varchar(100) NULL,
	salutation varchar(10) NULL,
	contact_mobile_no int8 NULL,
	land_line_no varchar(15) NULL,
	email varchar(100) NULL,
	fax varchar(15) NULL,
	door_no varchar(5) NULL,
	street varchar(50) NULL,
	landmark varchar(50) NULL,
	website varchar(100) NULL,
	geographical_divisions_id int4 NULL,	territory varchar(255) NULL,  -- Direct text field for territory name
	area_id int4 NULL,
	area varchar(255) NULL,
	city varchar(255) NULL,
	pincode_id int4 NULL,
	pincode varchar(255) NULL,
	district varchar(255) NULL,
	state varchar(255) NULL,
	country varchar(255) NULL,
	converted_customer_id varchar(255) NULL,
	user_id int4 NULL,
	CONSTRAINT pk_sales_lead PRIMARY KEY (id)
);


-- public.sales_lead foreign keys

ALTER TABLE public.sales_lead ADD CONSTRAINT fk_sales_lead_geodiv FOREIGN KEY (geographical_divisions_id) REFERENCES public.geographical_divisions(division_id);
ALTER TABLE public.sales_lead ADD CONSTRAINT fk_sales_lead_userid FOREIGN KEY (user_id) REFERENCES public."User"("UserID");




CREATE OR REPLACE FUNCTION public.sp_sales_lead_create(p_data TEXT)
RETURNS public.sales_lead AS
$$
DECLARE
    v_json JSON;
    v_result public.sales_lead%ROWTYPE;
    v_debug TEXT;
BEGIN
    -- Parse the text parameter to JSON
    v_json := p_data::JSON;
    
    -- Debugging: Log the parsed JSON to see actual content
    RAISE NOTICE 'Parsed JSON: %', v_json;
    
    INSERT INTO public.sales_lead (
        user_created, date_created, user_updated, date_updated,
        customer_name, lead_source, referral_source_name, hospital_of_referral,
        department_of_referral, social_media, event_date, qualification_status,
        event_name, lead_id, status, score, isactive, comments,
        lead_type, contact_name, salutation, contact_mobile_no, land_line_no,
        email, fax, door_no, street, landmark, website, territory, 
        area_id, area, city, pincode_id, pincode, district,
        state, country, converted_customer_id, user_id) 
    VALUES (
        COALESCE((v_json->>'userCreated')::int, (v_json->>'user_created')::int),
        COALESCE((v_json->>'dateCreated')::timestamp, (v_json->>'date_created')::timestamp, now()),
        COALESCE((v_json->>'userUpdated')::int, (v_json->>'user_updated')::int),
        COALESCE((v_json->>'dateUpdated')::timestamp, (v_json->>'date_updated')::timestamp),
        COALESCE(v_json->>'customerName', v_json->>'customer_name'),
        COALESCE(v_json->>'leadSource', v_json->>'lead_source'),
        COALESCE(v_json->>'referralSourceName', v_json->>'referral_source_name'),
        COALESCE(v_json->>'hospitalOfReferral', v_json->>'hospital_of_referral'),
        COALESCE(v_json->>'departmentOfReferral', v_json->>'department_of_referral'),
        COALESCE(v_json->>'socialMedia', v_json->>'social_media'),
        COALESCE((v_json->>'eventDate')::date, (v_json->>'event_date')::date),
        COALESCE(v_json->>'qualificationStatus', v_json->>'qualification_status'),
        COALESCE(v_json->>'eventName', v_json->>'event_name'),
        COALESCE(v_json->>'leadId', v_json->>'lead_id'),
        COALESCE(v_json->>'status', v_json->>'status'),
        COALESCE(v_json->>'score', v_json->>'score'),
        COALESCE((v_json->>'isActive')::boolean, (v_json->>'isactive')::boolean, true),
        COALESCE(v_json->>'comments', v_json->>'comments'),
        COALESCE(v_json->>'leadType', v_json->>'lead_type'),
        COALESCE(v_json->>'contactName', v_json->>'contact_name'),
        COALESCE(v_json->>'salutation', v_json->>'salutation'),
        COALESCE((v_json->>'contactMobileNo')::bigint, (v_json->>'contact_mobile_no')::bigint),
        COALESCE(v_json->>'landLineNo', v_json->>'land_line_no'),
        COALESCE(v_json->>'email', v_json->>'email'),
        COALESCE(v_json->>'fax', v_json->>'fax'),
        COALESCE(v_json->>'doorNo', v_json->>'door_no'),
        COALESCE(v_json->>'street', v_json->>'street'),
        COALESCE(v_json->>'landmark', v_json->>'landmark'),
        COALESCE(v_json->>'website', v_json->>'website'),
        COALESCE(v_json->>'territory', v_json->>'territory'),
        COALESCE((v_json->>'areaId')::int, (v_json->>'area_id')::int),
        COALESCE(v_json->>'area', v_json->>'area'),
        COALESCE(v_json->>'city', v_json->>'city'),
        COALESCE((v_json->>'pincodeId')::int, (v_json->>'pincode_id')::int),
        COALESCE(v_json->>'pincode', v_json->>'pincode'),
        COALESCE(v_json->>'district', v_json->>'district'),
        COALESCE(v_json->>'state', v_json->>'state'),
        COALESCE(v_json->>'country', v_json->>'country'),
        COALESCE(v_json->>'convertedCustomerId', v_json->>'converted_customer_id'),
        COALESCE((v_json->>'userId')::int, (v_json->>'user_id')::int)
    )
    RETURNING * INTO v_result;

    RETURN v_result;
END;
$$ LANGUAGE plpgsql;




CREATE OR REPLACE FUNCTION public.sp_sales_lead_read(p_id INT DEFAULT NULL)
RETURNS SETOF public.sales_lead AS
$$
BEGIN
    IF p_id IS NULL THEN
        RETURN QUERY SELECT * FROM public.sales_lead WHERE isactive = true;
    ELSE
        RETURN QUERY SELECT * FROM public.sales_lead WHERE id = p_id;
    END IF;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION public.sp_sales_lead_update(p_id INT, p_data TEXT)
RETURNS public.sales_lead AS
$$
DECLARE
    v_json JSON;
    v_result public.sales_lead%ROWTYPE;
BEGIN
    -- Parse the text parameter to JSON
    v_json := p_data::JSON;
    
    -- Debug log to see the actual JSON content
    RAISE NOTICE 'Parsed update JSON: %', v_json;
    
    IF p_id IS NULL THEN
        RAISE EXCEPTION 'ID is required for UPDATE';
    END IF;
    
    UPDATE public.sales_lead
    SET
        user_updated = COALESCE((v_json->>'userUpdated')::int, (v_json->>'user_updated')::int, user_updated),
        date_updated = COALESCE((v_json->>'dateUpdated')::timestamp, (v_json->>'date_updated')::timestamp, now()),
        customer_name = COALESCE(v_json->>'customerName', v_json->>'customer_name', customer_name),
        lead_source = COALESCE(v_json->>'leadSource', v_json->>'lead_source', lead_source),
        referral_source_name = COALESCE(v_json->>'referralSourceName', v_json->>'referral_source_name', referral_source_name),
        hospital_of_referral = COALESCE(v_json->>'hospitalOfReferral', v_json->>'hospital_of_referral', hospital_of_referral),
        department_of_referral = COALESCE(v_json->>'departmentOfReferral', v_json->>'department_of_referral', department_of_referral),
        social_media = COALESCE(v_json->>'socialMedia', v_json->>'social_media', social_media),
        event_date = COALESCE((v_json->>'eventDate')::date, (v_json->>'event_date')::date, event_date),
        qualification_status = COALESCE(v_json->>'qualificationStatus', v_json->>'qualification_status', qualification_status),
        event_name = COALESCE(v_json->>'eventName', v_json->>'event_name', event_name),
        lead_id = COALESCE(v_json->>'leadId', v_json->>'lead_id', lead_id),
        status = COALESCE(v_json->>'status', status),
        score = COALESCE(v_json->>'score', score),
        isactive = COALESCE((v_json->>'isActive')::boolean, (v_json->>'isactive')::boolean, isactive),
        comments = COALESCE(v_json->>'comments', comments),
        lead_type = COALESCE(v_json->>'leadType', v_json->>'lead_type', lead_type),
        contact_name = COALESCE(v_json->>'contactName', v_json->>'contact_name', contact_name),
        salutation = COALESCE(v_json->>'salutation', salutation),
        contact_mobile_no = COALESCE((v_json->>'contactMobileNo')::bigint, (v_json->>'contact_mobile_no')::bigint, contact_mobile_no),
        land_line_no = COALESCE(v_json->>'landLineNo', v_json->>'land_line_no', land_line_no),
        email = COALESCE(v_json->>'email', email),
        fax = COALESCE(v_json->>'fax', fax),
        door_no = COALESCE(v_json->>'doorNo', v_json->>'door_no', door_no),
        street = COALESCE(v_json->>'street', street),
        landmark = COALESCE(v_json->>'landmark', landmark),
        website = COALESCE(v_json->>'website', website),
        territory = COALESCE(v_json->>'territory', territory),
        area_id = COALESCE((v_json->>'areaId')::int, (v_json->>'area_id')::int, area_id),
        area = COALESCE(v_json->>'area', area),
        city = COALESCE(v_json->>'city', city),
        pincode_id = COALESCE((v_json->>'pincodeId')::int, (v_json->>'pincode_id')::int, pincode_id),
        pincode = COALESCE(v_json->>'pincode', pincode),
        district = COALESCE(v_json->>'district', district),
        state = COALESCE(v_json->>'state', state),
        country = COALESCE(v_json->>'country', country),
        converted_customer_id = COALESCE(v_json->>'convertedCustomerId', v_json->>'converted_customer_id', converted_customer_id),
        user_id = COALESCE((v_json->>'userId')::int, (v_json->>'user_id')::int, user_id)
    WHERE id = p_id
    RETURNING * INTO v_result;

    RETURN v_result;
END;
$$ LANGUAGE plpgsql;



CREATE OR REPLACE FUNCTION public.sp_sales_lead_soft_delete(p_id INT, p_user_updated INT)
RETURNS public.sales_lead AS
$$
DECLARE
    v_result public.sales_lead%ROWTYPE;
BEGIN
    IF p_id IS NULL THEN
        RAISE EXCEPTION 'ID is required for soft DELETE';
    END IF;

    UPDATE public.sales_lead
    SET 
        isactive = false,
        date_updated = now(),
        user_updated = p_user_updated
    WHERE id = p_id
    RETURNING * INTO v_result;

    RETURN v_result;
END;
$$ LANGUAGE plpgsql;


