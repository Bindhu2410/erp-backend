-- =============================================
-- OrganizationalUnits Table CRUD Stored Procedures
-- Database: PostgreSQL
-- Table: public."OrganizationalUnits"
-- Created: July 3, 2025
-- =============================================

-- =============================================
-- CREATE ORGANIZATIONAL UNIT
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_create_organizational_unit(
    p_unit_name VARCHAR(100),
    p_unit_type VARCHAR(50),
    p_description TEXT DEFAULT NULL,
    p_parent_unit_id INT DEFAULT NULL,
    p_manager_id INT DEFAULT NULL,
    p_is_active BOOLEAN DEFAULT TRUE,
    p_created_by INT DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    unit_id INT
) AS $$
DECLARE
    v_unit_id INT;
BEGIN
    -- Validate parent unit if provided
    IF p_parent_unit_id IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM public."OrganizationalUnits" 
        WHERE "UnitId" = p_parent_unit_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'Parent organizational unit not found'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Validate manager if provided
    IF p_manager_id IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM public."Employee" 
        WHERE "EmployeeID" = p_manager_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'Manager not found'::TEXT, 0;
        RETURN;
    END IF;

    -- Insert the organizational unit
    INSERT INTO public."OrganizationalUnits"(
        "UnitName",
        "UnitType",
        "Description",
        "ParentUnitId",
        "ManagerId",
        "IsActive",
        "DateCreated",
        "CreatedBy"
    ) VALUES (
        p_unit_name,
        p_unit_type,
        p_description,
        p_parent_unit_id,
        p_manager_id,
        p_is_active,
        CURRENT_TIMESTAMP,
        p_created_by
    )
    RETURNING "UnitId" INTO v_unit_id;
    
    RETURN QUERY SELECT TRUE, 'Organizational unit created successfully'::TEXT, v_unit_id;
    
