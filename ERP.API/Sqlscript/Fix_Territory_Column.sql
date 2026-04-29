-- Drop the problematic foreign key constraint if it exists
ALTER TABLE IF EXISTS public.sales_lead DROP CONSTRAINT IF EXISTS fk_sales_lead_territory;

-- Ensure the territory column is properly defined as text
ALTER TABLE public.sales_lead ALTER COLUMN territory TYPE VARCHAR(255);

-- Update the create/update stored procedures to use the territory text field
CREATE OR REPLACE FUNCTION public.sp_sales_lead_create(p_data TEXT)
RETURNS public.sales_lead AS
$$
DECLARE
    v_json JSON;
    v_result public.sales_lead%ROWTYPE;
BEGIN
    -- Parse the text parameter to JSON
    v_json := p_data::JSON;
    
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
        COALESCE((v_json->>'isActive')::boolean, (v_json->>'is_active')::boolean, true),
        COALESCE(v_json->>'comments', v_json->>'comments'),
        COALESCE(v_json->>'leadType', v_json->>'lead_type'),
        COALESCE(v_json->>'contactName', v_json->>'contact_name'),
        COALESCE(v_json->>'salutation', v_json->>'salutation'),
        COALESCE(v_json->>'contactMobileNo', v_json->>'contact_mobile_no'),
        COALESCE(v_json->>'landLineNo', v_json->>'land_line_no'),
        COALESCE(v_json->>'email', v_json->>'email'),
        COALESCE(v_json->>'fax', v_json->>'fax'),
        COALESCE(v_json->>'doorNo', v_json->>'door_no'),
        COALESCE(v_json->>'street', v_json->>'street'),
        COALESCE(v_json->>'landmark', v_json->>'landmark'),
        COALESCE(v_json->>'website', v_json->>'website'),
        COALESCE(v_json->>'territory', v_json->>'territory'), -- Use territory directly as text
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
    ) RETURNING *;

    RETURN v_result;
END;
$$ LANGUAGE plpgsql;
