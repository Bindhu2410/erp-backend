-- =============================================
-- UserOrganizationalUnits Table CRUD Stored Procedures
-- Database: PostgreSQL
-- Table: public."UserOrganizationalUnits"
-- Created: July 3, 2025
-- =============================================

-- =============================================
-- ASSIGN USER TO ORGANIZATIONAL UNIT
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_assign_user_to_unit(
    p_user_id INT,
    p_unit_id INT,
    p_is_primary BOOLEAN DEFAULT TRUE,
    p_assigned_by INT DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Validate user
    IF NOT EXISTS (
        SELECT 1 FROM public.users 
        WHERE userid = p_user_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT;
        RETURN;
    END IF;
    
    -- Validate organizational unit
    IF NOT EXISTS (
        SELECT 1 FROM public."OrganizationalUnits" 
        WHERE "UnitId" = p_unit_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'Organizational unit not found'::TEXT;
        RETURN;
    END IF;
    
    -- Check if assignment already exists
    IF EXISTS (
        SELECT 1 FROM public."UserOrganizationalUnits" 
        WHERE "UserId" = p_user_id AND "UnitId" = p_unit_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'User is already assigned to this unit'::TEXT;
        RETURN;
    END IF;

    -- If this is marked as primary, update any existing primary assignments to false
    IF p_is_primary THEN
        UPDATE public."UserOrganizationalUnits"
        SET "IsPrimary" = FALSE
        WHERE "UserId" = p_user_id AND "IsPrimary" = TRUE;
    END IF;

    -- Assign user to organizational unit
    INSERT INTO public."UserOrganizationalUnits"(
        "UserId",
        "UnitId",
        "IsPrimary",
        "DateAssigned",
        "AssignedBy"
    ) VALUES (
        p_user_id,
        p_unit_id,
        p_is_primary,
        CURRENT_TIMESTAMP,
        p_assigned_by
    );
    
    RETURN QUERY SELECT TRUE, 'User successfully assigned to organizational unit'::TEXT;
    
EXCEPTION
    WHEN unique_violation THEN
        RETURN QUERY SELECT FALSE, 'User is already assigned to this unit'::TEXT;
    WHEN foreign_key_violation THEN
        RETURN QUERY SELECT FALSE, 'Referenced entity does not exist'::TEXT;
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- ASSIGN USER TO MULTIPLE ORGANIZATIONAL UNITS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_assign_user_to_units(
    p_user_id INT,
    p_unit_ids INT[],
    p_primary_unit_id INT DEFAULT NULL,
    p_assigned_by INT DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    assigned_count INT,
    failed_count INT
) AS $$
DECLARE
    v_unit_id INT;
    v_assigned_count INT := 0;
    v_failed_count INT := 0;
    v_result RECORD;
BEGIN
    -- Validate user
    IF NOT EXISTS (
        SELECT 1 FROM public.users 
        WHERE userid = p_user_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT, 0, array_length(p_unit_ids, 1);
        RETURN;
    END IF;

    -- Validate primary unit if provided
    IF p_primary_unit_id IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM public."OrganizationalUnits" 
        WHERE "UnitId" = p_primary_unit_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'Primary unit not found'::TEXT, 0, array_length(p_unit_ids, 1);
        RETURN;
    END IF;

    -- Reset primary flag if a primary unit is specified
    IF p_primary_unit_id IS NOT NULL THEN
        UPDATE public."UserOrganizationalUnits"
        SET "IsPrimary" = FALSE
        WHERE "UserId" = p_user_id AND "IsPrimary" = TRUE;
    END IF;

    -- Assign user to each unit
    FOREACH v_unit_id IN ARRAY p_unit_ids
    LOOP
        -- Check if unit exists
        IF NOT EXISTS (
            SELECT 1 FROM public."OrganizationalUnits" 
            WHERE "UnitId" = v_unit_id
        ) THEN
            v_failed_count := v_failed_count + 1;
            CONTINUE;
        END IF;

        -- Skip if already assigned
        IF EXISTS (
            SELECT 1 FROM public."UserOrganizationalUnits" 
            WHERE "UserId" = p_user_id AND "UnitId" = v_unit_id
        ) THEN
            v_failed_count := v_failed_count + 1;
            CONTINUE;
        END IF;

        -- Add assignment
        BEGIN
            INSERT INTO public."UserOrganizationalUnits"(
                "UserId",
                "UnitId",
                "IsPrimary",
                "DateAssigned",
                "AssignedBy"
            ) VALUES (
                p_user_id,
                v_unit_id,
                CASE WHEN v_unit_id = p_primary_unit_id THEN TRUE ELSE FALSE END,
                CURRENT_TIMESTAMP,
                p_assigned_by
            );
            v_assigned_count := v_assigned_count + 1;
        EXCEPTION
            WHEN OTHERS THEN
                v_failed_count := v_failed_count + 1;
        END;
    END LOOP;

    -- Return results
    IF v_assigned_count > 0 THEN
        RETURN QUERY SELECT 
            TRUE, 
            format('User successfully assigned to %s organizational units. %s assignments failed.', v_assigned_count, v_failed_count)::TEXT, 
            v_assigned_count, 
            v_failed_count;
    ELSE
        RETURN QUERY SELECT 
            FALSE, 
            'Failed to assign user to any organizational units'::TEXT, 
            v_assigned_count, 
            v_failed_count;
    END IF;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0, array_length(p_unit_ids, 1);
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- ASSIGN MULTIPLE USERS TO ORGANIZATIONAL UNIT
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_assign_users_to_unit(
    p_user_ids INT[],
    p_unit_id INT,
    p_as_primary BOOLEAN DEFAULT FALSE,
    p_assigned_by INT DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    assigned_count INT,
    failed_count INT
) AS $$
DECLARE
    v_user_id INT;
    v_assigned_count INT := 0;
    v_failed_count INT := 0;
BEGIN
    -- Validate organizational unit
    IF NOT EXISTS (
        SELECT 1 FROM public."OrganizationalUnits" 
        WHERE "UnitId" = p_unit_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'Organizational unit not found'::TEXT, 0, array_length(p_user_ids, 1);
        RETURN;
    END IF;

    -- Assign each user to the unit
    FOREACH v_user_id IN ARRAY p_user_ids
    LOOP
        -- Check if user exists
        IF NOT EXISTS (
            SELECT 1 FROM public.users 
            WHERE userid = v_user_id
        ) THEN
            v_failed_count := v_failed_count + 1;
            CONTINUE;
        END IF;

        -- Skip if already assigned
        IF EXISTS (
            SELECT 1 FROM public."UserOrganizationalUnits" 
            WHERE "UserId" = v_user_id AND "UnitId" = p_unit_id
        ) THEN
            v_failed_count := v_failed_count + 1;
            CONTINUE;
        END IF;

        -- If this will be primary, update existing primary assignments
        IF p_as_primary THEN
            UPDATE public."UserOrganizationalUnits"
            SET "IsPrimary" = FALSE
            WHERE "UserId" = v_user_id AND "IsPrimary" = TRUE;
        END IF;

        -- Add assignment
        BEGIN
            INSERT INTO public."UserOrganizationalUnits"(
                "UserId",
                "UnitId",
                "IsPrimary",
                "DateAssigned",
                "AssignedBy"
            ) VALUES (
                v_user_id,
                p_unit_id,
                p_as_primary,
                CURRENT_TIMESTAMP,
                p_assigned_by
            );
            v_assigned_count := v_assigned_count + 1;
        EXCEPTION
            WHEN OTHERS THEN
                v_failed_count := v_failed_count + 1;
        END;
    END LOOP;

    -- Return results
    IF v_assigned_count > 0 THEN
        RETURN QUERY SELECT 
            TRUE, 
            format('%s users successfully assigned to organizational unit. %s assignments failed.', v_assigned_count, v_failed_count)::TEXT, 
            v_assigned_count, 
            v_failed_count;
    ELSE
        RETURN QUERY SELECT 
            FALSE, 
            'Failed to assign any users to organizational unit'::TEXT, 
            v_assigned_count, 
            v_failed_count;
    END IF;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0, array_length(p_user_ids, 1);
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- REMOVE USER FROM ORGANIZATIONAL UNIT
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_remove_user_from_unit(
    p_user_id INT,
    p_unit_id INT
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    was_primary BOOLEAN
) AS $$
DECLARE
    v_was_primary BOOLEAN;
BEGIN
    -- Check if assignment exists
    IF NOT EXISTS (
        SELECT 1 FROM public."UserOrganizationalUnits" 
        WHERE "UserId" = p_user_id AND "UnitId" = p_unit_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'User is not assigned to this unit'::TEXT, FALSE;
        RETURN;
    END IF;

    -- Check if this was a primary unit
    SELECT "IsPrimary" INTO v_was_primary
    FROM public."UserOrganizationalUnits"
    WHERE "UserId" = p_user_id AND "UnitId" = p_unit_id;

    -- Remove assignment
    DELETE FROM public."UserOrganizationalUnits"
    WHERE "UserId" = p_user_id AND "UnitId" = p_unit_id;
    
    RETURN QUERY SELECT TRUE, 'User successfully removed from organizational unit'::TEXT, v_was_primary;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, FALSE;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- REMOVE USER FROM ALL ORGANIZATIONAL UNITS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_remove_user_from_all_units(
    p_user_id INT
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    removed_count INT
) AS $$
DECLARE
    v_removed_count INT;
BEGIN
    -- Validate user
    IF NOT EXISTS (
        SELECT 1 FROM public.users 
        WHERE userid = p_user_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT, 0;
        RETURN;
    END IF;

    -- Remove all assignments
    WITH deleted AS (
        DELETE FROM public."UserOrganizationalUnits"
        WHERE "UserId" = p_user_id
        RETURNING *
    )
    SELECT COUNT(*) INTO v_removed_count FROM deleted;

    IF v_removed_count > 0 THEN
        RETURN QUERY SELECT TRUE, format('User successfully removed from %s organizational units', v_removed_count)::TEXT, v_removed_count;
    ELSE
        RETURN QUERY SELECT FALSE, 'User was not assigned to any organizational units'::TEXT, 0;
    END IF;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- REMOVE ALL USERS FROM ORGANIZATIONAL UNIT
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_remove_all_users_from_unit(
    p_unit_id INT
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    removed_count INT
) AS $$
DECLARE
    v_removed_count INT;
BEGIN
    -- Validate organizational unit
    IF NOT EXISTS (
        SELECT 1 FROM public."OrganizationalUnits" 
        WHERE "UnitId" = p_unit_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'Organizational unit not found'::TEXT, 0;
        RETURN;
    END IF;

    -- Remove all assignments
    WITH deleted AS (
        DELETE FROM public."UserOrganizationalUnits"
        WHERE "UnitId" = p_unit_id
        RETURNING *
    )
    SELECT COUNT(*) INTO v_removed_count FROM deleted;

    IF v_removed_count > 0 THEN
        RETURN QUERY SELECT TRUE, format('%s users successfully removed from organizational unit', v_removed_count)::TEXT, v_removed_count;
    ELSE
        RETURN QUERY SELECT FALSE, 'No users were assigned to this organizational unit'::TEXT, 0;
    END IF;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- SET PRIMARY ORGANIZATIONAL UNIT FOR USER
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_set_primary_unit_for_user(
    p_user_id INT,
    p_unit_id INT
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Validate user
    IF NOT EXISTS (
        SELECT 1 FROM public.users 
        WHERE userid = p_user_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT;
        RETURN;
    END IF;
    
    -- Validate organizational unit
    IF NOT EXISTS (
        SELECT 1 FROM public."OrganizationalUnits" 
        WHERE "UnitId" = p_unit_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'Organizational unit not found'::TEXT;
        RETURN;
    END IF;

    -- Check if the user is assigned to this unit
    IF NOT EXISTS (
        SELECT 1 FROM public."UserOrganizationalUnits" 
        WHERE "UserId" = p_user_id AND "UnitId" = p_unit_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'User is not assigned to this unit'::TEXT;
        RETURN;
    END IF;

    -- Update all assignments to non-primary
    UPDATE public."UserOrganizationalUnits"
    SET "IsPrimary" = FALSE
    WHERE "UserId" = p_user_id AND "IsPrimary" = TRUE;

    -- Set the specified unit as primary
    UPDATE public."UserOrganizationalUnits"
    SET "IsPrimary" = TRUE
    WHERE "UserId" = p_user_id AND "UnitId" = p_unit_id;
    
    RETURN QUERY SELECT TRUE, 'Primary organizational unit successfully set for user'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET USER ORGANIZATIONAL UNITS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_user_units(
    p_user_id INT
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
    "IsPrimary" BOOLEAN,
    "DateAssigned" TIMESTAMP,
    "AssignedBy" INT,
    "AssignedByUsername" VARCHAR(50)
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
        uou."IsPrimary",
        uou."DateAssigned",
        uou."AssignedBy",
        assigner.username AS "AssignedByUsername"
    FROM public."UserOrganizationalUnits" uou
    JOIN public."OrganizationalUnits" ou ON uou."UnitId" = ou."UnitId"
    LEFT JOIN public."OrganizationalUnits" parent ON ou."ParentUnitId" = parent."UnitId"
    LEFT JOIN public."Employee" emp ON ou."ManagerId" = emp."EmployeeID"
    LEFT JOIN public.users assigner ON uou."AssignedBy" = assigner.userid
    WHERE uou."UserId" = p_user_id
    ORDER BY uou."IsPrimary" DESC, ou."UnitName";
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET PRIMARY ORGANIZATIONAL UNIT FOR USER
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_user_primary_unit(
    p_user_id INT
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
    "DateAssigned" TIMESTAMP
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
        uou."DateAssigned"
    FROM public."UserOrganizationalUnits" uou
    JOIN public."OrganizationalUnits" ou ON uou."UnitId" = ou."UnitId"
    LEFT JOIN public."OrganizationalUnits" parent ON ou."ParentUnitId" = parent."UnitId"
    LEFT JOIN public."Employee" emp ON ou."ManagerId" = emp."EmployeeID"
    WHERE uou."UserId" = p_user_id AND uou."IsPrimary" = TRUE
    LIMIT 1;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET USERS IN ORGANIZATIONAL UNIT
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_unit_users(
    p_unit_id INT,
    p_only_primary BOOLEAN DEFAULT FALSE
)
RETURNS TABLE(
    "UserId" INT,
    "Username" VARCHAR(50),
    "Email" VARCHAR(100),
    "FullName" VARCHAR(100),
    "IsActive" BOOLEAN,
    "IsPrimary" BOOLEAN,
    "DateAssigned" TIMESTAMP,
    "AssignedBy" INT,
    "AssignedByUsername" VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u.userid AS "UserId",
        u.username AS "Username",
        u.email AS "Email",
        COALESCE(e."FirstName" || ' ' || e."LastName", u.username) AS "FullName",
        u.is_active AS "IsActive",
        uou."IsPrimary",
        uou."DateAssigned",
        uou."AssignedBy",
        assigner.username AS "AssignedByUsername"
    FROM public."UserOrganizationalUnits" uou
    JOIN public.users u ON uou."UserId" = u.userid
    LEFT JOIN public."Employee" e ON u.userid = e."UserId"
    LEFT JOIN public.users assigner ON uou."AssignedBy" = assigner.userid
    WHERE uou."UnitId" = p_unit_id
    AND (p_only_primary = FALSE OR uou."IsPrimary" = TRUE)
    ORDER BY u.username;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET USERS IN ORGANIZATIONAL UNIT WITH PAGINATION
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_unit_users_paginated(
    p_unit_id INT,
    p_page_number INT DEFAULT 1,
    p_page_size INT DEFAULT 10,
    p_search_term VARCHAR(100) DEFAULT NULL,
    p_only_primary BOOLEAN DEFAULT FALSE,
    p_is_active BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    "UserId" INT,
    "Username" VARCHAR(50),
    "Email" VARCHAR(100),
    "FullName" VARCHAR(100),
    "IsActive" BOOLEAN,
    "IsPrimary" BOOLEAN,
    "DateAssigned" TIMESTAMP,
    "AssignedBy" INT,
    "AssignedByUsername" VARCHAR(50),
    "TotalCount" BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u.userid AS "UserId",
        u.username AS "Username",
        u.email AS "Email",
        COALESCE(e."FirstName" || ' ' || e."LastName", u.username) AS "FullName",
        u.is_active AS "IsActive",
        uou."IsPrimary",
        uou."DateAssigned",
        uou."AssignedBy",
        assigner.username AS "AssignedByUsername",
        COUNT(*) OVER() AS "TotalCount"
    FROM public."UserOrganizationalUnits" uou
    JOIN public.users u ON uou."UserId" = u.userid
    LEFT JOIN public."Employee" e ON u.userid = e."UserId"
    LEFT JOIN public.users assigner ON uou."AssignedBy" = assigner.userid
    WHERE uou."UnitId" = p_unit_id
    AND (p_only_primary = FALSE OR uou."IsPrimary" = TRUE)
    AND (p_is_active IS NULL OR u.is_active = p_is_active)
    AND (
        p_search_term IS NULL OR
        u.username ILIKE '%' || p_search_term || '%' OR
        u.email ILIKE '%' || p_search_term || '%' OR
        COALESCE(e."FirstName" || ' ' || e."LastName", '') ILIKE '%' || p_search_term || '%'
    )
    ORDER BY u.username
    LIMIT p_page_size
    OFFSET (p_page_number - 1) * p_page_size;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CHECK IF USER IS IN ORGANIZATIONAL UNIT
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_check_user_in_unit(
    p_user_id INT,
    p_unit_id INT,
    p_check_primary_only BOOLEAN DEFAULT FALSE
)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1
        FROM public."UserOrganizationalUnits"
        WHERE "UserId" = p_user_id 
        AND "UnitId" = p_unit_id
        AND (p_check_primary_only = FALSE OR "IsPrimary" = TRUE)
    );
EXCEPTION
    WHEN OTHERS THEN
        RETURN FALSE;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CHECK IF USER IS IN ANY UNIT
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_check_user_in_any_unit(
    p_user_id INT
)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1
        FROM public."UserOrganizationalUnits"
        WHERE "UserId" = p_user_id
    );
EXCEPTION
    WHEN OTHERS THEN
        RETURN FALSE;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CHECK IF USER IS IN ANY OF THE SPECIFIED UNITS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_check_user_in_any_of_units(
    p_user_id INT,
    p_unit_ids INT[]
)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1
        FROM public."UserOrganizationalUnits"
        WHERE "UserId" = p_user_id
        AND "UnitId" = ANY(p_unit_ids)
    );
EXCEPTION
    WHEN OTHERS THEN
        RETURN FALSE;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET USERS NOT IN ORGANIZATIONAL UNIT
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_users_not_in_unit(
    p_unit_id INT,
    p_is_active BOOLEAN DEFAULT TRUE
)
RETURNS TABLE(
    "UserId" INT,
    "Username" VARCHAR(50),
    "Email" VARCHAR(100),
    "FullName" VARCHAR(100),
    "IsActive" BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u.userid AS "UserId",
        u.username AS "Username",
        u.email AS "Email",
        COALESCE(e."FirstName" || ' ' || e."LastName", u.username) AS "FullName",
        u.is_active AS "IsActive"
    FROM public.users u
    LEFT JOIN public."Employee" e ON u.userid = e."UserId"
    WHERE (p_is_active IS NULL OR u.is_active = p_is_active)
    AND NOT EXISTS (
        SELECT 1 FROM public."UserOrganizationalUnits" uou 
        WHERE uou."UserId" = u.userid AND uou."UnitId" = p_unit_id
    )
    ORDER BY u.username;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET ORGANIZATIONAL UNITS USER IS NOT IN
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_units_user_not_in(
    p_user_id INT,
    p_is_active BOOLEAN DEFAULT TRUE
)
RETURNS TABLE(
    "UnitId" INT,
    "UnitName" VARCHAR(100),
    "UnitType" VARCHAR(50),
    "Description" TEXT,
    "ParentUnitId" INT,
    "ParentUnitName" VARCHAR(100),
    "IsActive" BOOLEAN
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
        ou."IsActive"
    FROM public."OrganizationalUnits" ou
    LEFT JOIN public."OrganizationalUnits" parent ON ou."ParentUnitId" = parent."UnitId"
    WHERE (p_is_active IS NULL OR ou."IsActive" = p_is_active)
    AND NOT EXISTS (
        SELECT 1 FROM public."UserOrganizationalUnits" uou 
        WHERE uou."UserId" = p_user_id AND uou."UnitId" = ou."UnitId"
    )
    ORDER BY ou."UnitName";
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- SYNC USER ORGANIZATIONAL UNITS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_sync_user_units(
    p_user_id INT,
    p_unit_ids INT[],
    p_primary_unit_id INT DEFAULT NULL,
    p_assigned_by INT DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    added_count INT,
    removed_count INT,
    unchanged_count INT
) AS $$
DECLARE
    v_added_count INT := 0;
    v_removed_count INT := 0;
    v_unchanged_count INT := 0;
    v_unit_id INT;
    v_to_remove INT[];
BEGIN
    -- Validate user
    IF NOT EXISTS (
        SELECT 1 FROM public.users 
        WHERE userid = p_user_id
    ) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT, 0, 0, 0;
        RETURN;
    END IF;

    -- Validate primary unit if provided
    IF p_primary_unit_id IS NOT NULL THEN
        IF NOT EXISTS (
            SELECT 1 FROM public."OrganizationalUnits" 
            WHERE "UnitId" = p_primary_unit_id
        ) THEN
            RETURN QUERY SELECT FALSE, 'Primary unit not found'::TEXT, 0, 0, 0;
            RETURN;
        END IF;
        
        -- Make sure primary unit is in the list
        IF NOT p_primary_unit_id = ANY(p_unit_ids) THEN
            RETURN QUERY SELECT FALSE, 'Primary unit must be included in the unit IDs list'::TEXT, 0, 0, 0;
            RETURN;
        END IF;
    END IF;

    -- Find units to remove (current assignments not in new list)
    SELECT array_agg("UnitId")
    INTO v_to_remove
    FROM public."UserOrganizationalUnits"
    WHERE "UserId" = p_user_id
    AND NOT "UnitId" = ANY(COALESCE(p_unit_ids, ARRAY[]::INT[]));

    -- Remove units not in the new list
    IF v_to_remove IS NOT NULL THEN
        WITH removed AS (
            DELETE FROM public."UserOrganizationalUnits"
            WHERE "UserId" = p_user_id
            AND "UnitId" = ANY(v_to_remove)
            RETURNING *
        )
        SELECT COUNT(*) INTO v_removed_count FROM removed;
    END IF;

    -- Reset primary flags if a new primary is specified
    IF p_primary_unit_id IS NOT NULL THEN
        UPDATE public."UserOrganizationalUnits"
        SET "IsPrimary" = FALSE
        WHERE "UserId" = p_user_id 
        AND "IsPrimary" = TRUE
        AND "UnitId" <> p_primary_unit_id;
        
        -- Set the new primary
        UPDATE public."UserOrganizationalUnits"
        SET "IsPrimary" = TRUE
        WHERE "UserId" = p_user_id 
        AND "UnitId" = p_primary_unit_id;
    END IF;

    -- Add new assignments
    FOREACH v_unit_id IN ARRAY p_unit_ids
    LOOP
        -- Skip if unit doesn't exist
        IF NOT EXISTS (
            SELECT 1 FROM public."OrganizationalUnits" 
            WHERE "UnitId" = v_unit_id
        ) THEN
            CONTINUE;
        END IF;

        -- If already assigned, count as unchanged
        IF EXISTS (
            SELECT 1 FROM public."UserOrganizationalUnits" 
            WHERE "UserId" = p_user_id AND "UnitId" = v_unit_id
        ) THEN
            v_unchanged_count := v_unchanged_count + 1;
            CONTINUE;
        END IF;

        -- Add new assignment
        BEGIN
            INSERT INTO public."UserOrganizationalUnits"(
                "UserId",
                "UnitId",
                "IsPrimary",
                "DateAssigned",
                "AssignedBy"
            ) VALUES (
                p_user_id,
                v_unit_id,
                CASE WHEN v_unit_id = p_primary_unit_id THEN TRUE ELSE FALSE END,
                CURRENT_TIMESTAMP,
                p_assigned_by
            );
            v_added_count := v_added_count + 1;
        EXCEPTION
            WHEN OTHERS THEN
                -- Skip failures
                NULL;
        END;
    END LOOP;

    -- Return results
    RETURN QUERY SELECT 
        TRUE, 
        format('User organizational units synchronized: %s added, %s removed, %s unchanged', v_added_count, v_removed_count, v_unchanged_count)::TEXT, 
        v_added_count, 
        v_removed_count, 
        v_unchanged_count;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0, 0, 0;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET USER ORGANIZATIONAL UNIT COUNTS BY TYPE
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_user_unit_counts_by_type(
    p_user_id INT
)
RETURNS TABLE(
    "UnitType" VARCHAR(50),
    "Count" BIGINT,
    "PrimaryCount" BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ou."UnitType",
        COUNT(*) AS "Count",
        COUNT(*) FILTER (WHERE uou."IsPrimary" = TRUE) AS "PrimaryCount"
    FROM public."UserOrganizationalUnits" uou
    JOIN public."OrganizationalUnits" ou ON uou."UnitId" = ou."UnitId"
    WHERE uou."UserId" = p_user_id
    GROUP BY ou."UnitType"
    ORDER BY "Count" DESC;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET USER ORGANIZATIONAL UNIT STATISTICS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_user_organizational_unit_statistics()
RETURNS TABLE(
    total_assignments BIGINT,
    users_with_units BIGINT,
    units_with_users BIGINT,
    avg_units_per_user NUMERIC,
    avg_users_per_unit NUMERIC,
    max_units_for_user BIGINT,
    max_users_for_unit BIGINT,
    users_with_primary_unit BIGINT,
    units_as_primary BIGINT
) AS $$
DECLARE
    v_total_assignments BIGINT;
    v_users_with_units BIGINT;
    v_units_with_users BIGINT;
    v_avg_units_per_user NUMERIC;
    v_avg_users_per_unit NUMERIC;
    v_max_units_for_user BIGINT;
    v_max_users_for_unit BIGINT;
    v_users_with_primary_unit BIGINT;
    v_units_as_primary BIGINT;
BEGIN
    -- Total assignments
    SELECT COUNT(*) INTO v_total_assignments
    FROM public."UserOrganizationalUnits";
    
    -- Users with at least one unit
    SELECT COUNT(DISTINCT "UserId") INTO v_users_with_units
    FROM public."UserOrganizationalUnits";
    
    -- Units with at least one user
    SELECT COUNT(DISTINCT "UnitId") INTO v_units_with_users
    FROM public."UserOrganizationalUnits";
    
    -- Average units per user
    SELECT COALESCE(AVG(unit_count), 0) INTO v_avg_units_per_user
    FROM (
        SELECT "UserId", COUNT(*) as unit_count
        FROM public."UserOrganizationalUnits"
        GROUP BY "UserId"
    ) AS user_counts;
    
    -- Average users per unit
    SELECT COALESCE(AVG(user_count), 0) INTO v_avg_users_per_unit
    FROM (
        SELECT "UnitId", COUNT(*) as user_count
        FROM public."UserOrganizationalUnits"
        GROUP BY "UnitId"
    ) AS unit_counts;
    
    -- Max units for any user
    SELECT COALESCE(MAX(unit_count), 0) INTO v_max_units_for_user
    FROM (
        SELECT "UserId", COUNT(*) as unit_count
        FROM public."UserOrganizationalUnits"
        GROUP BY "UserId"
    ) AS user_counts;
    
    -- Max users for any unit
    SELECT COALESCE(MAX(user_count), 0) INTO v_max_users_for_unit
    FROM (
        SELECT "UnitId", COUNT(*) as user_count
        FROM public."UserOrganizationalUnits"
        GROUP BY "UnitId"
    ) AS unit_counts;
    
    -- Users with primary unit
    SELECT COUNT(DISTINCT "UserId") INTO v_users_with_primary_unit
    FROM public."UserOrganizationalUnits"
    WHERE "IsPrimary" = TRUE;
    
    -- Units that are primary for at least one user
    SELECT COUNT(DISTINCT "UnitId") INTO v_units_as_primary
    FROM public."UserOrganizationalUnits"
    WHERE "IsPrimary" = TRUE;

    RETURN QUERY
    SELECT 
        v_total_assignments,
        v_users_with_units,
        v_units_with_users,
        v_avg_units_per_user,
        v_avg_users_per_unit,
        v_max_units_for_user,
        v_max_users_for_unit,
        v_users_with_primary_unit,
        v_units_as_primary;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Example Usage Comments
-- =============================================

/*
-- Assign a user to an organizational unit
SELECT * FROM sp_um_assign_user_to_unit(1, 5, TRUE, 1);

-- Assign a user to multiple units
SELECT * FROM sp_um_assign_user_to_units(1, ARRAY[1, 2, 3], 1, 1);

-- Assign multiple users to a unit
SELECT * FROM sp_um_assign_users_to_unit(ARRAY[1, 2, 3], 5, FALSE, 1);

-- Remove a user from a unit
SELECT * FROM sp_um_remove_user_from_unit(1, 5);

-- Remove a user from all units
SELECT * FROM sp_um_remove_user_from_all_units(1);

-- Remove all users from a unit
SELECT * FROM sp_um_remove_all_users_from_unit(5);

-- Set primary unit for a user
SELECT * FROM sp_um_set_primary_unit_for_user(1, 3);

-- Get all units for a user
SELECT * FROM sp_um_get_user_units(1);

-- Get primary unit for a user
SELECT * FROM sp_um_get_user_primary_unit(1);

-- Get all users in a unit
SELECT * FROM sp_um_get_unit_users(5, FALSE);

-- Get users in a unit with pagination
SELECT * FROM sp_um_get_unit_users_paginated(5, 1, 10, 'john', FALSE, TRUE);

-- Check if a user is in a unit
SELECT * FROM sp_um_check_user_in_unit(1, 5, FALSE);

-- Check if a user is in any unit
SELECT * FROM sp_um_check_user_in_any_unit(1);

-- Check if a user is in any of the specified units
SELECT * FROM sp_um_check_user_in_any_of_units(1, ARRAY[5, 6, 7]);

-- Get users not in a unit
SELECT * FROM sp_um_get_users_not_in_unit(5, TRUE);

-- Get units a user is not in
SELECT * FROM sp_um_get_units_user_not_in(1, TRUE);

-- Sync user's organizational units
SELECT * FROM sp_um_sync_user_units(1, ARRAY[1, 3, 5], 3, 1);

-- Get unit counts by type for a user
SELECT * FROM sp_um_get_user_unit_counts_by_type(1);

-- Get user organizational unit statistics
SELECT * FROM sp_um_get_user_organizational_unit_statistics();
*/
