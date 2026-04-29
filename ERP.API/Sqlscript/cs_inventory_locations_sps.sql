-- Get Inventory Location by ID
CREATE OR REPLACE FUNCTION public.sp_get_cs_inventory_location_by_id(
    p_location_id integer
)
RETURNS TABLE (
    location_id integer,
    warehouse_id integer,
    location_code varchar,
    location_name varchar,
    location_category varchar,
    capacity_weight numeric,
    capacity_weight_uom varchar,
    capacity_volume numeric,
    capacity_volume_uom varchar,
    capacity_item_count integer,
    created_at timestamptz,
    updated_at timestamptz
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT 
        l.location_id,
        l.warehouse_id,
        l.location_code,
        l.location_name,
        l.location_category,
        l.capacity_weight,
        l.capacity_weight_uom,
        l.capacity_volume,
        l.capacity_volume_uom,
        l.capacity_item_count,
        l.created_at,
        l.updated_at
    FROM public.cs_inventory_locations l
    WHERE l.location_id = p_location_id;
END;
$function$;

-- Search Inventory Locations with pagination
CREATE OR REPLACE FUNCTION public.sp_search_cs_inventory_locations(
    p_warehouse_id integer = NULL,
    p_search_text varchar = NULL,
    p_location_category varchar = NULL,
    p_page_size integer = 10,
    p_page_number integer = 1
)
RETURNS TABLE (
    location_id integer,
    warehouse_id integer,
    location_code varchar,
    location_name varchar,
    location_category varchar,
    capacity_weight numeric,
    capacity_weight_uom varchar,
    capacity_volume numeric,
    capacity_volume_uom varchar,
    capacity_item_count integer,
    created_at timestamptz,
    updated_at timestamptz,
    total_count bigint,
    filtered_count bigint
)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_offset integer;
    v_where text;
    v_base_query text;
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Build WHERE clause
    v_where := 'WHERE 1=1';
    IF p_warehouse_id IS NOT NULL THEN
        v_where := v_where || ' AND l.warehouse_id = ' || p_warehouse_id::text;
    END IF;
    IF p_location_category IS NOT NULL THEN
        v_where := v_where || ' AND l.location_category = ' || quote_literal(p_location_category);
    END IF;
    IF p_search_text IS NOT NULL AND p_search_text <> '' THEN
        v_where := v_where || ' AND (l.location_code ILIKE ' || quote_literal('%' || p_search_text || '%') ||
                   ' OR l.location_name ILIKE ' || quote_literal('%' || p_search_text || '%') || ')';
    END IF;
    
    -- Build and execute query
    RETURN QUERY EXECUTE '
    WITH CountCTE AS (
        SELECT 
            COUNT(*) AS total_count,
            COUNT(*) FILTER (' || v_where || ') AS filtered_count
        FROM public.cs_inventory_locations l
    )
    SELECT 
        l.location_id,
        l.warehouse_id,
        l.location_code,
        l.location_name,
        l.location_category,
        l.capacity_weight,
        l.capacity_weight_uom,
        l.capacity_volume,
        l.capacity_volume_uom,
        l.capacity_item_count,
        l.created_at,
        l.updated_at,
        c.total_count,
        c.filtered_count
    FROM public.cs_inventory_locations l
    CROSS JOIN CountCTE c ' ||
    v_where || '
    ORDER BY l.location_code
    LIMIT ' || p_page_size || '
    OFFSET ' || v_offset;
END;
$function$;

-- Create Inventory Location
CREATE OR REPLACE FUNCTION public.sp_create_cs_inventory_location(
    p_warehouse_id integer,
    p_location_code varchar,
    p_location_name varchar,
    p_location_category varchar = NULL,
    p_capacity_weight numeric = NULL,
    p_capacity_weight_uom varchar = NULL,
    p_capacity_volume numeric = NULL,
    p_capacity_volume_uom varchar = NULL,
    p_capacity_item_count integer = NULL
)
RETURNS integer
LANGUAGE plpgsql
AS $function$
DECLARE
    v_location_id integer;
BEGIN
    INSERT INTO public.cs_inventory_locations(
        warehouse_id,
        location_code,
        location_name,
        location_category,
        capacity_weight,
        capacity_weight_uom,
        capacity_volume,
        capacity_volume_uom,
        capacity_item_count,
        created_at,
        updated_at
    )
    VALUES (
        p_warehouse_id,
        p_location_code,
        p_location_name,
        p_location_category,
        p_capacity_weight,
        p_capacity_weight_uom,
        p_capacity_volume,
        p_capacity_volume_uom,
        p_capacity_item_count,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    )
    RETURNING location_id INTO v_location_id;
    
    RETURN v_location_id;
END;
$function$;

-- Update Inventory Location
CREATE OR REPLACE FUNCTION public.sp_update_cs_inventory_location(
    p_location_id integer,
    p_warehouse_id integer,
    p_location_code varchar,
    p_location_name varchar,
    p_location_category varchar = NULL,
    p_capacity_weight numeric = NULL,
    p_capacity_weight_uom varchar = NULL,
    p_capacity_volume numeric = NULL,
    p_capacity_volume_uom varchar = NULL,
    p_capacity_item_count integer = NULL
)
RETURNS boolean
LANGUAGE plpgsql
AS $function$
DECLARE
    v_count integer;
BEGIN
    UPDATE public.cs_inventory_locations
    SET 
        warehouse_id = p_warehouse_id,
        location_code = p_location_code,
        location_name = p_location_name,
        location_category = p_location_category,
        capacity_weight = p_capacity_weight,
        capacity_weight_uom = p_capacity_weight_uom,
        capacity_volume = p_capacity_volume,
        capacity_volume_uom = p_capacity_volume_uom,
        capacity_item_count = p_capacity_item_count,
        updated_at = CURRENT_TIMESTAMP
    WHERE location_id = p_location_id;
    
    GET DIAGNOSTICS v_count = ROW_COUNT;
    RETURN v_count > 0;
END;
$function$;

-- Delete Inventory Location
CREATE OR REPLACE FUNCTION public.sp_delete_cs_inventory_location(
    p_location_id integer
)
RETURNS boolean
LANGUAGE plpgsql
AS $function$
DECLARE
    v_count integer;
BEGIN
    DELETE FROM public.cs_inventory_locations
    WHERE location_id = p_location_id;
    
    GET DIAGNOSTICS v_count = ROW_COUNT;
    RETURN v_count > 0;
END;
$function$;

-- Get Inventory Locations by Warehouse
CREATE OR REPLACE FUNCTION public.sp_get_cs_inventory_locations_by_warehouse(
    p_warehouse_id integer,
    p_page_size integer = 10,
    p_page_number integer = 1
)
RETURNS TABLE (
    location_id integer,
    warehouse_id integer,
    location_code varchar,
    location_name varchar,
    location_category varchar,
    capacity_weight numeric,
    capacity_weight_uom varchar,
    capacity_volume numeric,
    capacity_volume_uom varchar,
    capacity_item_count integer,
    created_at timestamptz,
    updated_at timestamptz,
    total_count bigint
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    WITH CountCTE AS (
        SELECT COUNT(*) AS total_count
        FROM cs_inventory_locations l
        WHERE l.warehouse_id = p_warehouse_id
    )
    SELECT 
        l.location_id,
        l.warehouse_id,
        l.location_code,
        l.location_name,
        l.location_category,
        l.capacity_weight,
        l.capacity_weight_uom,
        l.capacity_volume,
        l.capacity_volume_uom,
        l.capacity_item_count,
        l.created_at,
        l.updated_at,
        c.total_count
    FROM cs_inventory_locations l
    CROSS JOIN CountCTE c
    WHERE l.warehouse_id = p_warehouse_id
    ORDER BY l.location_code
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
END;
$function$;
