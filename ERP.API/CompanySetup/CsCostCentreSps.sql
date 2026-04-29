-- Create a new cost centre
CREATE OR REPLACE FUNCTION public.sp_create_cs_cost_centre(
    p_company_id integer,
    p_parent_cost_centre_id integer,
    p_cost_centre_code varchar,
    p_cost_centre_name varchar,
    p_is_active boolean DEFAULT true
)
RETURNS TABLE (
    cost_centre_id integer,
    company_id integer,
    parent_cost_centre_id integer,
    cost_centre_code varchar,
    cost_centre_name varchar,
    is_active boolean,
    created_at timestamptz,
    updated_at timestamptz
) AS $$
BEGIN
    RETURN QUERY
    INSERT INTO public.cs_cost_centres (
        company_id,
        parent_cost_centre_id,
        cost_centre_code,
        cost_centre_name,
        is_active
    )
    VALUES (
        p_company_id,
        p_parent_cost_centre_id,
        p_cost_centre_code,
        p_cost_centre_name,
        p_is_active
    )
    RETURNING cost_centre_id, company_id, parent_cost_centre_id, cost_centre_code, cost_centre_name, is_active, created_at, updated_at;
END;
$$ LANGUAGE plpgsql;

-- Update an existing cost centre
CREATE OR REPLACE FUNCTION public.sp_update_cs_cost_centre(
    p_cost_centre_id integer,
    p_parent_cost_centre_id integer,
    p_cost_centre_code varchar,
    p_cost_centre_name varchar,
    p_is_active boolean
)
RETURNS TABLE (
    cost_centre_id integer,
    company_id integer,
    parent_cost_centre_id integer,
    cost_centre_code varchar,
    cost_centre_name varchar,
    is_active boolean,
    created_at timestamptz,
    updated_at timestamptz
) AS $$
BEGIN
    RETURN QUERY
    UPDATE public.cs_cost_centres cc
    SET
        parent_cost_centre_id = p_parent_cost_centre_id,
        cost_centre_code = p_cost_centre_code,
        cost_centre_name = p_cost_centre_name,
        is_active = p_is_active,
        updated_at = CURRENT_TIMESTAMP
    WHERE cc.cost_centre_id = p_cost_centre_id
    RETURNING cc.cost_centre_id, cc.company_id, cc.parent_cost_centre_id, cc.cost_centre_code, cc.cost_centre_name, cc.is_active, cc.created_at, cc.updated_at;
END;
$$ LANGUAGE plpgsql;

-- Delete a cost centre
CREATE OR REPLACE FUNCTION public.sp_delete_cs_cost_centre(
    p_cost_centre_id integer
)
RETURNS boolean AS $$
DECLARE
    v_count integer;
BEGIN
    -- Check if the cost centre has any child cost centres
    SELECT COUNT(*) INTO v_count
    FROM public.cs_cost_centres
    WHERE parent_cost_centre_id = p_cost_centre_id;

    IF v_count > 0 THEN
        RAISE EXCEPTION 'Cannot delete cost centre that has child cost centres';
    END IF;

    -- Check if the cost centre is used in branch_cost_centres
    SELECT COUNT(*) INTO v_count
    FROM public.cs_branch_cost_centres
    WHERE cost_centre_id = p_cost_centre_id;

    IF v_count > 0 THEN
        RAISE EXCEPTION 'Cannot delete cost centre that is associated with branches';
    END IF;

    DELETE FROM public.cs_cost_centres
    WHERE cost_centre_id = p_cost_centre_id;

    GET DIAGNOSTICS v_count = ROW_COUNT;
    RETURN v_count > 0;
END;
$$ LANGUAGE plpgsql;

