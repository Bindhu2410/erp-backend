-- Stored procedure to get users based on hierarchy
-- Admin sees all users, Sales Manager sees their team only
CREATE OR REPLACE FUNCTION get_users_by_hierarchy(p_user_id INTEGER)
RETURNS TABLE (
    userid INTEGER,
    username VARCHAR(50),
    email VARCHAR(100),
    firstname VARCHAR(50),
    lastname VARCHAR(50),
    phonenumber VARCHAR(20),
    role VARCHAR(50),
    manager_id INTEGER,
    department VARCHAR(100),
    isactive BOOLEAN
) AS $$
DECLARE
    user_role VARCHAR(50);
BEGIN
    -- Get the requesting user's role
    SELECT role INTO user_role FROM users WHERE userid = p_user_id;
    
    -- If user is admin, return all users
    IF user_role = 'admin' THEN
        RETURN QUERY
        SELECT u.userid, u.username, u.email, u.firstname, u.lastname, 
               u.phonenumber, u.role, u.manager_id, u.department, u.isactive
        FROM users u
        WHERE u.isactive = true
        ORDER BY u.username;
    
    -- If user is sales manager, return their team members
    ELSIF user_role = 'sales_manager' THEN
        RETURN QUERY
        SELECT u.userid, u.username, u.email, u.firstname, u.lastname,
               u.phonenumber, u.role, u.manager_id, u.department, u.isactive
        FROM users u
        WHERE u.isactive = true 
        AND (u.manager_id = p_user_id OR u.userid = p_user_id)
        ORDER BY u.username;
    
    -- Regular users see only themselves
    ELSE
        RETURN QUERY
        SELECT u.userid, u.username, u.email, u.firstname, u.lastname,
               u.phonenumber, u.role, u.manager_id, u.department, u.isactive
        FROM users u
        WHERE u.userid = p_user_id AND u.isactive = true;
    END IF;
END;
$$ LANGUAGE plpgsql;