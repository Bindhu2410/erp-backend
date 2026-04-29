-- Create a new branch cost centre mapping
CREATE OR REPLACE FUNCTION public.sp_create_cs_branch_cost_centre(
    p_branch_id integer,
    p_cost_centre_id integer
)
RETURNS TABLE (
    branch_id integer,
    cost_centre_id integer
) AS $$
BEGIN
    INSERT INTO public.cs_branch_cost_centres (
        branch_id,
        cost_centre_id
    )
    VALUES (
        p_branch_id,
        p_cost_centre_id
    );

    RETURN QUERY
    SELECT 
        bcc.branch_id,
        bcc.cost_centre_id
    FROM public.cs_branch_cost_centres bcc
    WHERE bcc.branch_id = p_branch_id
    AND bcc.cost_centre_id = p_cost_centre_id;
END;
$$ LANGUAGE plpgsql;

-- Delete a branch cost centre mapping
CREATE OR REPLACE FUNCTION public.sp_delete_cs_branch_cost_centre(
    p_branch_id integer,
    p_cost_centre_id integer
)
RETURNS boolean AS $$
DECLARE
    v_count integer;
BEGIN
    DELETE FROM public.cs_branch_cost_centres
    WHERE branch_id = p_branch_id
    AND cost_centre_id = p_cost_centre_id;

    GET DIAGNOSTICS v_count = ROW_COUNT;
    RETURN v_count > 0;
END;
$$ LANGUAGE plpgsql;

-- Get cost centres by branch ID
CREATE OR REPLACE FUNCTION public.sp_get_cs_cost_centres_by_branch(
    p_branch_id integer
)
RETURNS TABLE (
    branch_id integer,
    cost_centre_id integer,
    cost_centre_name varchar,
    cost_centre_code varchar,
    description text,
    is_active boolean
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        bcc.branch_id,
        cc.cost_centre_id,
        cc.cost_centre_name,
        cc.cost_centre_code,
        cc.description,
        cc.is_active
    FROM public.cs_branch_cost_centres bcc
    INNER JOIN public.cs_cost_centres cc ON cc.cost_centre_id = bcc.cost_centre_id
    WHERE bcc.branch_id = p_branch_id;
END;
$$ LANGUAGE plpgsql;

-- Get branches by cost centre ID with pagination
CREATE OR REPLACE FUNCTION public.sp_get_cs_branches_by_cost_centre(
    p_cost_centre_id integer,
    p_page_number integer DEFAULT 1,
    p_page_size integer DEFAULT 10
)
RETURNS TABLE (
    branch_id integer,
    branch_name varchar,
    branch_code varchar,
    description text,
    is_active boolean,
    total_count bigint
) AS $$
BEGIN
    RETURN QUERY
    WITH paginated_results AS (
        SELECT 
            b.branch_id,
            b.branch_name,
            b.branch_code,
            b.description,
            b.is_active,
            COUNT(*) OVER() as total_count
        FROM public.cs_branch_cost_centres bcc
        INNER JOIN public.cs_branches b ON b.branch_id = bcc.branch_id
        WHERE bcc.cost_centre_id = p_cost_centre_id
        ORDER BY b.branch_name
        LIMIT p_page_size
        OFFSET ((p_page_number - 1) * p_page_size)
    )
    SELECT * FROM paginated_results;
END;
$$ LANGUAGE plpgsql;


-- DROP FUNCTION public.sp_get_cs_branch_cost_centres_dropdown(int4);

CREATE OR REPLACE FUNCTION public.sp_get_cs_branch_cost_centres_dropdown(p_branch_id integer)
 RETURNS TABLE(cost_centre_id integer, cost_centre_code character varying, name character varying, parent_cost_centre_id integer, path text)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    WITH RECURSIVE cost_centre_tree AS (
        -- Base case: top-level cost centres
        SELECT 
            c.cost_centre_id,
            c.cost_centre_code,
            c.name,
            c.parent_cost_centre_id,
            c.name::text AS path
        FROM cs_branch_cost_centres c
        WHERE c.branch_id = p_branch_id 
        AND c.parent_cost_centre_id IS NULL
        AND c.is_active = TRUE
        
        UNION ALL
        
        -- Recursive case: child cost centres
        SELECT 
            c.cost_centre_id,
            c.cost_centre_code,
            c.name,
            c.parent_cost_centre_id,
            ct.path || ' > ' || c.name
        FROM cs_branch_cost_centres c
        INNER JOIN cost_centre_tree ct ON c.parent_cost_centre_id = ct.cost_centre_id
        WHERE c.branch_id = p_branch_id
        AND c.is_active = TRUE
    )
    SELECT * FROM cost_centre_tree
    ORDER BY path;
END;
$function$
;