EXCEPTION
    WHEN unique_violation THEN
        RETURN QUERY SELECT FALSE, 'An organizational unit with this name already exists'::TEXT, 0;
    WHEN foreign_key_violation THEN
        RETURN QUERY SELECT FALSE, 'Referenced entity does not exist'::TEXT, 0;
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET ORGANIZATIONAL UNIT BY ID
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_organizational_unit_by_id(
    p_unit_id INT
)
RETURNS TABLE(
    "UnitId" INT,
    "UnitName" VARCHAR(100),
    "UnitType" VARCHAR(50),
    "Description" TEXT,
    "ParentUnitId" INT,
    "ParentUnitName" VARCHAR(100),
    "ManagerId" INT,
    "ManagerName" VARCHAR(200),
    "IsActive" BOOLEAN,
    "DateCreated" TIMESTAMP,
    "CreatedBy" INT,
    "CreatedByUsername" VARCHAR(50),
    "ChildCount" BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ou."UnitId",
        ou."UnitName",
        ou."UnitType",
        ou."Description",
        ou."ParentUnitId",
        parent."UnitName" AS "ParentUnitName",
        ou."ManagerId",
        emp."FirstName" || ' ' || emp."LastName" AS "ManagerName",
        ou."IsActive",
        ou."DateCreated",
        ou."CreatedBy",
        u.username AS "CreatedByUsername",
        (SELECT COUNT(*) FROM public."OrganizationalUnits" child WHERE child."ParentUnitId" = ou."UnitId") AS "ChildCount"
    FROM public."OrganizationalUnits" ou
    LEFT JOIN public."OrganizationalUnits" parent ON ou."ParentUnitId" = parent."UnitId"
    LEFT JOIN public."Employee" emp ON ou."ManagerId" = emp."EmployeeID"
    LEFT JOIN public.users u ON ou."CreatedBy" = u.userid
    WHERE ou."UnitId" = p_unit_id;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- UPDATE ORGANIZATIONAL UNIT
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_update_organizational_unit(
    p_unit_id INT,
    p_unit_name VARCHAR(100),
    p_unit_type VARCHAR(50),
    p_description TEXT DEFAULT NULL,
    p_parent_unit_id INT DEFAULT NULL,
    p_manager_id INT DEFAULT NULL,
    p_is_active BOOLEAN DEFAULT TRUE
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if organizational unit exists
    IF NOT EXISTS (SELECT 1 FROM public."OrganizationalUnits" WHERE "UnitId" = p_unit_id) THEN
        RETURN QUERY SELECT FALSE, 'Organizational unit not found'::TEXT;
        RETURN;
    END IF;
    
    -- Check if trying to set itself as parent
    IF p_unit_id = p_parent_unit_id THEN
        RETURN QUERY SELECT FALSE, 'Cannot set unit as its own parent'::TEXT;
        RETURN;
    END IF;
    
    -- Check if the new parent would create a circular reference
    IF p_parent_unit_id IS NOT NULL AND EXISTS (
        WITH RECURSIVE unit_hierarchy AS (
            -- Start with the parent
            SELECT "UnitId", "ParentUnitId" FROM public."OrganizationalUnits" 
            WHERE "UnitId" = p_parent_unit_id
            
            UNION ALL
            
            -- Join with parents recursively
            SELECT ou."UnitId", ou."ParentUnitId" FROM public."OrganizationalUnits" ou
            JOIN unit_hierarchy uh ON ou."UnitId" = uh."ParentUnitId"
        )
        SELECT 1 FROM unit_hierarchy WHERE "UnitId" = p_unit_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'Cannot create circular reference in unit hierarchy'::TEXT;
        RETURN;
    END IF;
    
    -- Validate parent unit if provided
    IF p_parent_unit_id IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM public."OrganizationalUnits" 
        WHERE "UnitId" = p_parent_unit_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'Parent organizational unit not found'::TEXT;
        RETURN;
    END IF;
    
    -- Validate manager if provided
    IF p_manager_id IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM public."Employee" 
        WHERE "EmployeeID" = p_manager_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'Manager not found'::TEXT;
        RETURN;
    END IF;

    -- Update the organizational unit
    UPDATE public."OrganizationalUnits"
    SET 
        "UnitName" = p_unit_name,
        "UnitType" = p_unit_type,
        "Description" = p_description,
        "ParentUnitId" = p_parent_unit_id,
        "ManagerId" = p_manager_id,
        "IsActive" = p_is_active
    WHERE "UnitId" = p_unit_id;
    
    RETURN QUERY SELECT TRUE, 'Organizational unit updated successfully'::TEXT;
    
EXCEPTION
    WHEN unique_violation THEN
        RETURN QUERY SELECT FALSE, 'An organizational unit with this name already exists'::TEXT;
    WHEN foreign_key_violation THEN
        RETURN QUERY SELECT FALSE, 'Referenced entity does not exist'::TEXT;
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- DELETE ORGANIZATIONAL UNIT
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_delete_organizational_unit(
    p_unit_id INT
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if organizational unit exists
    IF NOT EXISTS (SELECT 1 FROM public."OrganizationalUnits" WHERE "UnitId" = p_unit_id) THEN
        RETURN QUERY SELECT FALSE, 'Organizational unit not found'::TEXT;
        RETURN;
    END IF;
    
    -- Check if unit has children
    IF EXISTS (SELECT 1 FROM public."OrganizationalUnits" WHERE "ParentUnitId" = p_unit_id) THEN
        RETURN QUERY SELECT FALSE, 'Cannot delete unit with child units. Please reassign or delete child units first.'::TEXT;
        RETURN;
    END IF;
    
    -- Check if unit has associated employees or other dependencies
    -- This check should be customized based on other tables that might reference this unit
    -- For example:
    -- IF EXISTS (SELECT 1 FROM public."Employee" WHERE "UnitId" = p_unit_id) THEN
    --     RETURN QUERY SELECT FALSE, 'Cannot delete unit with associated employees. Please reassign employees first.'::TEXT;
    --     RETURN;
    -- END IF;

    -- Delete the organizational unit
    DELETE FROM public."OrganizationalUnits"
    WHERE "UnitId" = p_unit_id;
    
    RETURN QUERY SELECT TRUE, 'Organizational unit deleted successfully'::TEXT;
    
EXCEPTION
    WHEN foreign_key_violation THEN
        RETURN QUERY SELECT FALSE, 'Cannot delete unit as it is referenced by other records'::TEXT;
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- SET ORGANIZATIONAL UNIT ACTIVE STATUS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_set_organizational_unit_status(
    p_unit_id INT,
    p_is_active BOOLEAN
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if organizational unit exists
    IF NOT EXISTS (SELECT 1 FROM public."OrganizationalUnits" WHERE "UnitId" = p_unit_id) THEN
        RETURN QUERY SELECT FALSE, 'Organizational unit not found'::TEXT;
        RETURN;
    END IF;

    -- Update the organizational unit status
    UPDATE public."OrganizationalUnits"
    SET "IsActive" = p_is_active
    WHERE "UnitId" = p_unit_id;
    
    IF p_is_active THEN
        RETURN QUERY SELECT TRUE, 'Organizational unit activated successfully'::TEXT;
    ELSE
        RETURN QUERY SELECT TRUE, 'Organizational unit deactivated successfully'::TEXT;
    END IF;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET ALL ORGANIZATIONAL UNITS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_all_organizational_units(
    p_is_active BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    "UnitId" INT,
    "UnitName" VARCHAR(100),
    "UnitType" VARCHAR(50),
    "Description" TEXT,
    "ParentUnitId" INT,
    "ParentUnitName" VARCHAR(100),
    "ManagerId" INT,
    "ManagerName" VARCHAR(200),
    "IsActive" BOOLEAN,
    "DateCreated" TIMESTAMP,
    "CreatedBy" INT,
    "CreatedByUsername" VARCHAR(50),
    "ChildCount" BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ou."UnitId",
        ou."UnitName",
        ou."UnitType",
        ou."Description",
        ou."ParentUnitId",
        parent."UnitName" AS "ParentUnitName",
        ou."ManagerId",
        emp."FirstName" || ' ' || emp."LastName" AS "ManagerName",
        ou."IsActive",
        ou."DateCreated",
        ou."CreatedBy",
        u.username AS "CreatedByUsername",
        (SELECT COUNT(*) FROM public."OrganizationalUnits" child WHERE child."ParentUnitId" = ou."UnitId") AS "ChildCount"
    FROM public."OrganizationalUnits" ou
    LEFT JOIN public."OrganizationalUnits" parent ON ou."ParentUnitId" = parent."UnitId"
    LEFT JOIN public."Employee" emp ON ou."ManagerId" = emp."EmployeeID"
    LEFT JOIN public.users u ON ou."CreatedBy" = u.userid
    WHERE (p_is_active IS NULL OR ou."IsActive" = p_is_active)
    ORDER BY ou."UnitName";
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET ORGANIZATIONAL UNITS WITH PAGINATION
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_organizational_units_paginated(
    p_page_number INT DEFAULT 1,
    p_page_size INT DEFAULT 10,
    p_search_term VARCHAR(100) DEFAULT NULL,
    p_unit_type VARCHAR(50) DEFAULT NULL,
    p_is_active BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    "UnitId" INT,
    "UnitName" VARCHAR(100),
    "UnitType" VARCHAR(50),
    "Description" TEXT,
    "ParentUnitId" INT,
    "ParentUnitName" VARCHAR(100),
    "ManagerId" INT,
    "ManagerName" VARCHAR(200),
    "IsActive" BOOLEAN,
    "DateCreated" TIMESTAMP,
    "CreatedBy" INT,
    "CreatedByUsername" VARCHAR(50),
    "ChildCount" BIGINT,
    "TotalCount" BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ou."UnitId",
        ou."UnitName",
        ou."UnitType",
        ou."Description",
        ou."ParentUnitId",
        parent."UnitName" AS "ParentUnitName",
        ou."ManagerId",
        emp."FirstName" || ' ' || emp."LastName" AS "ManagerName",
        ou."IsActive",
        ou."DateCreated",
        ou."CreatedBy",
        u.username AS "CreatedByUsername",
        (SELECT COUNT(*) FROM public."OrganizationalUnits" child WHERE child."ParentUnitId" = ou."UnitId") AS "ChildCount",
        COUNT(*) OVER() AS "TotalCount"
    FROM public."OrganizationalUnits" ou
    LEFT JOIN public."OrganizationalUnits" parent ON ou."ParentUnitId" = parent."UnitId"
    LEFT JOIN public."Employee" emp ON ou."ManagerId" = emp."EmployeeID"
    LEFT JOIN public.users u ON ou."CreatedBy" = u.userid
    WHERE (p_is_active IS NULL OR ou."IsActive" = p_is_active)
      AND (p_unit_type IS NULL OR ou."UnitType" = p_unit_type)
      AND (p_search_term IS NULL OR 
           ou."UnitName" ILIKE '%' || p_search_term || '%' OR
           ou."Description" ILIKE '%' || p_search_term || '%')
    ORDER BY ou."UnitName"
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET CHILD UNITS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_child_units(
    p_parent_unit_id INT,
    p_is_active BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    "UnitId" INT,
    "UnitName" VARCHAR(100),
    "UnitType" VARCHAR(50),
    "Description" TEXT,
    "ParentUnitId" INT,
    "ManagerId" INT,
    "ManagerName" VARCHAR(200),
    "IsActive" BOOLEAN,
    "DateCreated" TIMESTAMP,
    "ChildCount" BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ou."UnitId",
        ou."UnitName",
        ou."UnitType",
        ou."Description",
        ou."ParentUnitId",
        ou."ManagerId",
        emp."FirstName" || ' ' || emp."LastName" AS "ManagerName",
        ou."IsActive",
        ou."DateCreated",
        (SELECT COUNT(*) FROM public."OrganizationalUnits" child WHERE child."ParentUnitId" = ou."UnitId") AS "ChildCount"
    FROM public."OrganizationalUnits" ou
    LEFT JOIN public."Employee" emp ON ou."ManagerId" = emp."EmployeeID"
    WHERE ou."ParentUnitId" = p_parent_unit_id
      AND (p_is_active IS NULL OR ou."IsActive" = p_is_active)
    ORDER BY ou."UnitName";
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET TOP-LEVEL UNITS (units without a parent)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_top_level_units(
    p_is_active BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    "UnitId" INT,
    "UnitName" VARCHAR(100),
    "UnitType" VARCHAR(50),
    "Description" TEXT,
    "ManagerId" INT,
    "ManagerName" VARCHAR(200),
    "IsActive" BOOLEAN,
    "DateCreated" TIMESTAMP,
    "ChildCount" BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ou."UnitId",
        ou."UnitName",
        ou."UnitType",
        ou."Description",
        ou."ManagerId",
        emp."FirstName" || ' ' || emp."LastName" AS "ManagerName",
        ou."IsActive",
        ou."DateCreated",
        (SELECT COUNT(*) FROM public."OrganizationalUnits" child WHERE child."ParentUnitId" = ou."UnitId") AS "ChildCount"
    FROM public."OrganizationalUnits" ou
    LEFT JOIN public."Employee" emp ON ou."ManagerId" = emp."EmployeeID"
    WHERE ou."ParentUnitId" IS NULL
      AND (p_is_active IS NULL OR ou."IsActive" = p_is_active)
    ORDER BY ou."UnitName";
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET ORGANIZATIONAL UNIT HIERARCHY (recursive)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_unit_hierarchy(
    p_unit_id INT DEFAULT NULL,
    p_is_active BOOLEAN DEFAULT TRUE
)
RETURNS TABLE(
    "UnitId" INT,
    "UnitName" VARCHAR(100),
    "UnitType" VARCHAR(50),
    "ParentUnitId" INT,
    "ParentUnitName" VARCHAR(100),
    "IsActive" BOOLEAN,
    "Level" INT,
    "Path" TEXT,
    "ChildCount" BIGINT
) AS $$
BEGIN
    RETURN QUERY
    WITH RECURSIVE unit_hierarchy AS (
        -- Base case: top level units or specified unit
        SELECT 
            ou."UnitId",
            ou."UnitName",
            ou."UnitType",
            ou."ParentUnitId",
            parent."UnitName" AS "ParentUnitName",
            ou."IsActive",
            1 AS "Level",
            ou."UnitName"::TEXT AS "Path",
            (SELECT COUNT(*) FROM public."OrganizationalUnits" child WHERE child."ParentUnitId" = ou."UnitId") AS "ChildCount"
        FROM public."OrganizationalUnits" ou
        LEFT JOIN public."OrganizationalUnits" parent ON ou."ParentUnitId" = parent."UnitId"
        WHERE (p_unit_id IS NULL AND ou."ParentUnitId" IS NULL)
           OR (p_unit_id IS NOT NULL AND ou."UnitId" = p_unit_id)
           AND (p_is_active IS NULL OR ou."IsActive" = p_is_active)
        
        UNION ALL
        
        -- Recursive case: child units
        SELECT 
            ou."UnitId",
            ou."UnitName",
            ou."UnitType",
            ou."ParentUnitId",
            parent."UnitName" AS "ParentUnitName",
            ou."IsActive",
            h."Level" + 1,
            h."Path" || ' > ' || ou."UnitName",
            (SELECT COUNT(*) FROM public."OrganizationalUnits" child WHERE child."ParentUnitId" = ou."UnitId") AS "ChildCount"
        FROM public."OrganizationalUnits" ou
        JOIN unit_hierarchy h ON h."UnitId" = ou."ParentUnitId"
        LEFT JOIN public."OrganizationalUnits" parent ON ou."ParentUnitId" = parent."UnitId"
        WHERE (p_is_active IS NULL OR ou."IsActive" = p_is_active)
    )
    SELECT 
        "UnitId", 
        "UnitName", 
        "UnitType", 
        "ParentUnitId", 
        "ParentUnitName", 
        "IsActive", 
        "Level", 
        "Path", 
        "ChildCount" 
    FROM unit_hierarchy
    ORDER BY "Path";
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- SEARCH ORGANIZATIONAL UNITS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_search_organizational_units(
    p_search_term VARCHAR(100),
    p_unit_type VARCHAR(50) DEFAULT NULL,
    p_is_active BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    "UnitId" INT,
    "UnitName" VARCHAR(100),
    "UnitType" VARCHAR(50),
    "Description" TEXT,
    "ParentUnitId" INT,
    "ParentUnitName" VARCHAR(100),
    "IsActive" BOOLEAN,
    "MatchType" VARCHAR(20)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ou."UnitId",
        ou."UnitName",
        ou."UnitType",
        ou."Description",
        ou."ParentUnitId",
        parent."UnitName" AS "ParentUnitName",
        ou."IsActive",
        'Name Match'::VARCHAR(20) AS "MatchType"
    FROM public."OrganizationalUnits" ou
    LEFT JOIN public."OrganizationalUnits" parent ON ou."ParentUnitId" = parent."UnitId"
    WHERE (p_is_active IS NULL OR ou."IsActive" = p_is_active)
      AND (p_unit_type IS NULL OR ou."UnitType" = p_unit_type)
      AND ou."UnitName" ILIKE '%' || p_search_term || '%'
    
    UNION
    
    SELECT 
        ou."UnitId",
        ou."UnitName",
        ou."UnitType",
        ou."Description",
        ou."ParentUnitId",
        parent."UnitName" AS "ParentUnitName",
        ou."IsActive",
        'Description Match'::VARCHAR(20) AS "MatchType"
    FROM public."OrganizationalUnits" ou
    LEFT JOIN public."OrganizationalUnits" parent ON ou."ParentUnitId" = parent."UnitId"
    WHERE (p_is_active IS NULL OR ou."IsActive" = p_is_active)
      AND (p_unit_type IS NULL OR ou."UnitType" = p_unit_type)
      AND ou."UnitName" NOT ILIKE '%' || p_search_term || '%'
      AND ou."Description" ILIKE '%' || p_search_term || '%'
    
    ORDER BY "MatchType", "UnitName";
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET UNITS BY MANAGER
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_units_by_manager(
    p_manager_id INT,
    p_is_active BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    "UnitId" INT,
    "UnitName" VARCHAR(100),
    "UnitType" VARCHAR(50),
    "Description" TEXT,
    "ParentUnitId" INT,
    "ParentUnitName" VARCHAR(100),
    "IsActive" BOOLEAN,
    "DateCreated" TIMESTAMP,
    "ChildCount" BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ou."UnitId",
        ou."UnitName",
        ou."UnitType",
        ou."Description",
        ou."ParentUnitId",
        parent."UnitName" AS "ParentUnitName",
        ou."IsActive",
        ou."DateCreated",
        (SELECT COUNT(*) FROM public."OrganizationalUnits" child WHERE child."ParentUnitId" = ou."UnitId") AS "ChildCount"
    FROM public."OrganizationalUnits" ou
    LEFT JOIN public."OrganizationalUnits" parent ON ou."ParentUnitId" = parent."UnitId"
    WHERE ou."ManagerId" = p_manager_id
      AND (p_is_active IS NULL OR ou."IsActive" = p_is_active)
    ORDER BY ou."UnitName";
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET ORGANIZATIONAL UNIT TYPES
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_unit_types()
RETURNS TABLE(
    "UnitType" VARCHAR(50),
    "Count" BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        "UnitType",
        COUNT(*) AS "Count"
    FROM public."OrganizationalUnits"
    GROUP BY "UnitType"
    ORDER BY "Count" DESC;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET ORGANIZATIONAL UNIT STATISTICS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_organizational_unit_statistics()
RETURNS TABLE(
    total_units BIGINT,
    active_units BIGINT,
    inactive_units BIGINT,
    top_level_units BIGINT,
    total_unit_types BIGINT,
    max_hierarchy_depth INT
) AS $$
DECLARE
    v_max_depth INT;
BEGIN
    -- Get max hierarchy depth
    WITH RECURSIVE unit_hierarchy AS (
        -- Base case: top level units
        SELECT 
            "UnitId",
            "ParentUnitId",
            1 AS depth
        FROM public."OrganizationalUnits"
        WHERE "ParentUnitId" IS NULL
        
        UNION ALL
        
        -- Recursive case: child units
        SELECT 
            ou."UnitId",
            ou."ParentUnitId",
            h.depth + 1
        FROM public."OrganizationalUnits" ou
        JOIN unit_hierarchy h ON h."UnitId" = ou."ParentUnitId"
    )
    SELECT MAX(depth) INTO v_max_depth FROM unit_hierarchy;

    RETURN QUERY
    SELECT 
        COUNT(*)::BIGINT AS total_units,
        COUNT(*) FILTER (WHERE "IsActive" = TRUE)::BIGINT AS active_units,
        COUNT(*) FILTER (WHERE "IsActive" = FALSE)::BIGINT AS inactive_units,
        COUNT(*) FILTER (WHERE "ParentUnitId" IS NULL)::BIGINT AS top_level_units,
        COUNT(DISTINCT "UnitType")::BIGINT AS total_unit_types,
        COALESCE(v_max_depth, 0) AS max_hierarchy_depth
    FROM public."OrganizationalUnits";
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- ASSIGN MANAGER TO UNIT
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_assign_manager_to_unit(
    p_unit_id INT,
    p_manager_id INT
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if organizational unit exists
    IF NOT EXISTS (SELECT 1 FROM public."OrganizationalUnits" WHERE "UnitId" = p_unit_id) THEN
        RETURN QUERY SELECT FALSE, 'Organizational unit not found'::TEXT;
        RETURN;
    END IF;
    
    -- Check if manager exists
    IF NOT EXISTS (SELECT 1 FROM public."Employee" WHERE "EmployeeID" = p_manager_id) THEN
        RETURN QUERY SELECT FALSE, 'Manager not found'::TEXT;
        RETURN;
    END IF;

    -- Update the organizational unit manager
    UPDATE public."OrganizationalUnits"
    SET "ManagerId" = p_manager_id
    WHERE "UnitId" = p_unit_id;
    
    RETURN QUERY SELECT TRUE, 'Manager assigned to unit successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- MOVE UNIT (CHANGE PARENT)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_move_organizational_unit(
    p_unit_id INT,
    p_new_parent_id INT DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if organizational unit exists
    IF NOT EXISTS (SELECT 1 FROM public."OrganizationalUnits" WHERE "UnitId" = p_unit_id) THEN
        RETURN QUERY SELECT FALSE, 'Organizational unit not found'::TEXT;
        RETURN;
    END IF;
    
    -- Check if trying to set itself as parent
    IF p_unit_id = p_new_parent_id THEN
        RETURN QUERY SELECT FALSE, 'Cannot set unit as its own parent'::TEXT;
        RETURN;
    END IF;
    
    -- Validate new parent if provided
    IF p_new_parent_id IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM public."OrganizationalUnits" 
        WHERE "UnitId" = p_new_parent_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'Parent organizational unit not found'::TEXT;
        RETURN;
    END IF;
    
    -- Check if the new parent would create a circular reference
    IF p_new_parent_id IS NOT NULL AND EXISTS (
        WITH RECURSIVE unit_hierarchy AS (
            -- Start with this unit
            SELECT "UnitId", "ParentUnitId" FROM public."OrganizationalUnits" 
            WHERE "UnitId" = p_unit_id
            
            UNION ALL
            
            -- Join with children recursively
            SELECT ou."UnitId", ou."ParentUnitId" FROM public."OrganizationalUnits" ou
            JOIN unit_hierarchy uh ON uh."UnitId" = ou."ParentUnitId"
        )
        SELECT 1 FROM unit_hierarchy WHERE "UnitId" = p_new_parent_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'Cannot create circular reference in unit hierarchy'::TEXT;
        RETURN;
    END IF;

    -- Update the organizational unit parent
    UPDATE public."OrganizationalUnits"
    SET "ParentUnitId" = p_new_parent_id
    WHERE "UnitId" = p_unit_id;
    
    IF p_new_parent_id IS NULL THEN
        RETURN QUERY SELECT TRUE, 'Unit moved to top level successfully'::TEXT;
    ELSE
        RETURN QUERY SELECT TRUE, 'Unit moved to new parent successfully'::TEXT;
    END IF;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Example Usage Comments
-- =============================================

/*
-- Create a new organizational unit
SELECT * FROM sp_um_create_organizational_unit('Sales Department', 'Department', 'Global Sales Operations', NULL, 1, TRUE, 1);

-- Get organizational unit by ID
SELECT * FROM sp_um_get_organizational_unit_by_id(1);

-- Update an organizational unit
SELECT * FROM sp_um_update_organizational_unit(1, 'Sales & Marketing', 'Department', 'Global Sales and Marketing Operations', NULL, 2, TRUE);

-- Delete an organizational unit
SELECT * FROM sp_um_delete_organizational_unit(1);

-- Set active status
SELECT * FROM sp_um_set_organizational_unit_status(1, FALSE);

-- Get all organizational units
SELECT * FROM sp_um_get_all_organizational_units();

-- Get organizational units with pagination
SELECT * FROM sp_um_get_organizational_units_paginated(1, 10, 'Sales', 'Department', TRUE);

-- Get child units
SELECT * FROM sp_um_get_child_units(1);

-- Get top-level units
SELECT * FROM sp_um_get_top_level_units();

-- Get unit hierarchy
SELECT * FROM sp_um_get_unit_hierarchy();

-- Search organizational units
SELECT * FROM sp_um_search_organizational_units('Sales');

-- Get units by manager
SELECT * FROM sp_um_get_units_by_manager(1);

-- Get unit types
SELECT * FROM sp_um_get_unit_types();

-- Get organizational unit statistics
SELECT * FROM sp_um_get_organizational_unit_statistics();

-- Assign manager to unit
SELECT * FROM sp_um_assign_manager_to_unit(1, 2);

-- Move organizational unit
SELECT * FROM sp_um_move_organizational_unit(2, 1);
*/
