-- =============================================
-- Roles Table CRUD Stored Procedures
-- Database: PostgreSQL
-- Table: public.roles
-- Created: July 2, 2025
-- =============================================

-- =============================================
-- CREATE ROLE
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_create_role(
    p_rolename VARCHAR(50),
    p_description TEXT DEFAULT NULL,
    p_issystemrole BOOLEAN DEFAULT FALSE,
    p_createdby INT DEFAULT NULL
)
RETURNS TABLE(
    roleid INT,
    success BOOLEAN,
    message TEXT
) AS $$
DECLARE
    v_roleid INT;
BEGIN
    -- Check if role name already exists
    IF EXISTS (SELECT 1 FROM public.roles WHERE rolename = p_rolename) THEN
        RETURN QUERY SELECT 0, FALSE, 'Role name already exists'::TEXT;
        RETURN;
    END IF;
    
    -- Check if createdby user exists if provided
    IF p_createdby IS NOT NULL AND NOT EXISTS (SELECT 1 FROM public.users WHERE userid = p_createdby) THEN
        RETURN QUERY SELECT 0, FALSE, 'Created by user does not exist'::TEXT;
        RETURN;
    END IF;
    
    -- Insert new role
    INSERT INTO public.roles (
        rolename, description, issystemrole, createdby, datecreated, isactive
    ) VALUES (
        p_rolename, p_description, p_issystemrole, p_createdby, CURRENT_TIMESTAMP, TRUE
    ) RETURNING roles.roleid INTO v_roleid;
    
    RETURN QUERY SELECT v_roleid, TRUE, 'Role created successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT 0, FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- READ ROLE BY ID
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_role_by_id(p_roleid INT)
RETURNS TABLE(
    roleid INT,
    rolename VARCHAR(50),
    description TEXT,
    issystemrole BOOLEAN,
    datecreated TIMESTAMP,
    createdby INT,
    isactive BOOLEAN,
    createdby_username VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        r.roleid, r.rolename, r.description, r.issystemrole,
        r.datecreated, r.createdby, r.isactive,
        u.username AS createdby_username
    FROM public.roles r
    LEFT JOIN public.users u ON r.createdby = u.userid
    WHERE r.roleid = p_roleid;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- READ ROLE BY NAME
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_role_by_name(p_rolename VARCHAR(50))
RETURNS TABLE(
    roleid INT,
    rolename VARCHAR(50),
    description TEXT,
    issystemrole BOOLEAN,
    datecreated TIMESTAMP,
    createdby INT,
    isactive BOOLEAN,
    createdby_username VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        r.roleid, r.rolename, r.description, r.issystemrole,
        r.datecreated, r.createdby, r.isactive,
        u.username AS createdby_username
    FROM public.roles r
    LEFT JOIN public.users u ON r.createdby = u.userid
    WHERE r.rolename = p_rolename;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- READ ALL ROLES (with pagination and filtering)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_all_roles(
    p_page_number INT DEFAULT 1,
    p_page_size INT DEFAULT 10,
    p_search_term VARCHAR(100) DEFAULT NULL,
    p_is_active BOOLEAN DEFAULT NULL,
    p_is_system_role BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    roleid INT,
    rolename VARCHAR(50),
    description TEXT,
    issystemrole BOOLEAN,
    datecreated TIMESTAMP,
    createdby INT,
    isactive BOOLEAN,
    createdby_username VARCHAR(50),
    total_count BIGINT
) AS $$
DECLARE
    v_offset INT;
BEGIN
    v_offset := (p_page_number - 1) * p_page_size;
    
    RETURN QUERY
    WITH role_data AS (
        SELECT 
            r.roleid, r.rolename, r.description, r.issystemrole,
            r.datecreated, r.createdby, r.isactive,
            u.username AS createdby_username,
            COUNT(*) OVER() as total_count
        FROM public.roles r
        LEFT JOIN public.users u ON r.createdby = u.userid
        WHERE 
            (p_is_active IS NULL OR r.isactive = p_is_active)
            AND (p_is_system_role IS NULL OR r.issystemrole = p_is_system_role)
            AND (p_search_term IS NULL OR 
                 r.rolename ILIKE '%' || p_search_term || '%' OR
                 r.description ILIKE '%' || p_search_term || '%')
        ORDER BY r.datecreated DESC
        LIMIT p_page_size OFFSET v_offset
    )
    SELECT * FROM role_data;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- UPDATE ROLE
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_update_role(
    p_roleid INT,
    p_rolename VARCHAR(50) DEFAULT NULL,
    p_description TEXT DEFAULT NULL,
    p_issystemrole BOOLEAN DEFAULT NULL,
    p_isactive BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    updated_roleid INT
) AS $$
BEGIN
    -- Check if role exists
    IF NOT EXISTS (SELECT 1 FROM public.roles WHERE roleid = p_roleid) THEN
        RETURN QUERY SELECT FALSE, 'Role not found'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Check rolename uniqueness if provided
    IF p_rolename IS NOT NULL AND EXISTS (
        SELECT 1 FROM public.roles WHERE rolename = p_rolename AND roleid != p_roleid
    ) THEN
        RETURN QUERY SELECT FALSE, 'Role name already exists'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Check if trying to modify a system role
    DECLARE v_is_system BOOLEAN;
    BEGIN
        SELECT issystemrole INTO v_is_system FROM public.roles WHERE roleid = p_roleid;
        IF v_is_system AND (p_rolename IS NOT NULL OR p_isactive = FALSE) THEN
            RETURN QUERY SELECT FALSE, 'Cannot modify system role name or deactivate it'::TEXT, 0;
            RETURN;
        END IF;
    END;
    
    -- Update role
    UPDATE public.roles SET
        rolename = COALESCE(p_rolename, rolename),
        description = COALESCE(p_description, description),
        issystemrole = COALESCE(p_issystemrole, issystemrole),
        isactive = COALESCE(p_isactive, isactive)
    WHERE roleid = p_roleid;
    
    RETURN QUERY SELECT TRUE, 'Role updated successfully'::TEXT, p_roleid;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- SOFT DELETE ROLE (DEACTIVATE)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_soft_delete_role(p_roleid INT)
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
    
    -- Check if it's a system role
    IF EXISTS (SELECT 1 FROM public.roles WHERE roleid = p_roleid AND issystemrole = TRUE) THEN
        RETURN QUERY SELECT FALSE, 'Cannot delete a system role'::TEXT;
        RETURN;
    END IF;
    
    -- Soft delete (deactivate) role
    UPDATE public.roles SET
        isactive = FALSE
    WHERE roleid = p_roleid;
    
    RETURN QUERY SELECT TRUE, 'Role deactivated successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- HARD DELETE ROLE (PERMANENT)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_hard_delete_role(p_roleid INT)
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
    
    -- Check if it's a system role
    IF EXISTS (SELECT 1 FROM public.roles WHERE roleid = p_roleid AND issystemrole = TRUE) THEN
        RETURN QUERY SELECT FALSE, 'Cannot delete a system role'::TEXT;
        RETURN;
    END IF;
    
    -- Check for role dependencies (this is a placeholder, add specific checks based on your schema)
    -- Example: IF EXISTS (SELECT 1 FROM public.user_roles WHERE roleid = p_roleid) THEN
    --     RETURN QUERY SELECT FALSE, 'Cannot delete role because it is assigned to users'::TEXT;
    --     RETURN;
    -- END IF;
    
    -- Hard delete role
    DELETE FROM public.roles WHERE roleid = p_roleid;
    
    RETURN QUERY SELECT TRUE, 'Role deleted permanently'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET ROLES STATISTICS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_roles_statistics()
RETURNS TABLE(
    total_roles BIGINT,
    active_roles BIGINT,
    inactive_roles BIGINT,
    system_roles BIGINT,
    custom_roles BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*)::BIGINT as total_roles,
        COUNT(CASE WHEN isactive = TRUE THEN 1 END)::BIGINT as active_roles,
        COUNT(CASE WHEN isactive = FALSE THEN 1 END)::BIGINT as inactive_roles,
        COUNT(CASE WHEN issystemrole = TRUE THEN 1 END)::BIGINT as system_roles,
        COUNT(CASE WHEN issystemrole = FALSE THEN 1 END)::BIGINT as custom_roles
    FROM public.roles;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Example Usage Comments
-- =============================================

/*
-- Create a new role
SELECT * FROM sp_um_create_role(
    'Administrator', 
    'Full access to all system functions', 
    TRUE, 
    1 -- Created by user ID 1
);

-- Create a standard user role
SELECT * FROM sp_um_create_role(
    'Standard User', 
    'Basic access to the system', 
    FALSE, 
    1
);

-- Get role by ID
SELECT * FROM sp_um_get_role_by_id(1);

-- Get role by name
SELECT * FROM sp_um_get_role_by_name('Administrator');

-- Get all roles with pagination
SELECT * FROM sp_um_get_all_roles(1, 10, NULL, TRUE, NULL);

-- Update role
SELECT * FROM sp_um_update_role(
    2, 
    'Standard User Updated', 
    'Updated description for standard users', 
    FALSE,
    TRUE
);

-- Soft delete role (deactivate)
SELECT * FROM sp_um_soft_delete_role(2);

-- Hard delete role (permanent)
SELECT * FROM sp_um_hard_delete_role(2);

-- Get roles statistics
SELECT * FROM sp_um_get_roles_statistics();
*/
