-- =============================================
-- Permissions Table CRUD Stored Procedures
-- Database: PostgreSQL
-- Table: public.permissions
-- Created: July 2, 2025
-- =============================================

-- =============================================
-- CREATE PERMISSION
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_create_permission(
    p_permissionname VARCHAR(100),
    p_description TEXT DEFAULT NULL,
    p_category VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE(
    permissionid INT,
    success BOOLEAN,
    message TEXT
) AS $$
DECLARE
    v_permissionid INT;
BEGIN
    -- Check if permission name already exists
    IF EXISTS (SELECT 1 FROM public.permissions WHERE permissionname = p_permissionname) THEN
        RETURN QUERY SELECT 0, FALSE, 'Permission name already exists'::TEXT;
        RETURN;
    END IF;
    
    -- Insert new permission
    INSERT INTO public.permissions (
        permissionname, description, category, isactive
    ) VALUES (
        p_permissionname, p_description, p_category, TRUE
    ) RETURNING permissions.permissionid INTO v_permissionid;
    
    RETURN QUERY SELECT v_permissionid, TRUE, 'Permission created successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT 0, FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- READ PERMISSION BY ID
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_permission_by_id(p_permissionid INT)
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
        p.permissionid, p.permissionname, p.description, p.category, p.isactive
    FROM public.permissions p
    WHERE p.permissionid = p_permissionid;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- READ PERMISSION BY NAME
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_permission_by_name(p_permissionname VARCHAR(100))
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
        p.permissionid, p.permissionname, p.description, p.category, p.isactive
    FROM public.permissions p
    WHERE p.permissionname = p_permissionname;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- READ ALL PERMISSIONS (with pagination and filtering)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_all_permissions(
    p_page_number INT DEFAULT 1,
    p_page_size INT DEFAULT 10,
    p_search_term VARCHAR(100) DEFAULT NULL,
    p_category VARCHAR(50) DEFAULT NULL,
    p_is_active BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    permissionid INT,
    permissionname VARCHAR(100),
    description TEXT,
    category VARCHAR(50),
    isactive BOOLEAN,
    total_count BIGINT
) AS $$
DECLARE
    v_offset INT;
BEGIN
    v_offset := (p_page_number - 1) * p_page_size;
    
    RETURN QUERY
    WITH permission_data AS (
        SELECT 
            p.permissionid, p.permissionname, p.description, p.category, p.isactive,
            COUNT(*) OVER() as total_count
        FROM public.permissions p
        WHERE 
            (p_is_active IS NULL OR p.isactive = p_is_active)
            AND (p_category IS NULL OR p.category = p_category)
            AND (p_search_term IS NULL OR 
                 p.permissionname ILIKE '%' || p_search_term || '%' OR
                 p.description ILIKE '%' || p_search_term || '%')
        ORDER BY p.category, p.permissionname
        LIMIT p_page_size OFFSET v_offset
    )
    SELECT * FROM permission_data;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET PERMISSIONS BY CATEGORY
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_permissions_by_category(
    p_category VARCHAR(50),
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
        p.permissionid, p.permissionname, p.description, p.category, p.isactive
    FROM public.permissions p
    WHERE 
        p.category = p_category
        AND (p_is_active IS NULL OR p.isactive = p_is_active)
    ORDER BY p.permissionname;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET DISTINCT CATEGORIES
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_permission_categories()
RETURNS TABLE(
    category VARCHAR(50)
) AS $$
BEGIN
    RETURN QUERY
    SELECT DISTINCT p.category
    FROM public.permissions p
    WHERE p.category IS NOT NULL
    ORDER BY p.category;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- UPDATE PERMISSION
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_update_permission(
    p_permissionid INT,
    p_permissionname VARCHAR(100) DEFAULT NULL,
    p_description TEXT DEFAULT NULL,
    p_category VARCHAR(50) DEFAULT NULL,
    p_isactive BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    updated_permissionid INT
) AS $$
BEGIN
    -- Check if permission exists
    IF NOT EXISTS (SELECT 1 FROM public.permissions WHERE permissionid = p_permissionid) THEN
        RETURN QUERY SELECT FALSE, 'Permission not found'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Check permissionname uniqueness if provided
    IF p_permissionname IS NOT NULL AND EXISTS (
        SELECT 1 FROM public.permissions 
        WHERE permissionname = p_permissionname AND permissionid != p_permissionid
    ) THEN
        RETURN QUERY SELECT FALSE, 'Permission name already exists'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Update permission
    UPDATE public.permissions SET
        permissionname = COALESCE(p_permissionname, permissionname),
        description = COALESCE(p_description, description),
        category = COALESCE(p_category, category),
        isactive = COALESCE(p_isactive, isactive)
    WHERE permissionid = p_permissionid;
    
    RETURN QUERY SELECT TRUE, 'Permission updated successfully'::TEXT, p_permissionid;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- SOFT DELETE PERMISSION (DEACTIVATE)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_soft_delete_permission(p_permissionid INT)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if permission exists
    IF NOT EXISTS (SELECT 1 FROM public.permissions WHERE permissionid = p_permissionid) THEN
        RETURN QUERY SELECT FALSE, 'Permission not found'::TEXT;
        RETURN;
    END IF;
    
    -- Soft delete (deactivate) permission
    UPDATE public.permissions SET
        isactive = FALSE
    WHERE permissionid = p_permissionid;
    
    RETURN QUERY SELECT TRUE, 'Permission deactivated successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- HARD DELETE PERMISSION (PERMANENT)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_hard_delete_permission(p_permissionid INT)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if permission exists
    IF NOT EXISTS (SELECT 1 FROM public.permissions WHERE permissionid = p_permissionid) THEN
        RETURN QUERY SELECT FALSE, 'Permission not found'::TEXT;
        RETURN;
    END IF;
    
    -- Check for permission dependencies (this is a placeholder - add specific checks based on your schema)
    -- Example: IF EXISTS (SELECT 1 FROM public.role_permissions WHERE permissionid = p_permissionid) THEN
    --     RETURN QUERY SELECT FALSE, 'Cannot delete permission because it is assigned to roles'::TEXT;
    --     RETURN;
    -- END IF;
    
    -- Hard delete permission
    DELETE FROM public.permissions WHERE permissionid = p_permissionid;
    
    RETURN QUERY SELECT TRUE, 'Permission deleted permanently'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET PERMISSIONS STATISTICS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_permissions_statistics()
RETURNS TABLE(
    total_permissions BIGINT,
    active_permissions BIGINT,
    inactive_permissions BIGINT,
    categories_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*)::BIGINT as total_permissions,
        COUNT(CASE WHEN isactive = TRUE THEN 1 END)::BIGINT as active_permissions,
        COUNT(CASE WHEN isactive = FALSE THEN 1 END)::BIGINT as inactive_permissions,
        COUNT(DISTINCT category)::BIGINT as categories_count
    FROM public.permissions;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- BATCH CREATE PERMISSIONS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_batch_create_permissions(
    p_permissions JSONB
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    created_count INT,
    failed_count INT
) AS $$
DECLARE
    v_permission JSONB;
    v_created_count INT := 0;
    v_failed_count INT := 0;
    v_name VARCHAR(100);
    v_desc TEXT;
    v_category VARCHAR(50);
    v_result RECORD;
BEGIN
    -- Loop through each permission in the JSON array
    FOR v_permission IN SELECT * FROM jsonb_array_elements(p_permissions)
    LOOP
        -- Extract values
        v_name := v_permission->>'permissionname';
        v_desc := v_permission->>'description';
        v_category := v_permission->>'category';
        
        -- Skip if permission name already exists
        IF EXISTS (SELECT 1 FROM public.permissions WHERE permissionname = v_name) THEN
            v_failed_count := v_failed_count + 1;
            CONTINUE;
        END IF;
        
        -- Insert new permission
        BEGIN
            INSERT INTO public.permissions (permissionname, description, category, isactive)
            VALUES (v_name, v_desc, v_category, TRUE);
            
            v_created_count := v_created_count + 1;
        EXCEPTION WHEN OTHERS THEN
            v_failed_count := v_failed_count + 1;
        END;
    END LOOP;
    
    RETURN QUERY SELECT 
        TRUE, 
        format('Created %s permissions, %s failed', v_created_count, v_failed_count)::TEXT,
        v_created_count,
        v_failed_count;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Example Usage Comments
-- =============================================

/*
-- Create a new permission
SELECT * FROM sp_um_create_permission(
    'users.create', 
    'Create new users in the system', 
    'User Management'
);

-- Get permission by ID
SELECT * FROM sp_um_get_permission_by_id(1);

-- Get permission by name
SELECT * FROM sp_um_get_permission_by_name('users.create');

-- Get all permissions with pagination and filtering
SELECT * FROM sp_um_get_all_permissions(1, 10, NULL, 'User Management', TRUE);

-- Get permissions by category
SELECT * FROM sp_um_get_permissions_by_category('User Management');

-- Get distinct categories
SELECT * FROM sp_um_get_permission_categories();

-- Update permission
SELECT * FROM sp_um_update_permission(
    1, 
    'users.create', 
    'Updated: Create new users in the system', 
    'User Management',
    TRUE
);

-- Soft delete permission (deactivate)
SELECT * FROM sp_um_soft_delete_permission(1);

-- Hard delete permission (permanent)
SELECT * FROM sp_um_hard_delete_permission(1);

-- Get permissions statistics
SELECT * FROM sp_um_get_permissions_statistics();

-- Batch create permissions
SELECT * FROM sp_um_batch_create_permissions('[
    {"permissionname": "users.view", "description": "View user details", "category": "User Management"},
    {"permissionname": "users.edit", "description": "Edit user details", "category": "User Management"},
    {"permissionname": "users.delete", "description": "Delete users", "category": "User Management"},
    {"permissionname": "roles.manage", "description": "Manage roles", "category": "Access Control"}
]');
*/
