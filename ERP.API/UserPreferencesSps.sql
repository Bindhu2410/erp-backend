-- =============================================
-- CRUD Stored Procedures for UserPreferences Table
-- Table: public."UserPreferences"
-- Primary Key: ("UserId", "PreferenceKey")
-- Created: July 3, 2025
-- =============================================

-- =============================================
-- 1. CREATE - Add/Update User Preference (UPSERT)
-- =============================================
CREATE OR REPLACE FUNCTION public.sp_create_um_user_preference(
    p_user_id integer,
    p_preference_key varchar(50),
    p_preference_value text
)
RETURNS TABLE(success boolean, message text)
LANGUAGE plpgsql
AS $function$
BEGIN
    -- Check if user exists
    IF NOT EXISTS (
        SELECT 1 FROM public.users 
        WHERE userid = p_user_id
    ) THEN
        RETURN QUERY 
        SELECT false, 'User not found'::text;
        RETURN;
    END IF;

    -- Validate preference key
    IF p_preference_key IS NULL OR LENGTH(TRIM(p_preference_key)) = 0 THEN
        RETURN QUERY 
        SELECT false, 'Preference key cannot be empty'::text;
        RETURN;
    END IF;

    -- Insert or update user preference
    BEGIN
        INSERT INTO public."UserPreferences" ("UserId", "PreferenceKey", "PreferenceValue", "DateModified")
        VALUES (p_user_id, p_preference_key, p_preference_value, CURRENT_TIMESTAMP)
        ON CONFLICT ("UserId", "PreferenceKey") 
        DO UPDATE SET 
            "PreferenceValue" = EXCLUDED."PreferenceValue",
            "DateModified" = CURRENT_TIMESTAMP;
        
        RETURN QUERY 
        SELECT true, 'User preference saved successfully'::text;
    EXCEPTION WHEN OTHERS THEN
        RETURN QUERY 
        SELECT false, ('Error saving user preference: ' || SQLERRM)::text;
    END;
END;
$function$;

