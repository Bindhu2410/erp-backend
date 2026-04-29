-- =============================================
-- UserRoles Table CRUD Stored Procedures
-- Database: PostgreSQL
-- Table: public.userroles
-- Created: July 3, 2025
-- =============================================

-- =============================================
-- ASSIGN ROLE TO USER
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_assign_role_to_user(
    p_userid INT,
    p_roleid INT,
    p_assignedby INT DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM public.users WHERE userid = p_userid) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT;
        RETURN;
    END IF;
    
    -- Check if role exists
    IF NOT EXISTS (SELECT 1 FROM public.roles WHERE roleid = p_roleid) THEN
        RETURN QUERY SELECT FALSE, 'Role not found'::TEXT;
        RETURN;
    END IF;
    
    -- Check if assignment already exists
    IF EXISTS (SELECT 1 FROM public.userroles 
               WHERE userid = p_userid AND roleid = p_roleid) THEN
        RETURN QUERY SELECT FALSE, 'Role is already assigned to this user'::TEXT;
        RETURN;
    END IF;
    
    -- Assign role to user
    INSERT INTO public.userroles(userid, roleid, dateassigned, assignedby)
    VALUES (p_userid, p_roleid, CURRENT_TIMESTAMP, p_assignedby);
    
    RETURN QUERY SELECT TRUE, 'Role assigned to user successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- ASSIGN MULTIPLE ROLES TO USER
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_assign_roles_to_user(
    p_userid INT,
    p_roleids INT[],
    p_assignedby INT DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    assigned_count INT,
    failed_count INT
) AS $$
DECLARE
    v_roleid INT;
    v_assigned_count INT := 0;
    v_failed_count INT := 0;
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM public.users WHERE userid = p_userid) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT, 0, array_length(p_roleids, 1);
        RETURN;
    END IF;
    
    -- Process each role ID
    FOREACH v_roleid IN ARRAY p_roleids
    LOOP
        -- Check if role exists
        IF NOT EXISTS (SELECT 1 FROM public.roles WHERE roleid = v_roleid) THEN
            v_failed_count := v_failed_count + 1;
            CONTINUE;
        END IF;
        
        -- Check if assignment already exists
        IF EXISTS (SELECT 1 FROM public.userroles 
                  WHERE userid = p_userid AND roleid = v_roleid) THEN
            v_failed_count := v_failed_count + 1;
            CONTINUE;
        END IF;
        
        -- Assign role to user
        BEGIN
            INSERT INTO public.userroles(userid, roleid, dateassigned, assignedby)
            VALUES (p_userid, v_roleid, CURRENT_TIMESTAMP, p_assignedby);
            
            v_assigned_count := v_assigned_count + 1;
        EXCEPTION WHEN OTHERS THEN
            v_failed_count := v_failed_count + 1;
        END;
    END LOOP;
    
    RETURN QUERY SELECT 
        TRUE, 
        format('Assigned %s roles, %s failed', v_assigned_count, v_failed_count)::TEXT,
        v_assigned_count,
        v_failed_count;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0, array_length(p_roleids, 1);
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- ASSIGN ROLE TO MULTIPLE USERS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_assign_role_to_users(
    p_userids INT[],
    p_roleid INT,
    p_assignedby INT DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    assigned_count INT,
    failed_count INT
) AS $$
DECLARE
    v_userid INT;
    v_assigned_count INT := 0;
    v_failed_count INT := 0;
