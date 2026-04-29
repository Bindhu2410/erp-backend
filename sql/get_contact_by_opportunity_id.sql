-- =============================================

-- Author:      Brindha
-- Create date: 2025-07-29
-- Description: Get contact name and mobile no by opportunity id with error handling
-- =============================================
CREATE OR REPLACE FUNCTION public.get_contact_by_opportunity_id(p_opportunity_id text)
RETURNS TABLE(contact_name text, mobile_no text) AS $$
BEGIN
    IF p_opportunity_id IS NULL OR trim(p_opportunity_id) = '' THEN
        RAISE EXCEPTION 'Opportunity ID cannot be null or empty';
    END IF;
    RETURN QUERY
    SELECT sc.contact_name, sc.mobile_no
    FROM public.sales_contacts sc
    JOIN public.sales_lead sl ON sc.sales_lead_id = sl.id
    JOIN public.sales_opportunities so ON sl.lead_id = so.lead_id
    WHERE so.opportunity_id = p_opportunity_id;
END;
$$ LANGUAGE plpgsql;

-- Usage:
-- SELECT * FROM public.get_contact_by_opportunity_id('OPP00002');
