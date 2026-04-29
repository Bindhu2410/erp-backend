--- for bulk upload lead api issue fixed using this sp 
CREATE OR REPLACE FUNCTION public.get_salesmanager_child_userids(p_salesmanager_id integer)
RETURNS TABLE(userid integer) AS $$
BEGIN
    RETURN QUERY
    SELECT th.userid
    FROM public.teamhierarchy th
    WHERE th.region = (
        SELECT t2.region
        FROM public.teamhierarchy t2
        WHERE t2.userid = p_salesmanager_id
        LIMIT 1
    );
END;
$$ LANGUAGE plpgsql;