-- Get a cost centre by ID
CREATE OR REPLACE FUNCTION public.sp_get_cs_cost_centre_by_id(
    p_cost_centre_id integer
)
RETURNS TABLE (
    cost_centre_id integer,
    company_id integer,
    parent_cost_centre_id integer,
    parent_cost_centre_name varchar,
    cost_centre_code varchar,
    cost_centre_name varchar,
    is_active boolean,
    created_at timestamptz,
    updated_at timestamptz
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        cc.cost_centre_id,
        cc.company_id,
        cc.parent_cost_centre_id,
        pcc.cost_centre_name as parent_cost_centre_name,
        cc.cost_centre_code,
        cc.cost_centre_name,
        cc.is_active,
        cc.created_at,
        cc.updated_at
    FROM public.cs_cost_centres cc
    LEFT JOIN public.cs_cost_centres pcc ON pcc.cost_centre_id = cc.parent_cost_centre_id
    WHERE cc.cost_centre_id = p_cost_centre_id;
END;
$$ LANGUAGE plpgsql;

-- Get cost centres by company with pagination
CREATE OR REPLACE FUNCTION public.sp_get_cs_cost_centres_by_company(
    p_company_id integer,
    p_page_number integer DEFAULT 1,
    p_page_size integer DEFAULT 10
)
RETURNS TABLE (
    cost_centre_id integer,
    company_id integer,
    parent_cost_centre_id integer,
    parent_cost_centre_name varchar,
    cost_centre_code varchar,
    cost_centre_name varchar,
    is_active boolean,
    created_at timestamptz,
    updated_at timestamptz,
    total_count bigint
) AS $$
BEGIN
    RETURN QUERY
    WITH paginated_results AS (
        SELECT 
            cc.cost_centre_id,
            cc.company_id,
            cc.parent_cost_centre_id,
            pcc.cost_centre_name as parent_cost_centre_name,
            cc.cost_centre_code,
            cc.cost_centre_name,
            cc.is_active,
            cc.created_at,
            cc.updated_at,
            COUNT(*) OVER() as total_count
        FROM public.cs_cost_centres cc
        LEFT JOIN public.cs_cost_centres pcc ON pcc.cost_centre_id = cc.parent_cost_centre_id
        WHERE cc.company_id = p_company_id
        ORDER BY cc.cost_centre_code
        LIMIT p_page_size
        OFFSET ((p_page_number - 1) * p_page_size)
    )
    SELECT * FROM paginated_results;
END;
$$ LANGUAGE plpgsql;

-- Search cost centres
CREATE OR REPLACE FUNCTION public.sp_search_cs_cost_centres(
    p_company_id integer,
    p_search_text varchar DEFAULT NULL,
    p_is_active boolean DEFAULT NULL,
    p_parent_cost_centre_id integer DEFAULT NULL,
    p_page_number integer DEFAULT 1,
    p_page_size integer DEFAULT 10
)
RETURNS TABLE (
    cost_centre_id integer,
    company_id integer,
    parent_cost_centre_id integer,
    parent_cost_centre_name varchar,
    cost_centre_code varchar,
    cost_centre_name varchar,
    is_active boolean,
    created_at timestamptz,
    updated_at timestamptz,
    total_count bigint
) AS $$
BEGIN
    RETURN QUERY
    WITH paginated_results AS (
        SELECT 
            cc.cost_centre_id,
            cc.company_id,
            cc.parent_cost_centre_id,
            pcc.cost_centre_name as parent_cost_centre_name,
            cc.cost_centre_code,
            cc.cost_centre_name,
            cc.is_active,
            cc.created_at,
            cc.updated_at,
            COUNT(*) OVER() as total_count
        FROM public.cs_cost_centres cc
        LEFT JOIN public.cs_cost_centres pcc ON pcc.cost_centre_id = cc.parent_cost_centre_id
        WHERE cc.company_id = p_company_id
        AND (p_search_text IS NULL 
            OR cc.cost_centre_code ILIKE '%' || p_search_text || '%'
            OR cc.cost_centre_name ILIKE '%' || p_search_text || '%')
        AND (p_is_active IS NULL OR cc.is_active = p_is_active)
        AND (p_parent_cost_centre_id IS NULL OR cc.parent_cost_centre_id = p_parent_cost_centre_id)
        ORDER BY cc.cost_centre_code
        LIMIT p_page_size
        OFFSET ((p_page_number - 1) * p_page_size)
    )
    SELECT * FROM paginated_results;
END;
$$ LANGUAGE plpgsql;

-- Get cost centre hierarchy
CREATE OR REPLACE FUNCTION public.sp_get_cs_cost_centre_hierarchy(
    p_company_id integer
)
RETURNS TABLE (
    cost_centre_id integer,
    parent_cost_centre_id integer,
    cost_centre_code varchar,
    cost_centre_name varchar,
    level integer,
    path text
) AS $$
BEGIN
    RETURN QUERY
    WITH RECURSIVE cost_centre_tree AS (
        -- Base case: top-level cost centres
        SELECT 
            cc.cost_centre_id,
            cc.parent_cost_centre_id,
            cc.cost_centre_code,
            cc.cost_centre_name,
            1 as level,
            cc.cost_centre_name::text as path
        FROM public.cs_cost_centres cc
        WHERE cc.company_id = p_company_id
        AND cc.parent_cost_centre_id IS NULL

        UNION ALL

        -- Recursive case: child cost centres
        SELECT 
            cc.cost_centre_id,
            cc.parent_cost_centre_id,
            cc.cost_centre_code,
            cc.cost_centre_name,
            cct.level + 1,
            cct.path || ' > ' || cc.cost_centre_name
        FROM public.cs_cost_centres cc
        INNER JOIN cost_centre_tree cct ON cc.parent_cost_centre_id = cct.cost_centre_id
        WHERE cc.company_id = p_company_id
    )
    SELECT * FROM cost_centre_tree
    ORDER BY path;
END;
$$ LANGUAGE plpgsql;



-- DROP PROCEDURE public.sp_cs_cost_centres_get_dropdown(int4);

CREATE OR REPLACE PROCEDURE public.sp_cs_cost_centres_get_dropdown(IN p_company_id integer)
 LANGUAGE plpgsql
AS $procedure$
BEGIN
    WITH RECURSIVE CostCentreHierarchy AS (
        SELECT 
            cost_centre_id,
            cost_centre_code,
            cost_centre_name,
            parent_cost_centre_id,
            (cost_centre_code || ' - ' || cost_centre_name)::TEXT as display_name,
            cost_centre_code::TEXT as hierarchy_path,
            1 as level
        FROM cs_cost_centres
        WHERE company_id = p_company_id AND parent_cost_centre_id IS NULL
        
        UNION ALL
        
        SELECT 
            cc.cost_centre_id,
            cc.cost_centre_code,
            cc.cost_centre_name,
            cc.parent_cost_centre_id,
            (repeat('    ', h.level) || cc.cost_centre_code || ' - ' || cc.cost_centre_name)::TEXT,
            (h.hierarchy_path || ' > ' || cc.cost_centre_code)::TEXT,
            h.level + 1
        FROM cs_cost_centres cc
        INNER JOIN CostCentreHierarchy h ON cc.parent_cost_centre_id = h.cost_centre_id
        WHERE cc.company_id = p_company_id
    )
    SELECT 
        cost_centre_id as value,
        display_name as label
    FROM CostCentreHierarchy
    ORDER BY hierarchy_path;
END;
$procedure$
;