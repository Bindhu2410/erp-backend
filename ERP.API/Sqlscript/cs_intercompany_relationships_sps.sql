-- Get Intercompany Relationship by ID
CREATE OR REPLACE FUNCTION public.sp_get_cs_intercompany_relationship_by_id(
    p_relationship_id integer
)
RETURNS TABLE (
    relationship_id integer,
    company1_id integer,
    company2_id integer,
    relationship_type varchar(50),
    is_active boolean,
    created_at timestamptz,
    updated_at timestamptz
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT 
        r.relationship_id,
        r.company1_id,
        r.company2_id,
        r.relationship_type,
        r.is_active,
        r.created_at,
        r.updated_at
    FROM public.cs_intercompany_relationships r
    WHERE r.relationship_id = p_relationship_id;
END;
$function$;

-- Search Intercompany Relationships with pagination
CREATE OR REPLACE FUNCTION public.sp_search_cs_intercompany_relationships(
    p_company_id integer = NULL,
    p_relationship_type varchar = NULL,
    p_is_active boolean = NULL,
    p_page_size integer = 10,
    p_page_number integer = 1
)
RETURNS TABLE (
    relationship_id integer,
    company1_id integer,
    company2_id integer,
    relationship_type varchar(50),
    is_active boolean,
    created_at timestamptz,
    updated_at timestamptz,
    total_count bigint,
    filtered_count bigint
)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_offset integer;
    v_total_count bigint;
    v_filtered_count bigint;
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Get total count
    SELECT COUNT(*) INTO v_total_count
    FROM public.cs_intercompany_relationships;
    
    -- Get filtered count
    SELECT COUNT(*) INTO v_filtered_count
    FROM public.cs_intercompany_relationships r
    WHERE (p_company_id IS NULL OR (r.company1_id = p_company_id OR r.company2_id = p_company_id))
    AND (p_relationship_type IS NULL OR r.relationship_type = p_relationship_type)
    AND (p_is_active IS NULL OR r.is_active = p_is_active);
    
    -- Return the results
    RETURN QUERY
    SELECT 
        r.relationship_id,
        r.company1_id,
        r.company2_id,
        r.relationship_type,
        r.is_active,
        r.created_at,
        r.updated_at,
        v_total_count,
        v_filtered_count
    FROM public.cs_intercompany_relationships r
    WHERE (p_company_id IS NULL OR (r.company1_id = p_company_id OR r.company2_id = p_company_id))
    AND (p_relationship_type IS NULL OR r.relationship_type = p_relationship_type)
    AND (p_is_active IS NULL OR r.is_active = p_is_active)
    ORDER BY r.relationship_id
    LIMIT p_page_size
    OFFSET v_offset;
END;
$function$;

-- Create Intercompany Relationship
CREATE OR REPLACE FUNCTION public.sp_create_cs_intercompany_relationship(
    p_company1_id integer,
    p_company2_id integer,
    p_relationship_type varchar(50),
    p_is_active boolean = true
)
RETURNS integer
LANGUAGE plpgsql
AS $function$
DECLARE
    v_relationship_id integer;
BEGIN
    INSERT INTO public.cs_intercompany_relationships(
        company1_id,
        company2_id,
        relationship_type,
        is_active,
        created_at,
        updated_at
    )
    VALUES (
        p_company1_id,
        p_company2_id,
        p_relationship_type,
        p_is_active,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    )
    RETURNING relationship_id INTO v_relationship_id;
    
    RETURN v_relationship_id;
END;
$function$;

-- Update Intercompany Relationship
CREATE OR REPLACE FUNCTION public.sp_update_cs_intercompany_relationship(
    p_relationship_id integer,
    p_company1_id integer,
    p_company2_id integer,
    p_relationship_type varchar(50),
    p_is_active boolean
)
RETURNS boolean
LANGUAGE plpgsql
AS $function$
DECLARE
    v_count integer;
BEGIN
    UPDATE public.cs_intercompany_relationships
    SET 
        company1_id = p_company1_id,
        company2_id = p_company2_id,
        relationship_type = p_relationship_type,
        is_active = p_is_active,
        updated_at = CURRENT_TIMESTAMP
    WHERE relationship_id = p_relationship_id;
    
    GET DIAGNOSTICS v_count = ROW_COUNT;
    RETURN v_count > 0;
END;
$function$;

-- Delete Intercompany Relationship
CREATE OR REPLACE FUNCTION public.sp_delete_cs_intercompany_relationship(
    p_relationship_id integer
)
RETURNS boolean
LANGUAGE plpgsql
AS $function$
DECLARE
    v_count integer;
BEGIN
    DELETE FROM public.cs_intercompany_relationships
    WHERE relationship_id = p_relationship_id;
    
    GET DIAGNOSTICS v_count = ROW_COUNT;
    RETURN v_count > 0;
END;
$function$;


-- DROP FUNCTION public.sp_get_cs_intercompany_relationships_by_company(int4, int4, int4);

CREATE OR REPLACE FUNCTION public.sp_get_cs_intercompany_relationships_by_company(
    p_company_id INTEGER,
    p_page_number INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 10
)
RETURNS TABLE (
    relationship_id INTEGER,
    company1_id INTEGER,
    company2_id INTEGER,
    relationship_type VARCHAR,
    is_active BOOLEAN,
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ,
    company1_name VARCHAR,
    company2_name VARCHAR,
    total_count BIGINT
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    WITH CountCTE AS (
        SELECT COUNT(*) AS total_count
        FROM cs_intercompany_relationships r
        WHERE r.company1_id = p_company_id OR r.company2_id = p_company_id
    )
    SELECT 
        r.relationship_id,
        r.company1_id,
        r.company2_id,
        r.relationship_type,
        r.is_active,
        r.created_at,
        r.updated_at,
        c1.legal_company_name AS company1_name,
        c2.legal_company_name AS company2_name,
        c.total_count
    FROM cs_intercompany_relationships r
    INNER JOIN cs_companies c1 ON r.company1_id = c1.company_id
    INNER JOIN cs_companies c2 ON r.company2_id = c2.company_id
    CROSS JOIN CountCTE c
    WHERE r.company1_id = p_company_id OR r.company2_id = p_company_id
    ORDER BY c1.legal_company_name, c2.legal_company_name
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
END;
$function$;

