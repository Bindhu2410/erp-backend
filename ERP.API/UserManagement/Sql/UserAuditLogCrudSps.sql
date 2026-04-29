-- UserAuditLog CRUD Stored Procedures

-- Create (Insert) User Audit Log
CREATE OR REPLACE PROCEDURE sp_insert_user_audit_log(
    p_user_id INTEGER,
    p_action_type VARCHAR(50),
    p_entity_type VARCHAR(50) = NULL,
    p_entity_id INTEGER = NULL,
    p_description TEXT = NULL,
    p_old_value TEXT = NULL,
    p_new_value TEXT = NULL,
    p_ip_address VARCHAR(50) = NULL,
    INOUT p_audit_id INTEGER = NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO public."UserAuditLog"(
        "UserId",
        "ActionType",
        "EntityType",
        "EntityId",
        "Description",
        "OldValue",
        "NewValue",
        "IpAddress",
        "ActionTime"
    ) VALUES (
        p_user_id,
        p_action_type,
        p_entity_type,
        p_entity_id,
        p_description,
        p_old_value,
        p_new_value,
        p_ip_address,
        CURRENT_TIMESTAMP
    ) RETURNING "AuditId" INTO p_audit_id;
END;
$$;

-- Read (Get) User Audit Log by ID
CREATE OR REPLACE FUNCTION sp_get_user_audit_log_by_id(
    p_audit_id INTEGER
)
RETURNS TABLE (
    "AuditId" INTEGER,
    "UserId" INTEGER,
    "ActionType" VARCHAR(50),
    "EntityType" VARCHAR(50),
    "EntityId" INTEGER,
    "Description" TEXT,
    "OldValue" TEXT,
    "NewValue" TEXT,
    "IpAddress" VARCHAR(50),
    "ActionTime" TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a."AuditId",
        a."UserId",
        a."ActionType",
        a."EntityType",
        a."EntityId",
        a."Description",
        a."OldValue",
        a."NewValue",
        a."IpAddress",
        a."ActionTime"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."AuditId" = p_audit_id;
END;
$$;

-- Update User Audit Log
CREATE OR REPLACE PROCEDURE sp_update_user_audit_log(
    p_audit_id INTEGER,
    p_description TEXT = NULL,
    p_old_value TEXT = NULL,
    p_new_value TEXT = NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE public."UserAuditLog"
    SET 
        "Description" = COALESCE(p_description, "Description"),
        "OldValue" = COALESCE(p_old_value, "OldValue"),
        "NewValue" = COALESCE(p_new_value, "NewValue")
    WHERE 
        "AuditId" = p_audit_id;
        
    -- Note: We intentionally limit what fields can be updated since audit logs
    -- should generally be immutable except for specific fields that might need correction
END;
$$;

-- Delete User Audit Log
CREATE OR REPLACE PROCEDURE sp_delete_user_audit_log(
    p_audit_id INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM public."UserAuditLog"
    WHERE "AuditId" = p_audit_id;
END;
$$;

-- Get User Audit Logs with Pagination
CREATE OR REPLACE FUNCTION sp_get_user_audit_logs_paged(
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10
)
RETURNS TABLE (
    "AuditId" INTEGER,
    "UserId" INTEGER,
    "ActionType" VARCHAR(50),
    "EntityType" VARCHAR(50),
    "EntityId" INTEGER,
    "Description" TEXT,
    "OldValue" TEXT,
    "NewValue" TEXT,
    "IpAddress" VARCHAR(50),
    "ActionTime" TIMESTAMP,
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
    SELECT COUNT(*) INTO v_total_count FROM public."UserAuditLog";
    
    RETURN QUERY
    SELECT 
        a."AuditId",
        a."UserId",
        a."ActionType",
        a."EntityType",
        a."EntityId",
        a."Description",
        a."OldValue",
        a."NewValue",
        a."IpAddress",
        a."ActionTime",
        v_total_count AS "TotalCount"
    FROM 
        public."UserAuditLog" a
    ORDER BY 
        a."ActionTime" DESC
    LIMIT p_page_size
    OFFSET v_offset;
END;
$$;

-- Get User Audit Logs By User ID with Pagination
CREATE OR REPLACE FUNCTION sp_get_user_audit_logs_by_user_id_paged(
    p_user_id INTEGER,
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10
)
RETURNS TABLE (
    "AuditId" INTEGER,
    "UserId" INTEGER,
    "ActionType" VARCHAR(50),
    "EntityType" VARCHAR(50),
    "EntityId" INTEGER,
    "Description" TEXT,
    "OldValue" TEXT,
    "NewValue" TEXT,
    "IpAddress" VARCHAR(50),
    "ActionTime" TIMESTAMP,
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
    
    -- Get total count for this user
    SELECT COUNT(*) INTO v_total_count 
    FROM public."UserAuditLog"
    WHERE "UserId" = p_user_id;
    
    RETURN QUERY
    SELECT 
        a."AuditId",
        a."UserId",
        a."ActionType",
        a."EntityType",
        a."EntityId",
        a."Description",
        a."OldValue",
        a."NewValue",
        a."IpAddress",
        a."ActionTime",
        v_total_count AS "TotalCount"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."UserId" = p_user_id
    ORDER BY 
        a."ActionTime" DESC
    LIMIT p_page_size
    OFFSET v_offset;
END;
$$;

-- Get User Audit Logs By Entity Type and ID with Pagination
CREATE OR REPLACE FUNCTION sp_get_user_audit_logs_by_entity_paged(
    p_entity_type VARCHAR(50),
    p_entity_id INTEGER,
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10
)
RETURNS TABLE (
    "AuditId" INTEGER,
    "UserId" INTEGER,
    "ActionType" VARCHAR(50),
    "EntityType" VARCHAR(50),
    "EntityId" INTEGER,
    "Description" TEXT,
    "OldValue" TEXT,
    "NewValue" TEXT,
    "IpAddress" VARCHAR(50),
    "ActionTime" TIMESTAMP,
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
    
    -- Get total count for this entity
    SELECT COUNT(*) INTO v_total_count 
    FROM public."UserAuditLog"
    WHERE "EntityType" = p_entity_type AND "EntityId" = p_entity_id;
    
    RETURN QUERY
    SELECT 
        a."AuditId",
        a."UserId",
        a."ActionType",
        a."EntityType",
        a."EntityId",
        a."Description",
        a."OldValue",
        a."NewValue",
        a."IpAddress",
        a."ActionTime",
        v_total_count AS "TotalCount"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."EntityType" = p_entity_type AND a."EntityId" = p_entity_id
    ORDER BY 
        a."ActionTime" DESC
    LIMIT p_page_size
    OFFSET v_offset;
END;
$$;

-- Get User Audit Logs By Date Range with Pagination
CREATE OR REPLACE FUNCTION sp_get_user_audit_logs_by_date_range_paged(
    p_start_date TIMESTAMP,
    p_end_date TIMESTAMP,
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10
)
RETURNS TABLE (
    "AuditId" INTEGER,
    "UserId" INTEGER,
    "ActionType" VARCHAR(50),
    "EntityType" VARCHAR(50),
    "EntityId" INTEGER,
    "Description" TEXT,
    "OldValue" TEXT,
    "NewValue" TEXT,
    "IpAddress" VARCHAR(50),
    "ActionTime" TIMESTAMP,
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
    FROM public."UserAuditLog"
    WHERE "ActionTime" BETWEEN p_start_date AND p_end_date;
    
    RETURN QUERY
    SELECT 
        a."AuditId",
        a."UserId",
        a."ActionType",
        a."EntityType",
        a."EntityId",
        a."Description",
        a."OldValue",
        a."NewValue",
        a."IpAddress",
        a."ActionTime",
        v_total_count AS "TotalCount"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."ActionTime" BETWEEN p_start_date AND p_end_date
    ORDER BY 
        a."ActionTime" DESC
    LIMIT p_page_size
    OFFSET v_offset;
END;
$$;

-- Get User Audit Logs By Action Type with Pagination
CREATE OR REPLACE FUNCTION sp_get_user_audit_logs_by_action_type_paged(
    p_action_type VARCHAR(50),
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10
)
RETURNS TABLE (
    "AuditId" INTEGER,
    "UserId" INTEGER,
    "ActionType" VARCHAR(50),
    "EntityType" VARCHAR(50),
    "EntityId" INTEGER,
    "Description" TEXT,
    "OldValue" TEXT,
    "NewValue" TEXT,
    "IpAddress" VARCHAR(50),
    "ActionTime" TIMESTAMP,
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
    
    -- Get total count for this action type
    SELECT COUNT(*) INTO v_total_count 
    FROM public."UserAuditLog"
    WHERE "ActionType" = p_action_type;
    
    RETURN QUERY
    SELECT 
        a."AuditId",
        a."UserId",
        a."ActionType",
        a."EntityType",
        a."EntityId",
        a."Description",
        a."OldValue",
        a."NewValue",
        a."IpAddress",
        a."ActionTime",
        v_total_count AS "TotalCount"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."ActionType" = p_action_type
    ORDER BY 
        a."ActionTime" DESC
    LIMIT p_page_size
    OFFSET v_offset;
END;
$$;

-- Get User Audit Logs By IP Address with Pagination
CREATE OR REPLACE FUNCTION sp_get_user_audit_logs_by_ip_address_paged(
    p_ip_address VARCHAR(50),
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10
)
RETURNS TABLE (
    "AuditId" INTEGER,
    "UserId" INTEGER,
    "ActionType" VARCHAR(50),
    "EntityType" VARCHAR(50),
    "EntityId" INTEGER,
    "Description" TEXT,
    "OldValue" TEXT,
    "NewValue" TEXT,
    "IpAddress" VARCHAR(50),
    "ActionTime" TIMESTAMP,
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
    
    -- Get total count for this IP address
    SELECT COUNT(*) INTO v_total_count 
    FROM public."UserAuditLog"
    WHERE "IpAddress" = p_ip_address;
    
    RETURN QUERY
    SELECT 
        a."AuditId",
        a."UserId",
        a."ActionType",
        a."EntityType",
        a."EntityId",
        a."Description",
        a."OldValue",
        a."NewValue",
        a."IpAddress",
        a."ActionTime",
        v_total_count AS "TotalCount"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."IpAddress" = p_ip_address
    ORDER BY 
        a."ActionTime" DESC
    LIMIT p_page_size
    OFFSET v_offset;
END;
$$;

-- Get User Activity Summary by User ID
CREATE OR REPLACE FUNCTION sp_get_user_activity_summary(
    p_user_id INTEGER,
    p_start_date TIMESTAMP = NULL,
    p_end_date TIMESTAMP = NULL
)
RETURNS TABLE (
    "ActionType" VARCHAR(50),
    "ActionCount" BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a."ActionType",
        COUNT(*) AS "ActionCount"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."UserId" = p_user_id
        AND (p_start_date IS NULL OR a."ActionTime" >= p_start_date)
        AND (p_end_date IS NULL OR a."ActionTime" <= p_end_date)
    GROUP BY 
        a."ActionType"
    ORDER BY 
        "ActionCount" DESC;
END;
$$;

-- Get User Audit Logs by Advanced Search with Pagination
CREATE OR REPLACE FUNCTION sp_get_user_audit_logs_advanced_search(
    p_user_id INTEGER = NULL,
    p_action_type VARCHAR(50) = NULL,
    p_entity_type VARCHAR(50) = NULL,
    p_entity_id INTEGER = NULL,
    p_start_date TIMESTAMP = NULL,
    p_end_date TIMESTAMP = NULL,
    p_ip_address VARCHAR(50) = NULL,
    p_search_text TEXT = NULL,
    p_page_number INTEGER = 1,
    p_page_size INTEGER = 10
)
RETURNS TABLE (
    "AuditId" INTEGER,
    "UserId" INTEGER,
    "ActionType" VARCHAR(50),
    "EntityType" VARCHAR(50),
    "EntityId" INTEGER,
    "Description" TEXT,
    "OldValue" TEXT,
    "NewValue" TEXT,
    "IpAddress" VARCHAR(50),
    "ActionTime" TIMESTAMP,
    "TotalCount" BIGINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INTEGER;
    v_total_count BIGINT;
    v_query TEXT;
    v_count_query TEXT;
    v_params TEXT := '';
BEGIN
    -- Calculate offset
    v_offset := (p_page_number - 1) * p_page_size;
    
    -- Base queries
    v_query := 'SELECT a."AuditId", a."UserId", a."ActionType", a."EntityType", a."EntityId", 
                a."Description", a."OldValue", a."NewValue", a."IpAddress", a."ActionTime", 
                count(*) OVER() AS "TotalCount"
                FROM public."UserAuditLog" a
                WHERE 1=1';
    
    v_count_query := 'SELECT COUNT(*) FROM public."UserAuditLog" a WHERE 1=1';
    
    -- Add filters based on parameters
    IF p_user_id IS NOT NULL THEN
        v_query := v_query || ' AND a."UserId" = ' || p_user_id;
        v_count_query := v_count_query || ' AND a."UserId" = ' || p_user_id;
    END IF;
    
    IF p_action_type IS NOT NULL THEN
        v_query := v_query || ' AND a."ActionType" = ' || quote_literal(p_action_type);
        v_count_query := v_count_query || ' AND a."ActionType" = ' || quote_literal(p_action_type);
    END IF;
    
    IF p_entity_type IS NOT NULL THEN
        v_query := v_query || ' AND a."EntityType" = ' || quote_literal(p_entity_type);
        v_count_query := v_count_query || ' AND a."EntityType" = ' || quote_literal(p_entity_type);
    END IF;
    
    IF p_entity_id IS NOT NULL THEN
        v_query := v_query || ' AND a."EntityId" = ' || p_entity_id;
        v_count_query := v_count_query || ' AND a."EntityId" = ' || p_entity_id;
    END IF;
    
    IF p_start_date IS NOT NULL THEN
        v_query := v_query || ' AND a."ActionTime" >= ' || quote_literal(p_start_date);
        v_count_query := v_count_query || ' AND a."ActionTime" >= ' || quote_literal(p_start_date);
    END IF;
    
    IF p_end_date IS NOT NULL THEN
        v_query := v_query || ' AND a."ActionTime" <= ' || quote_literal(p_end_date);
        v_count_query := v_count_query || ' AND a."ActionTime" <= ' || quote_literal(p_end_date);
    END IF;
    
    IF p_ip_address IS NOT NULL THEN
        v_query := v_query || ' AND a."IpAddress" = ' || quote_literal(p_ip_address);
        v_count_query := v_count_query || ' AND a."IpAddress" = ' || quote_literal(p_ip_address);
    END IF;
    
    IF p_search_text IS NOT NULL THEN
        v_query := v_query || ' AND (a."Description" ILIKE ' || quote_literal('%' || p_search_text || '%') || 
                    ' OR a."OldValue" ILIKE ' || quote_literal('%' || p_search_text || '%') || 
                    ' OR a."NewValue" ILIKE ' || quote_literal('%' || p_search_text || '%') || ')';
                    
        v_count_query := v_count_query || ' AND (a."Description" ILIKE ' || quote_literal('%' || p_search_text || '%') || 
                    ' OR a."OldValue" ILIKE ' || quote_literal('%' || p_search_text || '%') || 
                    ' OR a."NewValue" ILIKE ' || quote_literal('%' || p_search_text || '%') || ')';
    END IF;
    
    -- Get total count
    EXECUTE v_count_query INTO v_total_count;
    
    -- Complete the query
    v_query := v_query || ' ORDER BY a."ActionTime" DESC LIMIT ' || p_page_size || ' OFFSET ' || v_offset;
    
    -- Return the final result
    RETURN QUERY EXECUTE v_query;
END;
$$;

-- Clean Up Old Audit Logs
CREATE OR REPLACE PROCEDURE sp_cleanup_old_audit_logs(
    p_days_old INTEGER = 365,
    INOUT p_rows_deleted INTEGER = 0
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_cutoff_date TIMESTAMP;
BEGIN
    -- Calculate cutoff date
    v_cutoff_date := CURRENT_TIMESTAMP - (p_days_old || ' days')::INTERVAL;
    
    -- Delete old records and get count
    DELETE FROM public."UserAuditLog"
    WHERE "ActionTime" < v_cutoff_date
    RETURNING COUNT(*) INTO p_rows_deleted;
END;
$$;

-- Bulk Insert Audit Logs (useful for migration or batch processing)
CREATE OR REPLACE PROCEDURE sp_bulk_insert_audit_logs(
    p_user_ids INTEGER[],
    p_action_types VARCHAR(50)[],
    p_entity_types VARCHAR(50)[],
    p_entity_ids INTEGER[],
    p_descriptions TEXT[],
    p_old_values TEXT[],
    p_new_values TEXT[],
    p_ip_addresses VARCHAR(50)[],
    p_action_times TIMESTAMP[],
    INOUT p_inserted_count INTEGER = 0
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_array_length INTEGER;
    i INTEGER;
BEGIN
    -- Get array length
    v_array_length := array_length(p_user_ids, 1);
    
    -- Validate array lengths match
    IF array_length(p_action_types, 1) != v_array_length OR
       (p_entity_types IS NOT NULL AND array_length(p_entity_types, 1) != v_array_length) OR
       (p_entity_ids IS NOT NULL AND array_length(p_entity_ids, 1) != v_array_length) OR
       (p_descriptions IS NOT NULL AND array_length(p_descriptions, 1) != v_array_length) OR
       (p_old_values IS NOT NULL AND array_length(p_old_values, 1) != v_array_length) OR
       (p_new_values IS NOT NULL AND array_length(p_new_values, 1) != v_array_length) OR
       (p_ip_addresses IS NOT NULL AND array_length(p_ip_addresses, 1) != v_array_length) OR
       (p_action_times IS NOT NULL AND array_length(p_action_times, 1) != v_array_length) THEN
        RAISE EXCEPTION 'Array lengths do not match';
    END IF;
    
    -- Insert each record
    p_inserted_count := 0;
    FOR i IN 1..v_array_length LOOP
        INSERT INTO public."UserAuditLog"(
            "UserId",
            "ActionType",
            "EntityType",
            "EntityId",
            "Description",
            "OldValue",
            "NewValue",
            "IpAddress",
            "ActionTime"
        ) VALUES (
            p_user_ids[i],
            p_action_types[i],
            CASE WHEN p_entity_types IS NULL THEN NULL ELSE p_entity_types[i] END,
            CASE WHEN p_entity_ids IS NULL THEN NULL ELSE p_entity_ids[i] END,
            CASE WHEN p_descriptions IS NULL THEN NULL ELSE p_descriptions[i] END,
            CASE WHEN p_old_values IS NULL THEN NULL ELSE p_old_values[i] END,
            CASE WHEN p_new_values IS NULL THEN NULL ELSE p_new_values[i] END,
            CASE WHEN p_ip_addresses IS NULL THEN NULL ELSE p_ip_addresses[i] END,
            CASE WHEN p_action_times IS NULL THEN CURRENT_TIMESTAMP ELSE p_action_times[i] END
        );
        
        p_inserted_count := p_inserted_count + 1;
    END LOOP;
END;
$$;

-- Get Daily Audit Log Activity Count
CREATE OR REPLACE FUNCTION sp_get_daily_audit_log_activity(
    p_start_date TIMESTAMP = CURRENT_DATE - INTERVAL '30 days',
    p_end_date TIMESTAMP = CURRENT_DATE
)
RETURNS TABLE (
    "Date" DATE,
    "ActionCount" BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        DATE(a."ActionTime") AS "Date",
        COUNT(*) AS "ActionCount"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."ActionTime" BETWEEN p_start_date AND p_end_date
    GROUP BY 
        DATE(a."ActionTime")
    ORDER BY 
        "Date";
END;
$$;

-- Get Most Active Users in a Date Range
CREATE OR REPLACE FUNCTION sp_get_most_active_users(
    p_start_date TIMESTAMP = CURRENT_DATE - INTERVAL '30 days',
    p_end_date TIMESTAMP = CURRENT_DATE,
    p_limit INTEGER = 10
)
RETURNS TABLE (
    "UserId" INTEGER,
    "ActionCount" BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a."UserId",
        COUNT(*) AS "ActionCount"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."ActionTime" BETWEEN p_start_date AND p_end_date
    GROUP BY 
        a."UserId"
    ORDER BY 
        "ActionCount" DESC
    LIMIT p_limit;
END;
$$;

-- Get Action Type Distribution
CREATE OR REPLACE FUNCTION sp_get_action_type_distribution(
    p_start_date TIMESTAMP = CURRENT_DATE - INTERVAL '30 days',
    p_end_date TIMESTAMP = CURRENT_DATE
)
RETURNS TABLE (
    "ActionType" VARCHAR(50),
    "ActionCount" BIGINT,
    "Percentage" NUMERIC(5,2)
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_total_count BIGINT;
BEGIN
    -- Get total count
    SELECT COUNT(*) INTO v_total_count
    FROM public."UserAuditLog" a
    WHERE a."ActionTime" BETWEEN p_start_date AND p_end_date;
    
    -- Return action type distribution with percentages
    RETURN QUERY
    SELECT 
        a."ActionType",
        COUNT(*) AS "ActionCount",
        ROUND((COUNT(*) * 100.0 / NULLIF(v_total_count, 0))::NUMERIC, 2) AS "Percentage"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."ActionTime" BETWEEN p_start_date AND p_end_date
    GROUP BY 
        a."ActionType"
    ORDER BY 
        "ActionCount" DESC;
END;
$$;

-- Get Entity Activity Summary
CREATE OR REPLACE FUNCTION sp_get_entity_activity_summary(
    p_entity_type VARCHAR(50),
    p_start_date TIMESTAMP = CURRENT_DATE - INTERVAL '30 days',
    p_end_date TIMESTAMP = CURRENT_DATE,
    p_limit INTEGER = 10
)
RETURNS TABLE (
    "EntityId" INTEGER,
    "ActionCount" BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a."EntityId",
        COUNT(*) AS "ActionCount"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."EntityType" = p_entity_type
        AND a."EntityId" IS NOT NULL
        AND a."ActionTime" BETWEEN p_start_date AND p_end_date
    GROUP BY 
        a."EntityId"
    ORDER BY 
        "ActionCount" DESC
    LIMIT p_limit;
END;
$$;

-- Get Recent User Activity with Extended Info
CREATE OR REPLACE FUNCTION sp_get_recent_user_activity(
    p_user_id INTEGER,
    p_limit INTEGER = 10
)
RETURNS TABLE (
    "AuditId" INTEGER,
    "ActionType" VARCHAR(50),
    "EntityType" VARCHAR(50),
    "EntityId" INTEGER,
    "Description" TEXT,
    "ActionTime" TIMESTAMP,
    "DaysAgo" INTEGER,
    "HoursAgo" INTEGER,
    "MinutesAgo" INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a."AuditId",
        a."ActionType",
        a."EntityType",
        a."EntityId",
        a."Description",
        a."ActionTime",
        EXTRACT(DAY FROM (CURRENT_TIMESTAMP - a."ActionTime"))::INTEGER AS "DaysAgo",
        EXTRACT(HOUR FROM (CURRENT_TIMESTAMP - a."ActionTime"))::INTEGER % 24 AS "HoursAgo",
        EXTRACT(MINUTE FROM (CURRENT_TIMESTAMP - a."ActionTime"))::INTEGER % 60 AS "MinutesAgo"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."UserId" = p_user_id
    ORDER BY 
        a."ActionTime" DESC
    LIMIT p_limit;
END;
$$;

-- Get User Activity Timeline for a Specific Entity
CREATE OR REPLACE FUNCTION sp_get_entity_history(
    p_entity_type VARCHAR(50),
    p_entity_id INTEGER
)
RETURNS TABLE (
    "AuditId" INTEGER,
    "UserId" INTEGER,
    "ActionType" VARCHAR(50),
    "Description" TEXT,
    "OldValue" TEXT,
    "NewValue" TEXT,
    "ActionTime" TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a."AuditId",
        a."UserId",
        a."ActionType",
        a."Description",
        a."OldValue",
        a."NewValue",
        a."ActionTime"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."EntityType" = p_entity_type
        AND a."EntityId" = p_entity_id
    ORDER BY 
        a."ActionTime" DESC;
END;
$$;

-- Get User Session Activity Summary (for security monitoring)
CREATE OR REPLACE FUNCTION sp_get_user_session_activity(
    p_user_id INTEGER = NULL,
    p_start_date TIMESTAMP = CURRENT_DATE - INTERVAL '30 days',
    p_end_date TIMESTAMP = CURRENT_DATE
)
RETURNS TABLE (
    "IpAddress" VARCHAR(50),
    "LoginCount" BIGINT,
    "LastLogin" TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a."IpAddress",
        COUNT(*) AS "LoginCount",
        MAX(a."ActionTime") AS "LastLogin"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."ActionType" = 'Login'
        AND (p_user_id IS NULL OR a."UserId" = p_user_id)
        AND a."ActionTime" BETWEEN p_start_date AND p_end_date
        AND a."IpAddress" IS NOT NULL
    GROUP BY 
        a."IpAddress"
    ORDER BY 
        "LastLogin" DESC;
END;
$$;

-- Get User Failed Login Attempts (for security monitoring)
CREATE OR REPLACE FUNCTION sp_get_failed_login_attempts(
    p_user_id INTEGER = NULL,
    p_start_date TIMESTAMP = CURRENT_DATE - INTERVAL '7 days',
    p_end_date TIMESTAMP = CURRENT_DATE
)
RETURNS TABLE (
    "UserId" INTEGER,
    "IpAddress" VARCHAR(50),
    "FailureCount" BIGINT,
    "LastFailure" TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a."UserId",
        a."IpAddress",
        COUNT(*) AS "FailureCount",
        MAX(a."ActionTime") AS "LastFailure"
    FROM 
        public."UserAuditLog" a
    WHERE 
        a."ActionType" = 'FailedLogin'
        AND (p_user_id IS NULL OR a."UserId" = p_user_id)
        AND a."ActionTime" BETWEEN p_start_date AND p_end_date
    GROUP BY 
        a."UserId", a."IpAddress"
    ORDER BY 
        "FailureCount" DESC, "LastFailure" DESC;
END;
$$;
