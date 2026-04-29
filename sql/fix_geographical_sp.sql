-- Fix for ambiguous column reference in the hierarchy procedure
CREATE OR REPLACE FUNCTION sp_get_geographical_hierarchy_by_pincode(
    p_pincode VARCHAR
)
RETURNS TABLE (
    division_id BIGINT,
    parent_division_id BIGINT,
    division_name VARCHAR,
    division_type VARCHAR,
    level INT
) AS $$
BEGIN
    RETURN QUERY
    WITH RECURSIVE hierarchy_up AS (
        -- Start from the provided Pincode
        SELECT 
            gd.division_id, gd.parent_division_id, gd.division_name, gd.division_type, 0 AS level
        FROM geographical_divisions gd
        WHERE gd.division_type = 'Pincode' AND gd.division_name = p_pincode

        UNION ALL

        -- Recurse upward
        SELECT 
            gd.division_id, gd.parent_division_id, gd.division_name, gd.division_type, h.level - 1 AS level
        FROM geographical_divisions gd
        INNER JOIN hierarchy_up h ON gd.division_id = h.parent_division_id
    ),
    hierarchy_down AS (
        -- Start from the provided Pincode
        SELECT 
            gd.division_id, gd.parent_division_id, gd.division_name, gd.division_type, 0 AS level
        FROM geographical_divisions gd
        WHERE gd.division_type = 'Pincode' AND gd.division_name = p_pincode

        UNION ALL

        -- Recurse downward
        SELECT 
            gd.division_id, gd.parent_division_id, gd.division_name, gd.division_type, h.level + 1 AS level
        FROM geographical_divisions gd
        INNER JOIN hierarchy_down h ON gd.parent_division_id = h.division_id
    )

    SELECT * FROM hierarchy_up
    UNION
    SELECT * FROM hierarchy_down
    ORDER BY level;

END;
$$ LANGUAGE plpgsql;
