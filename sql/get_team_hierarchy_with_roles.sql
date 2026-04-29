-- Updated get_team_hierarchy_with_roles to support MD global view
CREATE OR REPLACE FUNCTION public.get_team_hierarchy_with_roles(p_userid integer)
RETURNS TABLE(userid integer, username text, roleid integer, rolename text, region text) AS $$
DECLARE
    user_role text;
    authority_roles text[] := ARRAY[
        'Managing Director', 'Admin', 'Manager', 'Marketing Coordinator', 'Sales Coordinator',
        'Sales Manager', 'Territory Manager', 'Area Manager', 'Field Service Technician', 'Sales Representative'
    ];
BEGIN
        -- Get the role of the current user
        SELECT r.rolename INTO user_role
        FROM public.userroles ur
        JOIN public.roles r ON ur.roleid = r.roleid
        WHERE ur.userid = p_userid
        LIMIT 1;

    IF user_role = 'Managing Director' THEN
        -- MD: return all users with authority roles from all regions
        RETURN QUERY
        SELECT u.userid, u.username::text, ur.roleid, r.rolename::text, th.region::text AS region
        FROM public.users u
        LEFT JOIN public.teamhierarchy th ON th.userid = u.userid
        LEFT JOIN public.userroles ur ON u.userid = ur.userid
        LEFT JOIN public.roles r ON ur.roleid = r.roleid
        WHERE r.rolename = ANY(authority_roles);
    ELSE
        -- Others: return users in the same region
        RETURN QUERY
        SELECT th.userid, u.username::text, ur.roleid, r.rolename::text, th.region::text AS region
        FROM public.teamhierarchy th
        JOIN public.users u ON th.userid = u.userid
        LEFT JOIN public.userroles ur ON th.userid = ur.userid
        LEFT JOIN public.roles r ON ur.roleid = r.roleid
        WHERE th.region = (SELECT th2.region FROM public.teamhierarchy th2 WHERE th2.userid = p_userid LIMIT 1);
    END IF;
END;
$$ LANGUAGE plpgsql;

    -- Get the role of the current user
