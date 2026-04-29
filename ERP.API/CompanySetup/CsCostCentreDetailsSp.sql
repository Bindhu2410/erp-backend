-- Create a stored procedure to get all cost centres with additional details
CREATE OR REPLACE FUNCTION public.sp_getall_cs_cost_centres_with_details()
RETURNS TABLE (
    cost_centre_id INT,
    company_id INT,
    parent_cost_centre_id INT,
    cost_centre_code VARCHAR,
    cost_centre_name VARCHAR,
    is_active BOOLEAN,
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ,
    company_name VARCHAR,
    parent_cost_centre_name VARCHAR,
    parent_cost_centre_code VARCHAR
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT
        cc.cost_centre_id,
        cc.company_id,
        cc.parent_cost_centre_id,
        cc.cost_centre_code,
        cc.cost_centre_name,
        cc.is_active,
        cc.created_at,
        cc.updated_at,
        c.company_name,
        parent.cost_centre_name AS parent_cost_centre_name,
        parent.cost_centre_code AS parent_cost_centre_code
    FROM 
        public.cs_cost_centres cc
    LEFT JOIN 
        public.cs_companies c ON cc.company_id = c.company_id
    LEFT JOIN 
        public.cs_cost_centres parent ON cc.parent_cost_centre_id = parent.cost_centre_id
    ORDER BY 
        cc.company_id, cc.cost_centre_code;
END;
$function$;
