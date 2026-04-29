-- ============================================================
-- 1. GET ALL: returns every row in teamhierarchy + any
--    referenced parent that has no row of their own
-- ============================================================
CREATE OR REPLACE FUNCTION public.sp_um_get_team_hierarchy()
RETURNS TABLE(
    userid          integer,
    username        text,
    rolename        text,
    region          text,
    parent_userid   integer,
    parent_username text,
    parent_rolename text
) AS $$
BEGIN
    RETURN QUERY
    -- All users who have a teamhierarchy row
    SELECT
        th.userid,
        u.username::text,
        r.rolename::text,
        th.region::text,
        th.parent_userid,
        pu.username::text  AS parent_username,
        pr.rolename::text  AS parent_rolename
    FROM public.teamhierarchy th
    JOIN  public.users    u  ON u.userid  = th.userid
    JOIN  public.roles    r  ON r.roleid  = th.roleid
    LEFT JOIN public.users    pu ON pu.userid = th.parent_userid
    LEFT JOIN public.userroles pur ON pur.userid = pu.userid
    LEFT JOIN public.roles    pr ON pr.roleid  = pur.roleid

    UNION

    -- Parent users referenced but NOT themselves in teamhierarchy
    SELECT
        u.userid,
        u.username::text,
        r.rolename::text,
        NULL::text   AS region,
        NULL::integer AS parent_userid,
        NULL::text   AS parent_username,
        NULL::text   AS parent_rolename
    FROM public.teamhierarchy th
    JOIN  public.users    u  ON u.userid  = th.parent_userid
    LEFT JOIN public.userroles ur ON ur.userid = u.userid
    LEFT JOIN public.roles    r  ON r.roleid  = ur.roleid
    WHERE th.parent_userid IS NOT NULL
      AND th.parent_userid NOT IN (SELECT t.userid FROM public.teamhierarchy t);
END;
$$ LANGUAGE plpgsql;


-- ============================================================
-- 2. GET BY USER ID
-- ============================================================
CREATE OR REPLACE FUNCTION public.sp_um_get_team_hierarchy_by_userid(p_userid integer)
RETURNS TABLE(
    userid          integer,
    username        text,
    rolename        text,
    region          text,
    parent_userid   integer,
    parent_username text,
    parent_rolename text
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        th.userid,
        u.username::text,
        r.rolename::text,
        th.region::text,
        th.parent_userid,
        pu.username::text  AS parent_username,
        pr.rolename::text  AS parent_rolename
    FROM public.teamhierarchy th
    JOIN  public.users    u  ON u.userid  = th.userid
    JOIN  public.roles    r  ON r.roleid  = th.roleid
    LEFT JOIN public.users    pu ON pu.userid = th.parent_userid
    LEFT JOIN public.userroles pur ON pur.userid = pu.userid
    LEFT JOIN public.roles    pr ON pr.roleid  = pur.roleid
    WHERE th.userid = p_userid;
END;
$$ LANGUAGE plpgsql;


-- ============================================================
-- 3. ADD OR UPDATE
--    - If userid already exists → update it
--    - If parent_userid has no row yet → insert parent first (parent_userid = NULL)
--    - Prevents duplicate rows for the same userid
-- ============================================================
CREATE OR REPLACE FUNCTION public.sp_um_add_or_update_team_hierarchy(
    p_userid        integer,
    p_parent_userid integer,
    p_roleid        integer,
    p_region        varchar,
    p_assignedby    integer
)
RETURNS text AS $$
DECLARE
    v_parent_roleid integer;
BEGIN
    -- If a parent is given and they have no teamhierarchy row, insert them first
    IF p_parent_userid IS NOT NULL AND p_parent_userid <> 0 THEN
        IF NOT EXISTS (SELECT 1 FROM public.teamhierarchy WHERE userid = p_parent_userid) THEN
            -- Get the parent's current role
            SELECT roleid INTO v_parent_roleid
            FROM public.userroles
            WHERE userid = p_parent_userid
            LIMIT 1;

            INSERT INTO public.teamhierarchy(userid, parent_userid, roleid, region, assignedby)
            VALUES (p_parent_userid, NULL, COALESCE(v_parent_roleid, p_roleid), p_region, p_assignedby);
        END IF;
    END IF;

    -- Upsert the child user
    IF EXISTS (SELECT 1 FROM public.teamhierarchy WHERE userid = p_userid) THEN
        UPDATE public.teamhierarchy
        SET parent_userid  = CASE WHEN p_parent_userid = 0 THEN NULL ELSE p_parent_userid END,
            roleid         = p_roleid,
            region         = p_region,
            assignedby     = p_assignedby,
            assigned_date  = CURRENT_TIMESTAMP
        WHERE userid = p_userid;
        RETURN 'Team hierarchy updated successfully.';
    ELSE
        INSERT INTO public.teamhierarchy(userid, parent_userid, roleid, region, assignedby)
        VALUES (
            p_userid,
            CASE WHEN p_parent_userid = 0 THEN NULL ELSE p_parent_userid END,
            p_roleid,
            p_region,
            p_assignedby
        );
        RETURN 'Team hierarchy added successfully.';
    END IF;
END;
$$ LANGUAGE plpgsql;


-- ============================================================
-- 4. DELETE BY USER ID
-- ============================================================
CREATE OR REPLACE FUNCTION public.sp_um_delete_team_hierarchy(p_userid integer)
RETURNS text AS $$
BEGIN
    IF EXISTS (SELECT 1 FROM public.teamhierarchy WHERE userid = p_userid) THEN
        DELETE FROM public.teamhierarchy WHERE userid = p_userid;
        RETURN 'Team hierarchy deleted successfully.';
    ELSE
        RETURN 'User not found in team hierarchy.';
    END IF;
END;
$$ LANGUAGE plpgsql;
