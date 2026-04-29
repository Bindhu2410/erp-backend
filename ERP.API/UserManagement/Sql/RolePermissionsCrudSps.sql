-- =============================================
-- RolePermissions Table CRUD Stored Procedures
-- Database: PostgreSQL
-- Table: public.rolepermissions
-- Created: July 3, 2025
-- =============================================

-- =============================================
-- ASSIGN PERMISSION TO ROLE
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_assign_permission_to_role(
    p_roleid INT,
    p_permissionid INT,
    p_assignedby INT DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if role exists
    IF NOT EXISTS (SELECT 1 FROM public.roles WHERE roleid = p_roleid) THEN
        RETURN QUERY SELECT FALSE, 'Role not found'::TEXT;
        RETURN;
    END IF;
    
    -- Check if permission exists
    IF NOT EXISTS (SELECT 1 FROM public.permissions WHERE permissionid = p_permissionid) THEN
        RETURN QUERY SELECT FALSE, 'Permission not found'::TEXT;
        RETURN;
    END IF;
    
    -- Check if assignment already exists
    IF EXISTS (SELECT 1 FROM public.rolepermissions 
               WHERE roleid = p_roleid AND permissionid = p_permissionid) THEN
        RETURN QUERY SELECT FALSE, 'Permission is already assigned to this role'::TEXT;
        RETURN;
    END IF;
    
    -- Assign permission to role
    INSERT INTO public.rolepermissions(roleid, permissionid, dateassigned, assignedby)
    VALUES (p_roleid, p_permissionid, CURRENT_TIMESTAMP, p_assignedby);
    
    RETURN QUERY SELECT TRUE, 'Permission assigned to role successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- ASSIGN MULTIPLE PERMISSIONS TO ROLE
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_assign_permissions_to_role(
    p_roleid INT,
    p_permissionids INT[],
    p_assignedby INT DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    assigned_count INT,
    failed_count INT
) AS $$
DECLARE
    v_permissionid INT;
    v_assigned_count INT := 0;
    v_failed_count INT := 0;
BEGIN
    -- Check if role exists
    IF NOT EXISTS (SELECT 1 FROM public.roles WHERE roleid = p_roleid) THEN
        RETURN QUERY SELECT FALSE, 'Role not found'::TEXT, 0, array_length(p_permissionids, 1);
        RETURN;
    END IF;
    
    -- Process each permission ID
    FOREACH v_permissionid IN ARRAY p_permissionids
    LOOP
        -- Check if permission exists
        IF NOT EXISTS (SELECT 1 FROM public.permissions WHERE permissionid = v_permissionid) THEN
            v_failed_count := v_failed_count + 1;
            CONTINUE;
        END IF;
        
        -- Check if assignment already exists
        IF EXISTS (SELECT 1 FROM public.rolepermissions 
                  WHERE roleid = p_roleid AND permissionid = v_permissionid) THEN
            v_failed_count := v_failed_count + 1;
            CONTINUE;
        END IF;
        
        -- Assign permission to role
        BEGIN
            INSERT INTO public.rolepermissions(roleid, permissionid, dateassigned, assignedby)
            VALUES (p_roleid, v_permissionid, CURRENT_TIMESTAMP, p_assignedby);
            
            v_assigned_count := v_assigned_count + 1;
        EXCEPTION WHEN OTHERS THEN
            v_failed_count := v_failed_count + 1;
        END;
    END LOOP;
    
    RETURN QUERY SELECT 
        TRUE, 
        format('Assigned %s permissions, %s failed', v_assigned_count, v_failed_count)::TEXT,
        v_assigned_count,
        v_failed_count;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0, array_length(p_permissionids, 1);
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- REVOKE PERMISSION FROM ROLE
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_revoke_permission_from_role(
    p_roleid INT,
    p_permissionid INT
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if assignment exists
    IF NOT EXISTS (SELECT 1 FROM public.rolepermissions 
                  WHERE roleid = p_roleid AND permissionid = p_permissionid) THEN
        RETURN QUERY SELECT FALSE, 'Permission is not assigned to this role'::TEXT;
        RETURN;
    END IF;
    
    -- Revoke permission from role
    DELETE FROM public.rolepermissions
    WHERE roleid = p_roleid AND permissionid = p_permissionid;
    
    RETURN QUERY SELECT TRUE, 'Permission revoked from role successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- REVOKE ALL PERMISSIONS FROM ROLE
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_revoke_all_permissions_from_role(
    p_roleid INT
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    revoked_count INT
) AS $$
DECLARE
    v_count INT;
BEGIN
    -- Check if role exists
    IF NOT EXISTS (SELECT 1 FROM public.roles WHERE roleid = p_roleid) THEN
        RETURN QUERY SELECT FALSE, 'Role not found'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Count existing assignments
    SELECT COUNT(*) INTO v_count FROM public.rolepermissions WHERE roleid = p_roleid;
    
    IF v_count = 0 THEN
        RETURN QUERY SELECT TRUE, 'No permissions assigned to this role'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Revoke all permissions from role
    DELETE FROM public.rolepermissions
    WHERE roleid = p_roleid;
    
    RETURN QUERY SELECT TRUE, 'All permissions revoked from role successfully'::TEXT, v_count;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET PERMISSIONS FOR ROLE
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_role_permissions(
    p_roleid INT
)
RETURNS TABLE(
    permissionid INT,
    permissionname VARCHAR(100),
    description TEXT,
    category VARCHAR(50),
    isactive BOOLEAN,
    dateassigned TIMESTAMP,
    assignedby INT,
    assignedby_username VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        p.permissionid,
        p.permissionname,
        p.description,
        p.category,
        p.isactive,
        rp.dateassigned,
        rp.assignedby,
        u.username AS assignedby_username
    FROM public.rolepermissions rp
    JOIN public.permissions p ON rp.permissionid = p.permissionid
    LEFT JOIN public.users u ON rp.assignedby = u.userid
    WHERE rp.roleid = p_roleid
    ORDER BY p.category, p.permissionname;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET ROLES FOR PERMISSION
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_permission_roles(
    p_permissionid INT
)
RETURNS TABLE(
    roleid INT,
    rolename VARCHAR(50),
    description TEXT,
    issystemrole BOOLEAN,
    isactive BOOLEAN,
    dateassigned TIMESTAMP,
    assignedby INT,
    assignedby_username VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        r.roleid,
        r.rolename,
        r.description,
        r.issystemrole,
        r.isactive,
        rp.dateassigned,
        rp.assignedby,
        u.username AS assignedby_username
    FROM public.rolepermissions rp
    JOIN public.roles r ON rp.roleid = r.roleid
    LEFT JOIN public.users u ON rp.assignedby = u.userid
    WHERE rp.permissionid = p_permissionid
    ORDER BY r.rolename;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CHECK IF ROLE HAS PERMISSION
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_role_has_permission(
    p_roleid INT,
    p_permissionid INT
)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1
        FROM public.rolepermissions rp
        JOIN public.permissions p ON rp.permissionid = p.permissionid
        WHERE rp.roleid = p_roleid 
          AND rp.permissionid = p_permissionid
          AND p.isactive = TRUE
    );
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CHECK IF ROLE HAS PERMISSION BY NAME
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_role_has_permission_by_name(
    p_roleid INT,
    p_permissionname VARCHAR(100)
)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1
        FROM public.rolepermissions rp
        JOIN public.permissions p ON rp.permissionid = p.permissionid
        WHERE rp.roleid = p_roleid 
          AND p.permissionname = p_permissionname
          AND p.isactive = TRUE
    );
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET UNASSIGNED PERMISSIONS FOR ROLE
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_unassigned_permissions_for_role(
    p_roleid INT,
    p_is_active BOOLEAN DEFAULT TRUE
)
RETURNS TABLE(
    permissionid INT,
    permissionname VARCHAR(100),
    description TEXT,
    category VARCHAR(50),
    isactive BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        p.permissionid,
        p.permissionname,
        p.description,
        p.category,
        p.isactive
    FROM public.permissions p
    WHERE 
        (p_is_active IS NULL OR p.isactive = p_is_active)
        AND NOT EXISTS (
            SELECT 1 FROM public.rolepermissions rp
            WHERE rp.roleid = p_roleid AND rp.permissionid = p.permissionid
        )
    ORDER BY p.category, p.permissionname;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- SYNC ROLE PERMISSIONS (REPLACE ALL)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_sync_role_permissions(
    p_roleid INT,
    p_permissionids INT[],
    p_assignedby INT DEFAULT NULL
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
    v_existing_permissions INT[];
    v_to_add INT[];
    v_to_remove INT[];
BEGIN
    -- Check if role exists
    IF NOT EXISTS (SELECT 1 FROM public.roles WHERE roleid = p_roleid) THEN
        RETURN QUERY SELECT FALSE, 'Role not found'::TEXT, 0, 0, 0;
        RETURN;
    END IF;
    
    -- Get current permissions for this role
    SELECT array_agg(permissionid) INTO v_existing_permissions
    FROM public.rolepermissions
    WHERE roleid = p_roleid;
    
    -- Handle case where role has no permissions yet
    IF v_existing_permissions IS NULL THEN
        v_existing_permissions := '{}'::INT[];
    END IF;
    
    -- Calculate permissions to add (in p_permissionids but not in v_existing_permissions)
    SELECT array_agg(p) INTO v_to_add
    FROM unnest(p_permissionids) p
    WHERE NOT p = ANY(v_existing_permissions);
    
    -- Handle case where no permissions to add
    IF v_to_add IS NULL THEN
        v_to_add := '{}'::INT[];
    END IF;
    
    -- Calculate permissions to remove (in v_existing_permissions but not in p_permissionids)
    SELECT array_agg(p) INTO v_to_remove
    FROM unnest(v_existing_permissions) p
    WHERE NOT p = ANY(p_permissionids);
    
    -- Handle case where no permissions to remove
    IF v_to_remove IS NULL THEN
        v_to_remove := '{}'::INT[];
    END IF;
    
    -- Calculate unchanged permissions
    v_unchanged_count := array_length(v_existing_permissions, 1) - array_length(v_to_remove, 1);
    IF v_unchanged_count IS NULL OR v_unchanged_count < 0 THEN
        v_unchanged_count := 0;
    END IF;
    
    -- Add new permissions
    IF array_length(v_to_add, 1) > 0 THEN
        INSERT INTO public.rolepermissions(roleid, permissionid, dateassigned, assignedby)
        SELECT p_roleid, p, CURRENT_TIMESTAMP, p_assignedby
        FROM unnest(v_to_add) p
        WHERE EXISTS (SELECT 1 FROM public.permissions WHERE permissionid = p);
        
        GET DIAGNOSTICS v_added_count = ROW_COUNT;
    END IF;
    
    -- Remove permissions no longer needed
    IF array_length(v_to_remove, 1) > 0 THEN
        DELETE FROM public.rolepermissions
        WHERE roleid = p_roleid AND permissionid = ANY(v_to_remove);
        
        GET DIAGNOSTICS v_removed_count = ROW_COUNT;
    END IF;
    
    RETURN QUERY SELECT 
        TRUE, 
        format('Permissions synced successfully: %s added, %s removed, %s unchanged', 
               v_added_count, v_removed_count, v_unchanged_count)::TEXT,
        v_added_count,
        v_removed_count,
        v_unchanged_count;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0, 0, 0;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET PERMISSIONS ASSIGNMENT STATISTICS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_role_permissions_statistics()
RETURNS TABLE(
    total_assignments BIGINT,
    roles_with_permissions BIGINT,
    avg_permissions_per_role NUMERIC,
    max_permissions_for_role BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*)::BIGINT AS total_assignments,
        COUNT(DISTINCT roleid)::BIGINT AS roles_with_permissions,
        ROUND(AVG(permission_count), 2) AS avg_permissions_per_role,
        MAX(permission_count)::BIGINT AS max_permissions_for_role
    FROM (
        SELECT 
            roleid, 
            COUNT(*) AS permission_count
        FROM public.rolepermissions
        GROUP BY roleid
    ) AS role_counts;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Example Usage Comments
-- =============================================

/*
-- Assign a permission to a role
SELECT * FROM sp_um_assign_permission_to_role(1, 1, 1);

-- Assign multiple permissions to a role
SELECT * FROM sp_um_assign_permissions_to_role(1, ARRAY[1, 2, 3], 1);

-- Revoke a permission from a role
SELECT * FROM sp_um_revoke_permission_from_role(1, 1);

-- Revoke all permissions from a role
SELECT * FROM sp_um_revoke_all_permissions_from_role(1);

-- Get all permissions for a role
SELECT * FROM sp_um_get_role_permissions(1);

-- Get all roles that have a specific permission
SELECT * FROM sp_um_get_permission_roles(1);

-- Check if a role has a specific permission
SELECT * FROM sp_um_role_has_permission(1, 1);

-- Check if a role has a permission by name
SELECT * FROM sp_um_role_has_permission_by_name(1, 'users.create');

-- Get permissions not assigned to a role
SELECT * FROM sp_um_get_unassigned_permissions_for_role(1, TRUE);

-- Sync role permissions (replace all)
SELECT * FROM sp_um_sync_role_permissions(1, ARRAY[1, 2, 3, 4], 1);

-- Get role permissions statistics
SELECT * FROM sp_um_get_role_permissions_statistics();
*/
