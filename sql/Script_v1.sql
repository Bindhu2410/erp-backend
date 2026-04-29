CREATE OR REPLACE PROCEDURE Sp_getOpportunitiesbyleadId(
    IN p_lead_id VARCHAR(255)
)
LANGUAGE plpgsql
AS $$
BEGIN
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
        sales_leads sl ON so.lead_id = sl.id
    WHERE 
        so.lead_id = p_lead_id
        AND so.isactive = true;
END;
$$;



-- public.geographical_divisions definition

-- Drop table

-- DROP TABLE public.geographical_divisions;

