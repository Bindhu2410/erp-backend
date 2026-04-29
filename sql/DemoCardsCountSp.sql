-- Create stored procedure for Demo Cards count
DROP FUNCTION IF EXISTS sp_get_demo_cards_count();

CREATE OR REPLACE FUNCTION sp_get_demo_cards_count()
RETURNS TABLE (
    demo_requested BIGINT,
    demo_scheduled BIGINT,
    demo_completed BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*) FILTER (WHERE status = 'Demo Requested') AS demo_requested,
        COUNT(*) FILTER (WHERE status = 'Demo Scheduled') AS demo_scheduled,
        COUNT(*) FILTER (WHERE status = 'Demo Completed') AS demo_completed
    FROM public.sales_demos;
END;
$$ LANGUAGE plpgsql;
