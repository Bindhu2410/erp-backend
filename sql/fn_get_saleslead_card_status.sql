-- Function: fn_get_saleslead_card_status
-- Returns Total Leads, New This Week, Qualified Leads

CREATE OR REPLACE FUNCTION fn_get_saleslead_card_status()
RETURNS TABLE (
    total_leads INT,
    new_this_week INT,
    qualified_leads INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*) AS total_leads,
        COUNT(*) FILTER (WHERE date_created >= date_trunc('week', CURRENT_DATE)) AS new_this_week,
        COUNT(*) FILTER (WHERE status = 'Qualified') AS qualified_leads
    FROM public.sales_lead;
END;
$$ LANGUAGE plpgsql;