-- =============================================
-- 2. READ - Get User Preference by Key
-- =============================================
CREATE OR REPLACE FUNCTION public.sp_get_um_user_preference(
    p_user_id integer,
    p_preference_key varchar(50)
)
RETURNS TABLE(
    user_id integer,
    preference_key varchar(50),
    preference_value text,
    date_modified timestamp
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT 
        up."UserId" as user_id,
        up."PreferenceKey" as preference_key,
        up."PreferenceValue" as preference_value,
        up."DateModified" as date_modified
    FROM public."UserPreferences" up
    WHERE up."UserId" = p_user_id 
      AND up."PreferenceKey" = p_preference_key;
END;
$function$;

-- =============================================
-- 3. READ - Get All User Preferences
-- =============================================
CREATE OR REPLACE FUNCTION public.sp_get_um_user_preferences(
    p_user_id integer
)
RETURNS TABLE(
    user_id integer,
    preference_key varchar(50),
    preference_value text,
    date_modified timestamp
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT 
        up."UserId" as user_id,
        up."PreferenceKey" as preference_key,
        up."PreferenceValue" as preference_value,
        up."DateModified" as date_modified
    FROM public."UserPreferences" up
    WHERE up."UserId" = p_user_id
    ORDER BY up."PreferenceKey";
END;
$function$;

-- =============================================
-- 4. READ - Get All User Preferences (Paged)
-- =============================================
CREATE OR REPLACE FUNCTION public.sp_get_um_all_user_preferences(
    p_page_number integer DEFAULT 1,
    p_page_size integer DEFAULT 50,
    p_user_id integer DEFAULT NULL
)
RETURNS TABLE(
    user_id integer,
    username varchar(50),
    preference_key varchar(50),
    preference_value text,
    date_modified timestamp,
    total_count bigint
)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_offset integer;
BEGIN
    v_offset := (p_page_number - 1) * p_page_size;
    
    RETURN QUERY
    SELECT 
        up."UserId" as user_id,
        u.username,
        up."PreferenceKey" as preference_key,
        up."PreferenceValue" as preference_value,
        up."DateModified" as date_modified,
        COUNT(*) OVER() as total_count
    FROM public."UserPreferences" up
    INNER JOIN public.users u ON up."UserId" = u.userid
    WHERE (p_user_id IS NULL OR up."UserId" = p_user_id)
    ORDER BY u.username, up."PreferenceKey"
    LIMIT p_page_size OFFSET v_offset;
END;
$function$;

-- =============================================
-- 5. READ - Check if User Preference exists
-- =============================================
CREATE OR REPLACE FUNCTION public.sp_check_um_user_preference_exists(
    p_user_id integer,
    p_preference_key varchar(50)
)
RETURNS boolean
LANGUAGE plpgsql
AS $function$
DECLARE
    v_exists boolean := false;
BEGIN
    SELECT EXISTS (
        SELECT 1 FROM public."UserPreferences" 
        WHERE "UserId" = p_user_id 
          AND "PreferenceKey" = p_preference_key
    ) INTO v_exists;
    
    RETURN v_exists;
END;
$function$;

-- =============================================
-- 6. UPDATE - Update User Preference
-- =============================================
CREATE OR REPLACE FUNCTION public.sp_update_um_user_preference(
    p_user_id integer,
    p_preference_key varchar(50),
    p_preference_value text
)
RETURNS TABLE(success boolean, message text)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_updated_count integer;
BEGIN
    -- Check if user preference exists
    IF NOT EXISTS (
        SELECT 1 FROM public."UserPreferences" 
        WHERE "UserId" = p_user_id 
          AND "PreferenceKey" = p_preference_key
    ) THEN
        RETURN QUERY 
        SELECT false, 'User preference not found'::text;
        RETURN;
    END IF;

    BEGIN
        -- Update the user preference
        UPDATE public."UserPreferences" 
        SET "PreferenceValue" = p_preference_value,
            "DateModified" = CURRENT_TIMESTAMP
        WHERE "UserId" = p_user_id 
          AND "PreferenceKey" = p_preference_key;
        
        GET DIAGNOSTICS v_updated_count = ROW_COUNT;
        
        IF v_updated_count > 0 THEN
            RETURN QUERY 
            SELECT true, 'User preference updated successfully'::text;
        ELSE
            RETURN QUERY 
            SELECT false, 'No records were updated'::text;
        END IF;
        
    EXCEPTION WHEN OTHERS THEN
        RETURN QUERY 
        SELECT false, ('Error updating user preference: ' || SQLERRM)::text;
    END;
END;
$function$;

-- =============================================
-- 7. UPDATE - Bulk Update User Preferences
-- =============================================
CREATE OR REPLACE FUNCTION public.sp_update_um_user_preferences_bulk(
    p_user_id integer,
    p_preferences jsonb
)
RETURNS TABLE(success boolean, message text, updated_count integer)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_pref_record jsonb;
    v_updated_count integer := 0;
    v_key varchar(50);
    v_value text;
BEGIN
    -- Check if user exists
    IF NOT EXISTS (
        SELECT 1 FROM public.users 
        WHERE userid = p_user_id
    ) THEN
        RETURN QUERY 
        SELECT false, 'User not found'::text, 0;
        RETURN;
    END IF;

    BEGIN
        -- Loop through preferences and update/insert
        FOR v_pref_record IN SELECT * FROM jsonb_array_elements(p_preferences)
        LOOP
            v_key := v_pref_record->>'key';
            v_value := v_pref_record->>'value';
            
            -- Skip if key is null or empty
            IF v_key IS NULL OR LENGTH(TRIM(v_key)) = 0 THEN
                CONTINUE;
            END IF;
            
            -- Insert or update preference
            INSERT INTO public."UserPreferences" ("UserId", "PreferenceKey", "PreferenceValue", "DateModified")
            VALUES (p_user_id, v_key, v_value, CURRENT_TIMESTAMP)
            ON CONFLICT ("UserId", "PreferenceKey") 
            DO UPDATE SET 
                "PreferenceValue" = EXCLUDED."PreferenceValue",
                "DateModified" = CURRENT_TIMESTAMP;
            
            v_updated_count := v_updated_count + 1;
        END LOOP;

        RETURN QUERY 
        SELECT true, ('Successfully updated ' || v_updated_count || ' user preferences')::text, v_updated_count;
        
    EXCEPTION WHEN OTHERS THEN
        RETURN QUERY 
        SELECT false, ('Error updating user preferences: ' || SQLERRM)::text, 0;
    END;
END;
$function$;

-- =============================================
-- 8. DELETE - Remove User Preference
-- =============================================
CREATE OR REPLACE FUNCTION public.sp_delete_um_user_preference(
    p_user_id integer,
    p_preference_key varchar(50)
)
RETURNS TABLE(success boolean, message text)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_deleted_count integer;
BEGIN
    -- Check if user preference exists
    IF NOT EXISTS (
        SELECT 1 FROM public."UserPreferences" 
        WHERE "UserId" = p_user_id 
          AND "PreferenceKey" = p_preference_key
    ) THEN
        RETURN QUERY 
        SELECT false, 'User preference not found'::text;
        RETURN;
    END IF;

    BEGIN
        -- Delete the user preference
        DELETE FROM public."UserPreferences" 
        WHERE "UserId" = p_user_id 
          AND "PreferenceKey" = p_preference_key;
        
        GET DIAGNOSTICS v_deleted_count = ROW_COUNT;
        
        IF v_deleted_count > 0 THEN
            RETURN QUERY 
            SELECT true, 'User preference deleted successfully'::text;
        ELSE
            RETURN QUERY 
            SELECT false, 'No records were deleted'::text;
        END IF;
        
    EXCEPTION WHEN OTHERS THEN
        RETURN QUERY 
        SELECT false, ('Error deleting user preference: ' || SQLERRM)::text;
    END;
END;
$function$;

-- =============================================
-- 9. DELETE - Remove All User Preferences
-- =============================================
CREATE OR REPLACE FUNCTION public.sp_delete_um_all_user_preferences(
    p_user_id integer
)
RETURNS TABLE(success boolean, message text, deleted_count integer)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_deleted_count integer;
BEGIN
    -- Check if user exists
    IF NOT EXISTS (
        SELECT 1 FROM public.users 
        WHERE userid = p_user_id
    ) THEN
        RETURN QUERY 
        SELECT false, 'User not found'::text, 0;
        RETURN;
    END IF;

    BEGIN
        -- Delete all preferences for this user
        DELETE FROM public."UserPreferences" 
        WHERE "UserId" = p_user_id;
        
        GET DIAGNOSTICS v_deleted_count = ROW_COUNT;
        
        RETURN QUERY 
        SELECT true, ('Successfully deleted ' || v_deleted_count || ' user preferences')::text, v_deleted_count;
        
    EXCEPTION WHEN OTHERS THEN
        RETURN QUERY 
        SELECT false, ('Error deleting user preferences: ' || SQLERRM)::text, 0;
    END;
END;
$function$;

-- =============================================
-- 10. UTILITY - Get User Preferences by Key Pattern
-- =============================================
CREATE OR REPLACE FUNCTION public.sp_get_um_user_preferences_by_pattern(
    p_user_id integer,
    p_key_pattern varchar(50)
)
RETURNS TABLE(
    user_id integer,
    preference_key varchar(50),
    preference_value text,
    date_modified timestamp
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT 
        up."UserId" as user_id,
        up."PreferenceKey" as preference_key,
        up."PreferenceValue" as preference_value,
        up."DateModified" as date_modified
    FROM public."UserPreferences" up
    WHERE up."UserId" = p_user_id 
      AND up."PreferenceKey" LIKE p_key_pattern
    ORDER BY up."PreferenceKey";
END;
$function$;

-- =============================================
-- 11. UTILITY - Get User Preferences Summary
-- =============================================
CREATE OR REPLACE FUNCTION public.sp_get_um_user_preferences_summary(
    p_user_id integer
)
RETURNS TABLE(
    user_id integer,
    username varchar(50),
    total_preferences bigint,
    last_modified timestamp
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT 
        u.userid as user_id,
        u.username,
        COUNT(up."PreferenceKey") as total_preferences,
        MAX(up."DateModified") as last_modified
    FROM public.users u
    LEFT JOIN public."UserPreferences" up ON u.userid = up."UserId"
    WHERE u.userid = p_user_id
    GROUP BY u.userid, u.username;
END;
$function$;

-- =============================================
-- 12. UTILITY - Get Users with Specific Preference
-- =============================================
CREATE OR REPLACE FUNCTION public.sp_get_um_users_with_preference(
    p_preference_key varchar(50),
    p_preference_value text DEFAULT NULL
)
RETURNS TABLE(
    user_id integer,
    username varchar(50),
    preference_value text,
    date_modified timestamp
)
LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT 
        u.userid as user_id,
        u.username,
        up."PreferenceValue" as preference_value,
        up."DateModified" as date_modified
    FROM public.users u
    INNER JOIN public."UserPreferences" up ON u.userid = up."UserId"
    WHERE up."PreferenceKey" = p_preference_key
      AND (p_preference_value IS NULL OR up."PreferenceValue" = p_preference_value)
    ORDER BY u.username;
END;
$function$;

-- =============================================
-- Usage Examples:
-- =============================================

/*
-- 1. Create/Update user preference (upsert)
SELECT * FROM sp_create_um_user_preference(1, 'theme', 'dark');

-- 2. Get specific user preference
SELECT * FROM sp_get_um_user_preference(1, 'theme');

-- 3. Get all preferences for a user
SELECT * FROM sp_get_um_user_preferences(1);

-- 4. Get all user preferences (paged)
SELECT * FROM sp_get_um_all_user_preferences(1, 20, NULL);

-- 5. Check if user preference exists
SELECT sp_check_um_user_preference_exists(1, 'theme');

-- 6. Update user preference
SELECT * FROM sp_update_um_user_preference(1, 'theme', 'light');

-- 7. Bulk update user preferences
SELECT * FROM sp_update_um_user_preferences_bulk(1, '[{"key": "theme", "value": "dark"}, {"key": "language", "value": "en"}]'::jsonb);

-- 8. Delete user preference
SELECT * FROM sp_delete_um_user_preference(1, 'theme');

-- 9. Delete all user preferences
SELECT * FROM sp_delete_um_all_user_preferences(1);

-- 10. Get user preferences by pattern
SELECT * FROM sp_get_um_user_preferences_by_pattern(1, 'ui_%');

-- 11. Get user preferences summary
SELECT * FROM sp_get_um_user_preferences_summary(1);

-- 12. Get users with specific preference
SELECT * FROM sp_get_um_users_with_preference('theme', 'dark');
*/
