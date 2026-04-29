-- =============================================
-- Author:      GitHub Copilot
-- Create date: 2025-07-03
-- Description: CRUD Stored Procedures for UserSessions
-- =============================================

-- Create UserSession stored procedure
CREATE OR REPLACE PROCEDURE public.usp_CreateUserSession(
    p_SessionId VARCHAR(100),
    p_UserId INT,
    p_IpAddress VARCHAR(50) = NULL,
    p_DeviceInfo VARCHAR(255) = NULL,
    p_UserAgent TEXT = NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO public."UserSessions" (
        "SessionId",
        "UserId",
        "LoginTime",
        "IpAddress",
        "DeviceInfo",
        "UserAgent",
        "IsActive"
    ) VALUES (
        p_SessionId,
        p_UserId,
        CURRENT_TIMESTAMP,
        p_IpAddress,
        p_DeviceInfo,
        p_UserAgent,
        TRUE
    );
END;
$$;

-- Get UserSession by SessionId stored procedure
CREATE OR REPLACE FUNCTION public.usp_GetUserSessionById(
    p_SessionId VARCHAR(100)
)
RETURNS TABLE (
    "SessionId" VARCHAR(100),
    "UserId" INT,
    "LoginTime" TIMESTAMP,
    "LogoutTime" TIMESTAMP,
    "IpAddress" VARCHAR(50),
    "DeviceInfo" VARCHAR(255),
    "UserAgent" TEXT,
    "IsActive" BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        us."SessionId",
        us."UserId",
        us."LoginTime",
        us."LogoutTime",
        us."IpAddress",
        us."DeviceInfo",
        us."UserAgent",
        us."IsActive"
    FROM
        public."UserSessions" us
    WHERE
        us."SessionId" = p_SessionId;
END;
$$;

-- Get UserSessions by UserId stored procedure
CREATE OR REPLACE FUNCTION public.usp_GetUserSessionsByUserId(
    p_UserId INT,
    p_ActiveOnly BOOLEAN = FALSE
)
RETURNS TABLE (
    "SessionId" VARCHAR(100),
    "UserId" INT,
    "LoginTime" TIMESTAMP,
    "LogoutTime" TIMESTAMP,
    "IpAddress" VARCHAR(50),
    "DeviceInfo" VARCHAR(255),
    "UserAgent" TEXT,
    "IsActive" BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        us."SessionId",
        us."UserId",
        us."LoginTime",
        us."LogoutTime",
        us."IpAddress",
        us."DeviceInfo",
        us."UserAgent",
        us."IsActive"
    FROM
        public."UserSessions" us
    WHERE
        us."UserId" = p_UserId
        AND (NOT p_ActiveOnly OR us."IsActive" = TRUE)
    ORDER BY
        us."LoginTime" DESC;
END;
$$;

-- Get All Active UserSessions stored procedure
CREATE OR REPLACE FUNCTION public.usp_GetAllActiveSessions()
RETURNS TABLE (
    "SessionId" VARCHAR(100),
    "UserId" INT,
    "LoginTime" TIMESTAMP,
    "LogoutTime" TIMESTAMP,
    "IpAddress" VARCHAR(50),
    "DeviceInfo" VARCHAR(255),
    "UserAgent" TEXT,
    "IsActive" BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        us."SessionId",
        us."UserId",
        us."LoginTime",
        us."LogoutTime",
        us."IpAddress",
        us."DeviceInfo",
        us."UserAgent",
        us."IsActive"
    FROM
        public."UserSessions" us
    WHERE
        us."IsActive" = TRUE
    ORDER BY
        us."LoginTime" DESC;
END;
$$;

-- Update UserSession stored procedure (Mark as inactive and set LogoutTime)
CREATE OR REPLACE PROCEDURE public.usp_EndUserSession(
    p_SessionId VARCHAR(100)
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE public."UserSessions"
    SET
        "LogoutTime" = CURRENT_TIMESTAMP,
        "IsActive" = FALSE
    WHERE
        "SessionId" = p_SessionId;
END;
$$;

-- Update UserSession stored procedure (Full update)
CREATE OR REPLACE PROCEDURE public.usp_UpdateUserSession(
    p_SessionId VARCHAR(100),
    p_LogoutTime TIMESTAMP = NULL,
    p_IpAddress VARCHAR(50) = NULL,
    p_DeviceInfo VARCHAR(255) = NULL,
    p_UserAgent TEXT = NULL,
    p_IsActive BOOLEAN = NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_CurrentIsActive BOOLEAN;
    v_CurrentLogoutTime TIMESTAMP;
BEGIN
    -- Get current values
    SELECT "IsActive", "LogoutTime"
    INTO v_CurrentIsActive, v_CurrentLogoutTime
    FROM public."UserSessions"
    WHERE "SessionId" = p_SessionId;
    
    -- Update the session
    UPDATE public."UserSessions"
    SET
        "LogoutTime" = COALESCE(p_LogoutTime, v_CurrentLogoutTime),
        "IpAddress" = COALESCE(p_IpAddress, "IpAddress"),
        "DeviceInfo" = COALESCE(p_DeviceInfo, "DeviceInfo"),
        "UserAgent" = COALESCE(p_UserAgent, "UserAgent"),
        "IsActive" = COALESCE(p_IsActive, v_CurrentIsActive)
    WHERE
        "SessionId" = p_SessionId;

    -- If marking as inactive and no logout time is provided, set logout time to current timestamp
    IF p_IsActive = FALSE AND p_LogoutTime IS NULL THEN
        UPDATE public."UserSessions"
        SET "LogoutTime" = CURRENT_TIMESTAMP
        WHERE "SessionId" = p_SessionId AND "LogoutTime" IS NULL;
    END IF;
END;
$$;

-- Delete UserSession stored procedure
CREATE OR REPLACE PROCEDURE public.usp_DeleteUserSession(
    p_SessionId VARCHAR(100)
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM public."UserSessions"
    WHERE "SessionId" = p_SessionId;
END;
$$;

-- Delete All UserSessions for a User stored procedure
CREATE OR REPLACE PROCEDURE public.usp_DeleteAllUserSessionsByUserId(
    p_UserId INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM public."UserSessions"
    WHERE "UserId" = p_UserId;
END;
$$;

-- Delete Expired UserSessions stored procedure
CREATE OR REPLACE PROCEDURE public.usp_CleanupInactiveSessions(
    p_OlderThanDays INT = 30
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM public."UserSessions"
    WHERE 
        "IsActive" = FALSE 
        AND "LogoutTime" < (CURRENT_TIMESTAMP - (p_OlderThanDays || ' days')::INTERVAL);
END;
$$;

-- Get UserSession Statistics stored procedure
CREATE OR REPLACE FUNCTION public.usp_GetUserSessionStats()
RETURNS TABLE (
    "TotalSessions" BIGINT,
    "ActiveSessions" BIGINT,
    "UniqueUsers" BIGINT,
    "ActiveUsers" BIGINT,
    "AverageSessionDurationMinutes" NUMERIC
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*)::BIGINT AS "TotalSessions",
        SUM(CASE WHEN "IsActive" = TRUE THEN 1 ELSE 0 END)::BIGINT AS "ActiveSessions",
        COUNT(DISTINCT "UserId")::BIGINT AS "UniqueUsers",
        COUNT(DISTINCT CASE WHEN "IsActive" = TRUE THEN "UserId" ELSE NULL END)::BIGINT AS "ActiveUsers",
        AVG(
            EXTRACT(EPOCH FROM (
                COALESCE("LogoutTime", CURRENT_TIMESTAMP) - "LoginTime"
            )) / 60
        )::NUMERIC AS "AverageSessionDurationMinutes"
    FROM
        public."UserSessions";
END;
$$;

-- Get User's Session History stored procedure
CREATE OR REPLACE FUNCTION public.usp_GetUserSessionHistory(
    p_UserId INT,
    p_StartDate TIMESTAMP = NULL,
    p_EndDate TIMESTAMP = NULL,
    p_Limit INT = 100
)
RETURNS TABLE (
    "SessionId" VARCHAR(100),
    "LoginTime" TIMESTAMP,
    "LogoutTime" TIMESTAMP,
    "IpAddress" VARCHAR(50),
    "DeviceInfo" VARCHAR(255),
    "IsActive" BOOLEAN,
    "SessionDurationMinutes" NUMERIC
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        us."SessionId",
        us."LoginTime",
        us."LogoutTime",
        us."IpAddress",
        us."DeviceInfo",
        us."IsActive",
        EXTRACT(EPOCH FROM (
            COALESCE(us."LogoutTime", CURRENT_TIMESTAMP) - us."LoginTime"
        )) / 60 AS "SessionDurationMinutes"
    FROM
        public."UserSessions" us
    WHERE
        us."UserId" = p_UserId
        AND (p_StartDate IS NULL OR us."LoginTime" >= p_StartDate)
        AND (p_EndDate IS NULL OR us."LoginTime" <= p_EndDate)
    ORDER BY
        us."LoginTime" DESC
    LIMIT p_Limit;
END;
$$;

-- Get Recently Active Sessions stored procedure
CREATE OR REPLACE FUNCTION public.usp_GetRecentActiveSessions(
    p_Minutes INT = 30,
    p_Limit INT = 100
)
RETURNS TABLE (
    "SessionId" VARCHAR(100),
    "UserId" INT,
    "LoginTime" TIMESTAMP,
    "IpAddress" VARCHAR(50),
    "DeviceInfo" VARCHAR(255),
    "IsActive" BOOLEAN,
    "LastActivityMinutesAgo" NUMERIC
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        us."SessionId",
        us."UserId",
        us."LoginTime",
        us."IpAddress",
        us."DeviceInfo",
        us."IsActive",
        EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - 
            COALESCE(us."LogoutTime", us."LoginTime")
        )) / 60 AS "LastActivityMinutesAgo"
    FROM
        public."UserSessions" us
    WHERE
        us."IsActive" = TRUE
        AND (
            us."LoginTime" >= (CURRENT_TIMESTAMP - (p_Minutes || ' minutes')::INTERVAL)
            OR 
            us."LogoutTime" >= (CURRENT_TIMESTAMP - (p_Minutes || ' minutes')::INTERVAL)
        )
    ORDER BY
        COALESCE(us."LogoutTime", us."LoginTime") DESC
    LIMIT p_Limit;
END;
$$;

-- Mark All User Sessions as Inactive stored procedure
CREATE OR REPLACE PROCEDURE public.usp_EndAllUserSessions(
    p_UserId INT,
    p_ExceptSessionId VARCHAR(100) = NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE public."UserSessions"
    SET
        "LogoutTime" = CURRENT_TIMESTAMP,
        "IsActive" = FALSE
    WHERE
        "UserId" = p_UserId
        AND "IsActive" = TRUE
        AND ("SessionId" != p_ExceptSessionId OR p_ExceptSessionId IS NULL);
END;
$$;
