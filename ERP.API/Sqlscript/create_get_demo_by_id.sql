-- Create function to get a sales demo by ID
DROP FUNCTION IF EXISTS public.fn_get_sales_demo_by_id(INTEGER);

CREATE OR REPLACE FUNCTION public.fn_get_sales_demo_by_id(
    IN p_id INTEGER
)
RETURNS TABLE (
    id INTEGER,
    user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    user_id INTEGER,
    demo_date TIMESTAMP,
    status VARCHAR(100),
    address_id INTEGER,
    opportunity_id INTEGER,
    customer_id INTEGER,
    demo_contact VARCHAR(255),
    customer_name VARCHAR(255),
    demo_name VARCHAR(255),
    demo_approach VARCHAR(255),
    demo_outcome VARCHAR(255),
    demo_feedback VARCHAR(255),
    comments VARCHAR(255),
    presenter_id INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        sd.id,
        sd.user_created,
        sd.date_created,
        sd.user_updated,
        sd.date_updated,
        sd.user_id,
        sd.demo_date,
        sd.status,
        sd.address_id,
        sd.opportunity_id,
        sd.customer_id,
        sd.demo_contact,
        sd.customer_name,
        sd.demo_name,
        sd.demo_approach,
        sd.demo_outcome,
        sd.demo_feedback,
        sd.comments,
        sd.presenter_id
    FROM public.sales_demos sd
    WHERE sd.id = p_id;
END;
$$ LANGUAGE plpgsql;
