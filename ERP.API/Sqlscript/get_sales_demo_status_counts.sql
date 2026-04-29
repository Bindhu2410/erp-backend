CREATE OR REPLACE FUNCTION get_sales_demo_status_counts()
RETURNS TABLE(
    requested_count integer,
    scheduled_count integer,
    cancelled_count integer,
    completed_count integer
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*) FILTER (WHERE LOWER(status) IN ('demo requested', 'requested')) AS requested_count,
        COUNT(*) FILTER (WHERE LOWER(status) IN ('demo scheduled', 'scheduled')) AS scheduled_count,
        COUNT(*) FILTER (WHERE LOWER(status) IN ('demo cancelled', 'cancelled')) AS cancelled_count,
        COUNT(*) FILTER (WHERE LOWER(status) IN ('demo completed', 'completed')) AS completed_count
    FROM sales_demos;
END;
$$ LANGUAGE plpgsql;