BEGIN
    -- Check if role exists
    IF NOT EXISTS (SELECT 1 FROM public.roles WHERE roleid = p_roleid) THEN
        RETURN QUERY SELECT FALSE, 'Role not found'::TEXT, 0, array_length(p_userids, 1);
        RETURN;
    END IF;
    
    -- Process each user ID
    FOREACH v_userid IN ARRAY p_userids
    LOOP
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM public.users WHERE userid = v_userid) THEN
            v_failed_count := v_failed_count + 1;
            CONTINUE;
        END IF;
        
        -- Check if assignment already exists
        IF EXISTS (SELECT 1 FROM public.userroles 
                  WHERE userid = v_userid AND roleid = p_roleid) THEN
            v_failed_count := v_failed_count + 1;
            CONTINUE;
        END IF;
        
        -- Assign role to user
        BEGIN
            INSERT INTO public.userroles(userid, roleid, dateassigned, assignedby)
            VALUES (v_userid, p_roleid, CURRENT_TIMESTAMP, p_assignedby);
            
            v_assigned_count := v_assigned_count + 1;
        EXCEPTION WHEN OTHERS THEN
            v_failed_count := v_failed_count + 1;
        END;
    END LOOP;
    
    RETURN QUERY SELECT 
        TRUE, 
        format('Assigned role to %s users, %s failed', v_assigned_count, v_failed_count)::TEXT,
        v_assigned_count,
        v_failed_count;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0, array_length(p_userids, 1);
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- REVOKE ROLE FROM USER
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_revoke_role_from_user(
    p_userid INT,
    p_roleid INT
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if assignment exists
    IF NOT EXISTS (SELECT 1 FROM public.userroles 
                  WHERE userid = p_userid AND roleid = p_roleid) THEN
        RETURN QUERY SELECT FALSE, 'Role is not assigned to this user'::TEXT;
        RETURN;
    END IF;
    
    -- Revoke role from user
    DELETE FROM public.userroles
    WHERE userid = p_userid AND roleid = p_roleid;
    
    RETURN QUERY SELECT TRUE, 'Role revoked from user successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- REVOKE ALL ROLES FROM USER
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_revoke_all_roles_from_user(
    p_userid INT
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    revoked_count INT
) AS $$
DECLARE
    v_count INT;
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM public.users WHERE userid = p_userid) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Count existing assignments
    SELECT COUNT(*) INTO v_count FROM public.userroles WHERE userid = p_userid;
    
    IF v_count = 0 THEN
        RETURN QUERY SELECT TRUE, 'No roles assigned to this user'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Revoke all roles from user
    DELETE FROM public.userroles
    WHERE userid = p_userid;
    
    RETURN QUERY SELECT TRUE, 'All roles revoked from user successfully'::TEXT, v_count;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- REVOKE ROLE FROM ALL USERS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_revoke_role_from_all_users(
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
    SELECT COUNT(*) INTO v_count FROM public.userroles WHERE roleid = p_roleid;
    
    IF v_count = 0 THEN
        RETURN QUERY SELECT TRUE, 'No users assigned to this role'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Revoke role from all users
    DELETE FROM public.userroles
    WHERE roleid = p_roleid;
    
    RETURN QUERY SELECT TRUE, format('Role revoked from %s users successfully', v_count)::TEXT, v_count;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET USER ROLES
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_user_roles(
    p_userid INT
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
        ur.dateassigned,
        ur.assignedby,
        u.username AS assignedby_username
    FROM public.userroles ur
    JOIN public.roles r ON ur.roleid = r.roleid
    LEFT JOIN public.users u ON ur.assignedby = u.userid
    WHERE ur.userid = p_userid
    ORDER BY r.rolename;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET USERS WITH ROLE
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_users_with_role(
    p_roleid INT
)
RETURNS TABLE(
    userid INT,
    username VARCHAR(50),
    email VARCHAR(255),
    fullname VARCHAR(100),
    isactive BOOLEAN,
    dateassigned TIMESTAMP,
    assignedby INT,
    assignedby_username VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u.userid,
        u.username,
        u.email,
        u.fullname,
        u.isactive,
        ur.dateassigned,
        ur.assignedby,
        a.username AS assignedby_username
    FROM public.userroles ur
    JOIN public.users u ON ur.userid = u.userid
    LEFT JOIN public.users a ON ur.assignedby = a.userid
    WHERE ur.roleid = p_roleid
    ORDER BY u.username;
    
EXCEPTION
    WHEN OTHERS THEN
        -- Return empty result
        RETURN;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET USERS WITH ROLE (WITH PAGINATION)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_users_with_role_paginated(
    p_roleid INT,
    p_page_number INT DEFAULT 1,
    p_page_size INT DEFAULT 10,
    p_search_term VARCHAR(100) DEFAULT NULL,
    p_is_active BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    userid INT,
    username VARCHAR(50),
    email VARCHAR(255),
    fullname VARCHAR(100),
    isactive BOOLEAN,
    dateassigned TIMESTAMP,
    assignedby INT,
    assignedby_username VARCHAR(50),
    total_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u.userid,
        u.username,
        u.email,
        u.fullname,
        u.isactive,
        ur.dateassigned,
        ur.assignedby,
        a.username AS assignedby_username,
        COUNT(*) OVER() AS total_count
    FROM public.userroles ur
    JOIN public.users u ON ur.userid = u.userid
    LEFT JOIN public.users a ON ur.assignedby = a.userid
    WHERE ur.roleid = p_roleid
      AND (p_is_active IS NULL OR u.isactive = p_is_active)
      AND (p_search_term IS NULL OR 
           u.username ILIKE '%' || p_search_term || '%' OR
           u.fullname ILIKE '%' || p_search_term || '%' OR
           u.email ILIKE '%' || p_search_term || '%')
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
-- CHECK IF USER HAS ROLE
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_user_has_role(
    p_userid INT,
    p_roleid INT
)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1
        FROM public.userroles ur
        JOIN public.roles r ON ur.roleid = r.roleid
        WHERE ur.userid = p_userid 
          AND ur.roleid = p_roleid
          AND r.isactive = TRUE
    );
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CHECK IF USER HAS ROLE BY NAME
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_user_has_role_by_name(
    p_userid INT,
    p_rolename VARCHAR(50)
)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1
        FROM public.userroles ur
        JOIN public.roles r ON ur.roleid = r.roleid
        WHERE ur.userid = p_userid 
          AND r.rolename = p_rolename
          AND r.isactive = TRUE
    );
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET UNASSIGNED ROLES FOR USER
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_unassigned_roles_for_user(
    p_userid INT,
    p_is_active BOOLEAN DEFAULT TRUE
)
RETURNS TABLE(
    roleid INT,
    rolename VARCHAR(50),
    description TEXT,
    issystemrole BOOLEAN,
    isactive BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        r.roleid,
        r.rolename,
        r.description,
        r.issystemrole,
        r.isactive
    FROM public.roles r
    WHERE 
        (p_is_active IS NULL OR r.isactive = p_is_active)
        AND NOT EXISTS (
            SELECT 1 FROM public.userroles ur
            WHERE ur.userid = p_userid AND ur.roleid = r.roleid
        )
    ORDER BY r.rolename;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET USERS WITHOUT ROLE
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_users_without_role(
    p_roleid INT,
    p_is_active BOOLEAN DEFAULT TRUE
)
RETURNS TABLE(
    userid INT,
    username VARCHAR(50),
    email VARCHAR(255),
    fullname VARCHAR(100),
    isactive BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u.userid,
        u.username,
        u.email,
        u.fullname,
        u.isactive
    FROM public.users u
    WHERE 
        (p_is_active IS NULL OR u.isactive = p_is_active)
        AND NOT EXISTS (
            SELECT 1 FROM public.userroles ur
            WHERE ur.userid = u.userid AND ur.roleid = p_roleid
        )
    ORDER BY u.username;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- SYNC USER ROLES (REPLACE ALL)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_sync_user_roles(
    p_userid INT,
    p_roleids INT[],
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
    v_existing_roles INT[];
    v_to_add INT[];
    v_to_remove INT[];
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM public.users WHERE userid = p_userid) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT, 0, 0, 0;
        RETURN;
    END IF;
    
    -- Get current roles for this user
    SELECT array_agg(roleid) INTO v_existing_roles
    FROM public.userroles
    WHERE userid = p_userid;
    
    -- Handle case where user has no roles yet
    IF v_existing_roles IS NULL THEN
        v_existing_roles := '{}'::INT[];
    END IF;
    
    -- Calculate roles to add (in p_roleids but not in v_existing_roles)
    SELECT array_agg(r) INTO v_to_add
    FROM unnest(p_roleids) r
    WHERE NOT r = ANY(v_existing_roles);
    
    -- Handle case where no roles to add
    IF v_to_add IS NULL THEN
        v_to_add := '{}'::INT[];
    END IF;
    
    -- Calculate roles to remove (in v_existing_roles but not in p_roleids)
    SELECT array_agg(r) INTO v_to_remove
    FROM unnest(v_existing_roles) r
    WHERE NOT r = ANY(p_roleids);
    
    -- Handle case where no roles to remove
    IF v_to_remove IS NULL THEN
        v_to_remove := '{}'::INT[];
    END IF;
    
    -- Calculate unchanged roles
    v_unchanged_count := array_length(v_existing_roles, 1) - array_length(v_to_remove, 1);
    IF v_unchanged_count IS NULL OR v_unchanged_count < 0 THEN
        v_unchanged_count := 0;
    END IF;
    
    -- Add new roles
    IF array_length(v_to_add, 1) > 0 THEN
        INSERT INTO public.userroles(userid, roleid, dateassigned, assignedby)
        SELECT p_userid, r, CURRENT_TIMESTAMP, p_assignedby
        FROM unnest(v_to_add) r
        WHERE EXISTS (SELECT 1 FROM public.roles WHERE roleid = r);
        
        GET DIAGNOSTICS v_added_count = ROW_COUNT;
    END IF;
    
    -- Remove roles no longer needed
    IF array_length(v_to_remove, 1) > 0 THEN
        DELETE FROM public.userroles
        WHERE userid = p_userid AND roleid = ANY(v_to_remove);
        
        GET DIAGNOSTICS v_removed_count = ROW_COUNT;
    END IF;
    
    RETURN QUERY SELECT 
        TRUE, 
        format('Roles synced successfully: %s added, %s removed, %s unchanged', 
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
-- GET USER ROLE ASSIGNMENT STATISTICS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_user_roles_statistics()
RETURNS TABLE(
    total_assignments BIGINT,
    users_with_roles BIGINT,
    roles_assigned_to_users BIGINT,
    avg_roles_per_user NUMERIC,
    avg_users_per_role NUMERIC,
    max_roles_for_user BIGINT,
    max_users_for_role BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*)::BIGINT AS total_assignments,
        COUNT(DISTINCT userid)::BIGINT AS users_with_roles,
        COUNT(DISTINCT roleid)::BIGINT AS roles_assigned_to_users,
        ROUND(AVG(role_count), 2) AS avg_roles_per_user,
        ROUND(AVG(user_count), 2) AS avg_users_per_role,
        MAX(role_count)::BIGINT AS max_roles_for_user,
        MAX(user_count)::BIGINT AS max_users_for_role
    FROM (
        SELECT 
            userid, 
            COUNT(*) AS role_count
        FROM public.userroles
        GROUP BY userid
    ) AS user_counts
    CROSS JOIN (
        SELECT 
            roleid, 
            COUNT(*) AS user_count
        FROM public.userroles
        GROUP BY roleid
    ) AS role_counts;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CHECK IF USER HAS ANY PERMISSIONS FROM ROLE
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_user_has_any_permission_from_role(
    p_userid INT,
    p_permissionids INT[]
)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1
        FROM public.userroles ur
        JOIN public.roles r ON ur.roleid = r.roleid
        JOIN public.rolepermissions rp ON r.roleid = rp.roleid
        JOIN public.permissions p ON rp.permissionid = p.permissionid
        WHERE ur.userid = p_userid
          AND r.isactive = TRUE
          AND p.isactive = TRUE
          AND p.permissionid = ANY(p_permissionids)
    );
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- CHECK IF USER HAS ALL PERMISSIONS FROM ROLES
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_user_has_all_permissions_from_roles(
    p_userid INT,
    p_permissionids INT[]
)
RETURNS BOOLEAN AS $$
DECLARE
    v_count INT;
BEGIN
    SELECT COUNT(DISTINCT p.permissionid)
    INTO v_count
    FROM public.userroles ur
    JOIN public.roles r ON ur.roleid = r.roleid
    JOIN public.rolepermissions rp ON r.roleid = rp.roleid
    JOIN public.permissions p ON rp.permissionid = p.permissionid
    WHERE ur.userid = p_userid
      AND r.isactive = TRUE
      AND p.isactive = TRUE
      AND p.permissionid = ANY(p_permissionids);
      
    RETURN v_count = array_length(p_permissionids, 1);
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET ALL USER PERMISSIONS (FROM ALL ROLES)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_all_user_permissions(
    p_userid INT
)
RETURNS TABLE(
    permissionid INT,
    permissionname VARCHAR(100),
    description TEXT,
    category VARCHAR(50),
    isactive BOOLEAN,
    roleid INT,
    rolename VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT DISTINCT
        p.permissionid,
        p.permissionname,
        p.description,
        p.category,
        p.isactive,
        r.roleid,
        r.rolename
    FROM public.userroles ur
    JOIN public.roles r ON ur.roleid = r.roleid
    JOIN public.rolepermissions rp ON r.roleid = rp.roleid
    JOIN public.permissions p ON rp.permissionid = p.permissionid
    WHERE ur.userid = p_userid
      AND r.isactive = TRUE
      AND p.isactive = TRUE
    ORDER BY p.category, p.permissionname;
    
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
-- Assign a role to a user
SELECT * FROM sp_um_assign_role_to_user(1, 1, 1);

-- Assign multiple roles to a user
SELECT * FROM sp_um_assign_roles_to_user(1, ARRAY[1, 2, 3], 1);

-- Assign a role to multiple users
SELECT * FROM sp_um_assign_role_to_users(ARRAY[1, 2, 3], 1, 1);

-- Revoke a role from a user
SELECT * FROM sp_um_revoke_role_from_user(1, 1);

-- Revoke all roles from a user
SELECT * FROM sp_um_revoke_all_roles_from_user(1);

-- Revoke a role from all users
SELECT * FROM sp_um_revoke_role_from_all_users(1);

-- Get all roles assigned to a user
SELECT * FROM sp_um_get_user_roles(1);

-- Get all users assigned to a role
SELECT * FROM sp_um_get_users_with_role(1);

-- Get users with a role (paginated)
SELECT * FROM sp_um_get_users_with_role_paginated(1, 1, 10, 'john', TRUE);

-- Check if user has role
SELECT * FROM sp_um_user_has_role(1, 1);

-- Check if user has role by name
SELECT * FROM sp_um_user_has_role_by_name(1, 'Administrator');

-- Get unassigned roles for a user
SELECT * FROM sp_um_get_unassigned_roles_for_user(1, TRUE);

-- Get users without a specific role
SELECT * FROM sp_um_get_users_without_role(1, TRUE);

-- Sync user roles (replace all)
SELECT * FROM sp_um_sync_user_roles(1, ARRAY[1, 2, 3, 4], 1);

-- Get user role statistics
SELECT * FROM sp_um_get_user_roles_statistics();

-- Check if user has any permissions from their roles
SELECT * FROM sp_um_user_has_any_permission_from_role(1, ARRAY[1, 2, 3]);

-- Check if user has all permissions from their roles
SELECT * FROM sp_um_user_has_all_permissions_from_roles(1, ARRAY[1, 2, 3]);

-- Get all permissions a user has (from all their roles)
SELECT * FROM sp_um_get_all_user_permissions(1);
*/
