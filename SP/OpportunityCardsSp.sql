-- Create function for Opportunity Cards count by status
DROP FUNCTION IF EXISTS sp_get_opportunity_cards_count();

CREATE OR REPLACE FUNCTION sp_get_opportunity_cards_count()
RETURNS TABLE (
    status VARCHAR,
    count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        so.status,
        COUNT(*)::BIGINT AS count
    FROM 
        public.sales_opportunities so
    WHERE 
        so.isactive = true
    GROUP BY 
        so.status
    HAVING 
        so.status IN ('Identified', 'Solution Presentation', 'Proposal', 'Negotiation', 'Closed Won');
END;
$$ LANGUAGE plpgsql;
