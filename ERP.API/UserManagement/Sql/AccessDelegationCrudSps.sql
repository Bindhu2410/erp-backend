-- AccessDelegations CRUD Stored Procedures

CREATE OR REPLACE PROCEDURE sp_um_insert_access_delegation(
    p_from_user_id INTEGER,
    p_to_user_id INTEGER,
    p_start_date TIMESTAMP,
    p_end_date TIMESTAMP,
    p_created_by INTEGER,
    p_reason TEXT = NULL,
    p_is_active BOOLEAN = TRUE,
    INOUT p_delegation_id INTEGER = NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO public."AccessDelegations"(
        "FromUserId",
        "ToUserId",
        "StartDate",
        "EndDate",
        "Reason",
        "IsActive",
        "CreatedBy",
        "DateCreated"
    ) VALUES (
        p_from_user_id,
        p_to_user_id,
        p_start_date,
        p_end_date,
        p_reason,
        p_is_active,
        p_created_by,
        CURRENT_TIMESTAMP
    ) RETURNING "DelegationId" INTO p_delegation_id;
END;
$$;


-- Read (Get) Access Delegation by ID
CREATE OR REPLACE FUNCTION sp_um_get_access_delegation_by_id(
    p_delegation_id INTEGER
)
RETURNS TABLE (
    "DelegationId" INTEGER,
    "FromUserId" INTEGER,
    "ToUserId" INTEGER,
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    "Reason" TEXT,
    "IsActive" BOOLEAN,
    "CreatedBy" INTEGER,
    "DateCreated" TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        d."DelegationId",
        d."FromUserId",
        d."ToUserId",
        d."StartDate",
        d."EndDate",
        d."Reason",
        d."IsActive",
        d."CreatedBy",
        d."DateCreated"
    FROM 
        public."AccessDelegations" d
    WHERE 
        d."DelegationId" = p_delegation_id;
END;
$$;

-- Update Access Delegation
CREATE OR REPLACE PROCEDURE sp_um_update_access_delegation(
    p_delegation_id INTEGER,
    p_from_user_id INTEGER = NULL,
    p_to_user_id INTEGER = NULL,
    p_start_date TIMESTAMP = NULL,
    p_end_date TIMESTAMP = NULL,
    p_reason TEXT = NULL,
    p_is_active BOOLEAN = NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE public."AccessDelegations"
    SET 
        "FromUserId" = COALESCE(p_from_user_id, "FromUserId"),
        "ToUserId" = COALESCE(p_to_user_id, "ToUserId"),
        "StartDate" = COALESCE(p_start_date, "StartDate"),
        "EndDate" = COALESCE(p_end_date, "EndDate"),
        "Reason" = COALESCE(p_reason, "Reason"),
        "IsActive" = COALESCE(p_is_active, "IsActive")
    WHERE 
        "DelegationId" = p_delegation_id;
END;
$$;

-- Delete Access Delegation
CREATE OR REPLACE PROCEDURE sp_um_delete_access_delegation(
    p_delegation_id INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM public."AccessDelegations"
    WHERE "DelegationId" = p_delegation_id;
END;
$$;

-- Get All Access Delegations with Pagination
CREATE OR REPLACE FUNCTION sp_um_get_access_delegations_paged(
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10
)
RETURNS TABLE (
    "DelegationId" INTEGER,
    "FromUserId" INTEGER,
    "ToUserId" INTEGER,
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    "Reason" TEXT,
    "IsActive" BOOLEAN,
    "CreatedBy" INTEGER,
    "DateCreated" TIMESTAMP,
    "TotalCount" BIGINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INTEGER;
    v_total_count BIGINT;
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Get total count
    SELECT COUNT(*) INTO v_total_count FROM public."AccessDelegations";
    
    RETURN QUERY
    SELECT 
        d."DelegationId",
        d."FromUserId",
        d."ToUserId",
        d."StartDate",
        d."EndDate",
        d."Reason",
        d."IsActive",
        d."CreatedBy",
        d."DateCreated",
        v_total_count AS "TotalCount"
    FROM 
        public."AccessDelegations" d
    ORDER BY 
        d."DateCreated" DESC
    LIMIT p_page_size
    OFFSET v_offset;
END;
$$;

-- Get Active Delegations by FromUserId
CREATE OR REPLACE FUNCTION sp_um_get_active_delegations_by_from_user(
    p_from_user_id INTEGER
)
RETURNS TABLE (
    "DelegationId" INTEGER,
    "FromUserId" INTEGER,
    "ToUserId" INTEGER,
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    "Reason" TEXT,
    "IsActive" BOOLEAN,
    "CreatedBy" INTEGER,
    "DateCreated" TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        d."DelegationId",
        d."FromUserId",
        d."ToUserId",
        d."StartDate",
        d."EndDate",
        d."Reason",
        d."IsActive",
        d."CreatedBy",
        d."DateCreated"
    FROM 
        public."AccessDelegations" d
    WHERE 
        d."FromUserId" = p_from_user_id
        AND d."IsActive" = TRUE
        AND d."StartDate" <= CURRENT_TIMESTAMP
        AND d."EndDate" >= CURRENT_TIMESTAMP
    ORDER BY 
        d."EndDate";
END;
$$;

-- Get Active Delegations by ToUserId
CREATE OR REPLACE FUNCTION sp_um_get_active_delegations_by_to_user(
    p_to_user_id INTEGER
)
RETURNS TABLE (
    "DelegationId" INTEGER,
    "FromUserId" INTEGER,
    "ToUserId" INTEGER,
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    "Reason" TEXT,
    "IsActive" BOOLEAN,
    "CreatedBy" INTEGER,
    "DateCreated" TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        d."DelegationId",
        d."FromUserId",
        d."ToUserId",
        d."StartDate",
        d."EndDate",
        d."Reason",
        d."IsActive",
        d."CreatedBy",
        d."DateCreated"
    FROM 
        public."AccessDelegations" d
    WHERE 
        d."ToUserId" = p_to_user_id
        AND d."IsActive" = TRUE
        AND d."StartDate" <= CURRENT_TIMESTAMP
        AND d."EndDate" >= CURRENT_TIMESTAMP
    ORDER BY 
        d."EndDate";
END;
$$;

-- Get All Delegations by FromUserId with Pagination
CREATE OR REPLACE FUNCTION sp_um_get_delegations_by_from_user_paged(
    p_from_user_id INTEGER,
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10,
    p_include_inactive BOOLEAN = FALSE
)
RETURNS TABLE (
    "DelegationId" INTEGER,
    "FromUserId" INTEGER,
    "ToUserId" INTEGER,
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    "Reason" TEXT,
    "IsActive" BOOLEAN,
    "CreatedBy" INTEGER,
    "DateCreated" TIMESTAMP,
    "TotalCount" BIGINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INTEGER;
    v_total_count BIGINT;
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Get total count
    IF p_include_inactive THEN
        SELECT COUNT(*) INTO v_total_count 
        FROM public."AccessDelegations" 
        WHERE "FromUserId" = p_from_user_id;
    ELSE
        SELECT COUNT(*) INTO v_total_count 
        FROM public."AccessDelegations" 
        WHERE "FromUserId" = p_from_user_id AND "IsActive" = TRUE;
    END IF;
    
    RETURN QUERY
    SELECT 
        d."DelegationId",
        d."FromUserId",
        d."ToUserId",
        d."StartDate",
        d."EndDate",
        d."Reason",
        d."IsActive",
        d."CreatedBy",
        d."DateCreated",
        v_total_count AS "TotalCount"
    FROM 
        public."AccessDelegations" d
    WHERE 
        d."FromUserId" = p_from_user_id
        AND (p_include_inactive OR d."IsActive" = TRUE)
    ORDER BY 
        d."DateCreated" DESC
    LIMIT p_page_size
    OFFSET v_offset;
END;
$$;

-- Get All Delegations by ToUserId with Pagination
CREATE OR REPLACE FUNCTION sp_um_get_delegations_by_to_user_paged(
    p_to_user_id INTEGER,
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10,
    p_include_inactive BOOLEAN = FALSE
)
RETURNS TABLE (
    "DelegationId" INTEGER,
    "FromUserId" INTEGER,
    "ToUserId" INTEGER,
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    "Reason" TEXT,
    "IsActive" BOOLEAN,
    "CreatedBy" INTEGER,
    "DateCreated" TIMESTAMP,
    "TotalCount" BIGINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INTEGER;
    v_total_count BIGINT;
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Get total count
    IF p_include_inactive THEN
        SELECT COUNT(*) INTO v_total_count 
        FROM public."AccessDelegations" 
        WHERE "ToUserId" = p_to_user_id;
    ELSE
        SELECT COUNT(*) INTO v_total_count 
        FROM public."AccessDelegations" 
        WHERE "ToUserId" = p_to_user_id AND "IsActive" = TRUE;
    END IF;
    
    RETURN QUERY
    SELECT 
        d."DelegationId",
        d."FromUserId",
        d."ToUserId",
        d."StartDate",
        d."EndDate",
        d."Reason",
        d."IsActive",
        d."CreatedBy",
        d."DateCreated",
        v_total_count AS "TotalCount"
    FROM 
        public."AccessDelegations" d
    WHERE 
        d."ToUserId" = p_to_user_id
        AND (p_include_inactive OR d."IsActive" = TRUE)
    ORDER BY 
        d."DateCreated" DESC
    LIMIT p_page_size
    OFFSET v_offset;
END;
$$;

-- Get Access Delegations by Date Range with Pagination
CREATE OR REPLACE FUNCTION sp_um_get_delegations_by_date_range_paged(
    p_start_date TIMESTAMP,
    p_end_date TIMESTAMP,
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10,
    p_active_only BOOLEAN = FALSE
)
RETURNS TABLE (
    "DelegationId" INTEGER,
    "FromUserId" INTEGER,
    "ToUserId" INTEGER,
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    "Reason" TEXT,
    "IsActive" BOOLEAN,
    "CreatedBy" INTEGER,
    "DateCreated" TIMESTAMP,
    "TotalCount" BIGINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INTEGER;
    v_total_count BIGINT;
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Get total count for this date range
    SELECT COUNT(*) INTO v_total_count 
    FROM public."AccessDelegations"
    WHERE 
        (
            (p_start_date IS NULL OR "EndDate" >= p_start_date) AND
            (p_end_date IS NULL OR "StartDate" <= p_end_date)
        )
        AND (NOT p_active_only OR "IsActive" = TRUE);
    
    RETURN QUERY
    SELECT 
        d."DelegationId",
        d."FromUserId",
        d."ToUserId",
        d."StartDate",
        d."EndDate",
        d."Reason",
        d."IsActive",
        d."CreatedBy",
        d."DateCreated",
        v_total_count AS "TotalCount"
    FROM 
        public."AccessDelegations" d
    WHERE 
        (
            (p_start_date IS NULL OR d."EndDate" >= p_start_date) AND
            (p_end_date IS NULL OR d."StartDate" <= p_end_date)
        )
        AND (NOT p_active_only OR d."IsActive" = TRUE)
    ORDER BY 
        d."StartDate" DESC
    LIMIT p_page_size
    OFFSET v_offset;
END;
$$;

-- Get Current Active Delegations
CREATE OR REPLACE FUNCTION sp_um_get_current_active_delegations(
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10
)
RETURNS TABLE (
    "DelegationId" INTEGER,
    "FromUserId" INTEGER,
    "ToUserId" INTEGER,
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    "Reason" TEXT,
    "IsActive" BOOLEAN,
    "CreatedBy" INTEGER,
    "DateCreated" TIMESTAMP,
    "TotalCount" BIGINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INTEGER;
    v_total_count BIGINT;
    v_current_time TIMESTAMP := CURRENT_TIMESTAMP;
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Get total count of active delegations
    SELECT COUNT(*) INTO v_total_count 
    FROM public."AccessDelegations"
    WHERE 
        "IsActive" = TRUE
        AND "StartDate" <= v_current_time
        AND "EndDate" >= v_current_time;
    
    RETURN QUERY
    SELECT 
        d."DelegationId",
        d."FromUserId",
        d."ToUserId",
        d."StartDate",
        d."EndDate",
        d."Reason",
        d."IsActive",
        d."CreatedBy",
        d."DateCreated",
        v_total_count AS "TotalCount"
    FROM 
        public."AccessDelegations" d
    WHERE 
        d."IsActive" = TRUE
        AND d."StartDate" <= v_current_time
        AND d."EndDate" >= v_current_time
    ORDER BY 
        d."EndDate" ASC
    LIMIT p_page_size
    OFFSET v_offset;
END;
$$;

-- Deactivate Expired Delegations
CREATE OR REPLACE PROCEDURE sp_um_deactivate_expired_delegations(
    INOUT p_delegations_updated INTEGER = 0
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE public."AccessDelegations"
    SET "IsActive" = FALSE
    WHERE 
        "IsActive" = TRUE 
        AND "EndDate" < CURRENT_TIMESTAMP
    RETURNING COUNT(*) INTO p_delegations_updated;
END;
$$;

-- Check if User Has Active Delegation
CREATE OR REPLACE FUNCTION sp_um_check_user_has_active_delegation(
    p_from_user_id INTEGER,
    p_to_user_id INTEGER
)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE
    v_has_delegation BOOLEAN;
BEGIN
    SELECT EXISTS (
        SELECT 1
        FROM public."AccessDelegations" d
        WHERE 
            d."FromUserId" = p_from_user_id
            AND d."ToUserId" = p_to_user_id
            AND d."IsActive" = TRUE
            AND d."StartDate" <= CURRENT_TIMESTAMP
            AND d."EndDate" >= CURRENT_TIMESTAMP
    ) INTO v_has_delegation;
    
    RETURN v_has_delegation;
END;
$$;

-- Get Delegation History by User (both from and to)
CREATE OR REPLACE FUNCTION sp_um_get_delegation_history_by_user(
    p_user_id INTEGER,
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10
)
RETURNS TABLE (
    "DelegationId" INTEGER,
    "FromUserId" INTEGER,
    "ToUserId" INTEGER,
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    "Reason" TEXT,
    "IsActive" BOOLEAN,
    "CreatedBy" INTEGER,
    "DateCreated" TIMESTAMP,
    "DelegationRole" VARCHAR(10),
    "TotalCount" BIGINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INTEGER;
    v_total_count BIGINT;
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Get total count
    SELECT COUNT(*) INTO v_total_count 
    FROM public."AccessDelegations" 
    WHERE "FromUserId" = p_user_id OR "ToUserId" = p_user_id;
    
    RETURN QUERY
    SELECT 
        d."DelegationId",
        d."FromUserId",
        d."ToUserId",
        d."StartDate",
        d."EndDate",
        d."Reason",
        d."IsActive",
        d."CreatedBy",
        d."DateCreated",
        CASE
            WHEN d."FromUserId" = p_user_id THEN 'Delegator'
            ELSE 'Delegate'
        END AS "DelegationRole",
        v_total_count AS "TotalCount"
    FROM 
        public."AccessDelegations" d
    WHERE 
        d."FromUserId" = p_user_id OR d."ToUserId" = p_user_id
    ORDER BY 
        d."DateCreated" DESC
    LIMIT p_page_size
    OFFSET v_offset;
END;
$$;

-- Extend Delegation End Date
CREATE OR REPLACE PROCEDURE sp_um_extend_delegation(
    p_delegation_id INTEGER,
    p_new_end_date TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE public."AccessDelegations"
    SET "EndDate" = p_new_end_date
    WHERE 
        "DelegationId" = p_delegation_id
        AND "IsActive" = TRUE
        AND "EndDate" >= CURRENT_TIMESTAMP;
END;
$$;

-- Advanced Search for Access Delegations
CREATE OR REPLACE FUNCTION sp_um_search_delegations(
    p_from_user_id INTEGER = NULL,
    p_to_user_id INTEGER = NULL,
    p_start_date TIMESTAMP = NULL,
    p_end_date TIMESTAMP = NULL,
    p_is_active BOOLEAN = NULL,
    p_search_text TEXT = NULL,
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10
)
RETURNS TABLE (
    "DelegationId" INTEGER,
    "FromUserId" INTEGER,
    "ToUserId" INTEGER,
    "StartDate" TIMESTAMP,
    "EndDate" TIMESTAMP,
    "Reason" TEXT,
    "IsActive" BOOLEAN,
    "CreatedBy" INTEGER,
    "DateCreated" TIMESTAMP,
    "TotalCount" BIGINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INTEGER;
    v_query TEXT;
    v_count_query TEXT;
    v_total_count BIGINT;
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Base queries
    v_query := 'SELECT d."DelegationId", d."FromUserId", d."ToUserId", d."StartDate", d."EndDate", 
                d."Reason", d."IsActive", d."CreatedBy", d."DateCreated", count(*) OVER() AS "TotalCount"
                FROM public."AccessDelegations" d
                WHERE 1=1';
    
    v_count_query := 'SELECT COUNT(*) FROM public."AccessDelegations" d WHERE 1=1';
    
    -- Add filters based on parameters
    IF p_from_user_id IS NOT NULL THEN
        v_query := v_query || ' AND d."FromUserId" = ' || p_from_user_id;
        v_count_query := v_count_query || ' AND d."FromUserId" = ' || p_from_user_id;
    END IF;
    
    IF p_to_user_id IS NOT NULL THEN
        v_query := v_query || ' AND d."ToUserId" = ' || p_to_user_id;
        v_count_query := v_count_query || ' AND d."ToUserId" = ' || p_to_user_id;
    END IF;
    
    IF p_start_date IS NOT NULL THEN
        v_query := v_query || ' AND d."EndDate" >= ' || quote_literal(p_start_date);
        v_count_query := v_count_query || ' AND d."EndDate" >= ' || quote_literal(p_start_date);
    END IF;
    
    IF p_end_date IS NOT NULL THEN
        v_query := v_query || ' AND d."StartDate" <= ' || quote_literal(p_end_date);
        v_count_query := v_count_query || ' AND d."StartDate" <= ' || quote_literal(p_end_date);
    END IF;
    
    IF p_is_active IS NOT NULL THEN
        v_query := v_query || ' AND d."IsActive" = ' || p_is_active;
        v_count_query := v_count_query || ' AND d."IsActive" = ' || p_is_active;
    END IF;
    
    IF p_search_text IS NOT NULL THEN
        v_query := v_query || ' AND (d."Reason" ILIKE ' || quote_literal('%' || p_search_text || '%') || ')';
        v_count_query := v_count_query || ' AND (d."Reason" ILIKE ' || quote_literal('%' || p_search_text || '%') || ')';
    END IF;
    
    -- Get total count
    EXECUTE v_count_query INTO v_total_count;
    
    -- Complete the query
    v_query := v_query || ' ORDER BY d."DateCreated" DESC LIMIT ' || p_page_size || ' OFFSET ' || v_offset;
    
    -- Return the final result
    RETURN QUERY EXECUTE v_query;
END;
$$;

-- Get Delegation Stats
CREATE OR REPLACE FUNCTION sp_um_get_delegation_stats(
    p_start_date TIMESTAMP = CURRENT_DATE - INTERVAL '30 days',
    p_end_date TIMESTAMP = CURRENT_DATE
)
RETURNS TABLE (
    "TotalDelegations" BIGINT,
    "ActiveDelegations" BIGINT,
    "ExpiredDelegations" BIGINT,
    "UpcomingDelegations" BIGINT,
    "AverageDurationDays" NUMERIC(10,2)
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_now TIMESTAMP := CURRENT_TIMESTAMP;
    v_total BIGINT;
    v_active BIGINT;
    v_expired BIGINT;
    v_upcoming BIGINT;
    v_avg_days NUMERIC(10,2);
BEGIN
    -- Get total count
    SELECT COUNT(*) INTO v_total
    FROM public."AccessDelegations"
    WHERE 
        (p_start_date IS NULL OR "DateCreated" >= p_start_date)
        AND (p_end_date IS NULL OR "DateCreated" <= p_end_date);
    
    -- Get active count
    SELECT COUNT(*) INTO v_active
    FROM public."AccessDelegations"
    WHERE 
        "IsActive" = TRUE
        AND "StartDate" <= v_now
        AND "EndDate" >= v_now
        AND (p_start_date IS NULL OR "DateCreated" >= p_start_date)
        AND (p_end_date IS NULL OR "DateCreated" <= p_end_date);
    
    -- Get expired count
    SELECT COUNT(*) INTO v_expired
    FROM public."AccessDelegations"
    WHERE 
        "EndDate" < v_now
        AND (p_start_date IS NULL OR "DateCreated" >= p_start_date)
        AND (p_end_date IS NULL OR "DateCreated" <= p_end_date);
    
    -- Get upcoming count
    SELECT COUNT(*) INTO v_upcoming
    FROM public."AccessDelegations"
    WHERE 
        "IsActive" = TRUE
        AND "StartDate" > v_now
        AND (p_start_date IS NULL OR "DateCreated" >= p_start_date)
        AND (p_end_date IS NULL OR "DateCreated" <= p_end_date);
    
    -- Get average duration in days
    SELECT COALESCE(AVG(EXTRACT(EPOCH FROM ("EndDate" - "StartDate"))/86400), 0) INTO v_avg_days
    FROM public."AccessDelegations"
    WHERE 
        (p_start_date IS NULL OR "DateCreated" >= p_start_date)
        AND (p_end_date IS NULL OR "DateCreated" <= p_end_date);
    
    RETURN QUERY
    SELECT 
        v_total AS "TotalDelegations",
        v_active AS "ActiveDelegations",
        v_expired AS "ExpiredDelegations",
        v_upcoming AS "UpcomingDelegations",
        v_avg_days AS "AverageDurationDays";
END;
$$;

-- Get Most Active Delegators
CREATE OR REPLACE FUNCTION sp_um_get_most_active_delegators(
    p_limit INTEGER = 10,
    p_start_date TIMESTAMP = NULL,
    p_end_date TIMESTAMP = NULL
)
RETURNS TABLE (
    "UserId" INTEGER,
    "DelegationCount" BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        d."FromUserId" AS "UserId",
        COUNT(*) AS "DelegationCount"
    FROM 
        public."AccessDelegations" d
    WHERE 
        (p_start_date IS NULL OR d."DateCreated" >= p_start_date)
        AND (p_end_date IS NULL OR d."DateCreated" <= p_end_date)
    GROUP BY 
        d."FromUserId"
    ORDER BY 
        "DelegationCount" DESC
    LIMIT p_limit;
END;
$$;

-- Get Most Popular Delegates
CREATE OR REPLACE FUNCTION sp_um_get_most_popular_delegates(
    p_limit INTEGER = 10,
    p_start_date TIMESTAMP = NULL,
    p_end_date TIMESTAMP = NULL
)
RETURNS TABLE (
    "UserId" INTEGER,
    "DelegationCount" BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        d."ToUserId" AS "UserId",
        COUNT(*) AS "DelegationCount"
    FROM 
        public."AccessDelegations" d
    WHERE 
        (p_start_date IS NULL OR d."DateCreated" >= p_start_date)
        AND (p_end_date IS NULL OR d."DateCreated" <= p_end_date)
    GROUP BY 
        d."ToUserId"
    ORDER BY 
        "DelegationCount" DESC
    LIMIT p_limit;
END;
$$;
