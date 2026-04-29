-- =============================================
-- Users Table CRUD Stored Procedures
-- Database: PostgreSQL
-- Table: public.users
-- Created: July 2, 2025
-- =============================================

-- =============================================
-- CREATE USER (with auto-generated password hash/salt)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_create_user(
    p_username VARCHAR(50),
    p_email VARCHAR(100),
    p_firstname VARCHAR(50),
    p_lastname VARCHAR(50),
    p_password VARCHAR(255), -- Plain text password - will be hashed by Password Service
    p_phonenumber VARCHAR(20) DEFAULT NULL,
    p_profileimageurl VARCHAR(255) DEFAULT NULL,
    p_preferredlanguage VARCHAR(10) DEFAULT 'en-US',
    p_timezone VARCHAR(50) DEFAULT 'UTC',
    p_twofactorenabled BOOLEAN DEFAULT FALSE,
    p_twofactorkey VARCHAR(100) DEFAULT NULL,
    p_notes TEXT DEFAULT NULL
)
RETURNS TABLE(
    userid INT,
    success BOOLEAN,
    message TEXT,
    generated_hash VARCHAR(255),
    generated_salt VARCHAR(50)
) AS $$
DECLARE
    v_userid INT;
    v_password_hash VARCHAR(255);
    v_password_salt VARCHAR(50);
BEGIN
    -- Check if username already exists
    IF EXISTS (SELECT 1 FROM public.users WHERE username = p_username) THEN
        RETURN QUERY SELECT 0, FALSE, 'Username already exists'::TEXT, ''::VARCHAR(255), ''::VARCHAR(50);
        RETURN;
    END IF;
    
    -- Check if email already exists
    IF EXISTS (SELECT 1 FROM public.users WHERE email = p_email) THEN
        RETURN QUERY SELECT 0, FALSE, 'Email already exists'::TEXT, ''::VARCHAR(255), ''::VARCHAR(50);
        RETURN;
    END IF;
    
    -- Generate password hash and salt (placeholder - implement in your Password Service)
    -- This would typically be handled by your application's Password Service
    -- For now, we'll use a simple approach - replace with your actual implementation
    v_password_salt := encode(gen_random_bytes(32), 'base64');
    v_password_hash := encode(digest(p_password || v_password_salt, 'sha256'), 'hex');
    
    -- Insert new user
    INSERT INTO public.users (
        username, email, firstname, lastname, passwordhash, passwordsalt,
        phonenumber, profileimageurl, preferredlanguage, timezone,
        twofactorenabled, twofactorkey, notes, datecreated, lastpasswordchangedate
    ) VALUES (
        p_username, p_email, p_firstname, p_lastname, v_password_hash, v_password_salt,
        p_phonenumber, p_profileimageurl, p_preferredlanguage, p_timezone,
        p_twofactorenabled, p_twofactorkey, p_notes, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
    ) RETURNING users.userid INTO v_userid;
    
    RETURN QUERY SELECT v_userid, TRUE, 'User created successfully'::TEXT, v_password_hash, v_password_salt;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT 0, FALSE, SQLERRM::TEXT, ''::VARCHAR(255), ''::VARCHAR(50);
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- READ USER BY ID
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_user_by_id(p_userid INT)
RETURNS TABLE(
    userid INT,
    username VARCHAR(50),
    email VARCHAR(100),
    firstname VARCHAR(50),
    lastname VARCHAR(50),
    phonenumber VARCHAR(20),
    profileimageurl VARCHAR(255),
    datecreated TIMESTAMP,
    lastlogindate TIMESTAMP,
    isactive BOOLEAN,
    islocked BOOLEAN,
    failedloginattempts INT,
    preferredlanguage VARCHAR(10),
    timezone VARCHAR(50),
    twofactorenabled BOOLEAN,
    lastpasswordchangedate TIMESTAMP,
    requirepasswordchange BOOLEAN,
    notes TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u.userid, u.username, u.email, u.firstname, u.lastname,
        u.phonenumber, u.profileimageurl, u.datecreated, u.lastlogindate,
        u.isactive, u.islocked, u.failedloginattempts, u.preferredlanguage,
        u.timezone, u.twofactorenabled, u.lastpasswordchangedate,
        u.requirepasswordchange, u.notes
    FROM public.users u
    WHERE u.userid = p_userid;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- READ USER BY USERNAME
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_user_by_username(p_username VARCHAR(50))
RETURNS TABLE(
    userid INT,
    username VARCHAR(50),
    email VARCHAR(100),
    firstname VARCHAR(50),
    lastname VARCHAR(50),
    passwordhash VARCHAR(255),
    passwordsalt VARCHAR(50),
    phonenumber VARCHAR(20),
    profileimageurl VARCHAR(255),
    datecreated TIMESTAMP,
    lastlogindate TIMESTAMP,
    isactive BOOLEAN,
    islocked BOOLEAN,
    failedloginattempts INT,
    resetpasswordtoken VARCHAR(100),
    resetpasswordexpiry TIMESTAMP,
    preferredlanguage VARCHAR(10),
    timezone VARCHAR(50),
    twofactorenabled BOOLEAN,
    twofactorkey VARCHAR(100),
    lastpasswordchangedate TIMESTAMP,
    requirepasswordchange BOOLEAN,
    notes TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u.userid, u.username, u.email, u.firstname, u.lastname,
        u.passwordhash, u.passwordsalt, u.phonenumber, u.profileimageurl,
        u.datecreated, u.lastlogindate, u.isactive, u.islocked,
        u.failedloginattempts, u.resetpasswordtoken, u.resetpasswordexpiry,
        u.preferredlanguage, u.timezone, u.twofactorenabled, u.twofactorkey,
        u.lastpasswordchangedate, u.requirepasswordchange, u.notes
    FROM public.users u
    WHERE u.username = p_username;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- READ USER BY EMAIL
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_user_by_email(p_email VARCHAR(100))
RETURNS TABLE(
    userid INT,
    username VARCHAR(50),
    email VARCHAR(100),
    firstname VARCHAR(50),
    lastname VARCHAR(50),
    passwordhash VARCHAR(255),
    passwordsalt VARCHAR(50),
    phonenumber VARCHAR(20),
    profileimageurl VARCHAR(255),
    datecreated TIMESTAMP,
    lastlogindate TIMESTAMP,
    isactive BOOLEAN,
    islocked BOOLEAN,
    failedloginattempts INT,
    resetpasswordtoken VARCHAR(100),
    resetpasswordexpiry TIMESTAMP,
    preferredlanguage VARCHAR(10),
    timezone VARCHAR(50),
    twofactorenabled BOOLEAN,
    twofactorkey VARCHAR(100),
    lastpasswordchangedate TIMESTAMP,
    requirepasswordchange BOOLEAN,
    notes TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u.userid, u.username, u.email, u.firstname, u.lastname,
        u.passwordhash, u.passwordsalt, u.phonenumber, u.profileimageurl,
        u.datecreated, u.lastlogindate, u.isactive, u.islocked,
        u.failedloginattempts, u.resetpasswordtoken, u.resetpasswordexpiry,
        u.preferredlanguage, u.timezone, u.twofactorenabled, u.twofactorkey,
        u.lastpasswordchangedate, u.requirepasswordchange, u.notes
    FROM public.users u
    WHERE u.email = p_email;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- READ ALL USERS (with pagination)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_all_users(
    p_page_number INT DEFAULT 1,
    p_page_size INT DEFAULT 10,
    p_search_term VARCHAR(100) DEFAULT NULL,
    p_is_active BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    userid INT,
    username VARCHAR(50),
    email VARCHAR(100),
    firstname VARCHAR(50),
    lastname VARCHAR(50),
    phonenumber VARCHAR(20),
    profileimageurl VARCHAR(255),
    datecreated TIMESTAMP,
    lastlogindate TIMESTAMP,
    isactive BOOLEAN,
    islocked BOOLEAN,
    failedloginattempts INT,
    preferredlanguage VARCHAR(10),
    timezone VARCHAR(50),
    twofactorenabled BOOLEAN,
    lastpasswordchangedate TIMESTAMP,
    requirepasswordchange BOOLEAN,
    total_count BIGINT
) AS $$
DECLARE
    v_offset INT;
BEGIN
    v_offset := (p_page_number - 1) * p_page_size;
    
    RETURN QUERY
    WITH user_data AS (
        SELECT 
            u.userid, u.username, u.email, u.firstname, u.lastname,
            u.phonenumber, u.profileimageurl, u.datecreated, u.lastlogindate,
            u.isactive, u.islocked, u.failedloginattempts, u.preferredlanguage,
            u.timezone, u.twofactorenabled, u.lastpasswordchangedate,
            u.requirepasswordchange,
            COUNT(*) OVER() as total_count
        FROM public.users u
        WHERE 
            (p_is_active IS NULL OR u.isactive = p_is_active)
            AND (p_search_term IS NULL OR 
                 u.username ILIKE '%' || p_search_term || '%' OR
                 u.email ILIKE '%' || p_search_term || '%' OR
                 u.firstname ILIKE '%' || p_search_term || '%' OR
                 u.lastname ILIKE '%' || p_search_term || '%')
        ORDER BY u.datecreated DESC
        LIMIT p_page_size OFFSET v_offset
    )
    SELECT * FROM user_data;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- UPDATE USER
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_update_user(
    p_userid INT,
    p_username VARCHAR(50) DEFAULT NULL,
    p_email VARCHAR(100) DEFAULT NULL,
    p_firstname VARCHAR(50) DEFAULT NULL,
    p_lastname VARCHAR(50) DEFAULT NULL,
    p_phonenumber VARCHAR(20) DEFAULT NULL,
    p_profileimageurl VARCHAR(255) DEFAULT NULL,
    p_preferredlanguage VARCHAR(10) DEFAULT NULL,
    p_timezone VARCHAR(50) DEFAULT NULL,
    p_twofactorenabled BOOLEAN DEFAULT NULL,
    p_twofactorkey VARCHAR(100) DEFAULT NULL,
    p_notes TEXT DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    updated_userid INT
) AS $$
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM public.users WHERE userid = p_userid) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Check username uniqueness if provided
    IF p_username IS NOT NULL AND EXISTS (
        SELECT 1 FROM public.users WHERE username = p_username AND userid != p_userid
    ) THEN
        RETURN QUERY SELECT FALSE, 'Username already exists'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Check email uniqueness if provided
    IF p_email IS NOT NULL AND EXISTS (
        SELECT 1 FROM public.users WHERE email = p_email AND userid != p_userid
    ) THEN
        RETURN QUERY SELECT FALSE, 'Email already exists'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Update user
    UPDATE public.users SET
        username = COALESCE(p_username, username),
        email = COALESCE(p_email, email),
        firstname = COALESCE(p_firstname, firstname),
        lastname = COALESCE(p_lastname, lastname),
        phonenumber = COALESCE(p_phonenumber, phonenumber),
        profileimageurl = COALESCE(p_profileimageurl, profileimageurl),
        preferredlanguage = COALESCE(p_preferredlanguage, preferredlanguage),
        timezone = COALESCE(p_timezone, timezone),
        twofactorenabled = COALESCE(p_twofactorenabled, twofactorenabled),
        twofactorkey = COALESCE(p_twofactorkey, twofactorkey),
        notes = COALESCE(p_notes, notes)
    WHERE userid = p_userid;
    
    RETURN QUERY SELECT TRUE, 'User updated successfully'::TEXT, p_userid;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- UPDATE USER PASSWORD (with plain text password)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_update_user_password(
    p_userid INT,
    p_new_password VARCHAR(255), -- Plain text password - will be hashed by Password Service
    p_require_password_change BOOLEAN DEFAULT FALSE
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    generated_hash VARCHAR(255),
    generated_salt VARCHAR(50)
) AS $$
DECLARE
    v_password_hash VARCHAR(255);
    v_password_salt VARCHAR(50);
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM public.users WHERE userid = p_userid) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT, ''::VARCHAR(255), ''::VARCHAR(50);
        RETURN;
    END IF;
    
    -- Generate new password hash and salt (placeholder - implement in your Password Service)
    v_password_salt := encode(gen_random_bytes(32), 'base64');
    v_password_hash := encode(digest(p_new_password || v_password_salt, 'sha256'), 'hex');
    
    -- Update password
    UPDATE public.users SET
        passwordhash = v_password_hash,
        passwordsalt = v_password_salt,
        lastpasswordchangedate = CURRENT_TIMESTAMP,
        requirepasswordchange = p_require_password_change,
        resetpasswordtoken = NULL,
        resetpasswordexpiry = NULL,
        failedloginattempts = 0
    WHERE userid = p_userid;
    
    RETURN QUERY SELECT TRUE, 'Password updated successfully'::TEXT, v_password_hash, v_password_salt;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, ''::VARCHAR(255), ''::VARCHAR(50);
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- UPDATE USER PASSWORD (Alternative - with pre-hashed password from service)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_update_user_password_with_hash(
    p_userid INT,
    p_passwordhash VARCHAR(255), -- Already hashed by Password Service
    p_passwordsalt VARCHAR(50),  -- Already generated by Password Service
    p_require_password_change BOOLEAN DEFAULT FALSE
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM public.users WHERE userid = p_userid) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT;
        RETURN;
    END IF;
    
    -- Update password with pre-hashed values
    UPDATE public.users SET
        passwordhash = p_passwordhash,
        passwordsalt = p_passwordsalt,
        lastpasswordchangedate = CURRENT_TIMESTAMP,
        requirepasswordchange = p_require_password_change,
        resetpasswordtoken = NULL,
        resetpasswordexpiry = NULL,
        failedloginattempts = 0
    WHERE userid = p_userid;
    
    RETURN QUERY SELECT TRUE, 'Password updated successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- UPDATE USER STATUS (ACTIVATE/DEACTIVATE)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_update_user_status(
    p_userid INT,
    p_isactive BOOLEAN,
    p_islocked BOOLEAN DEFAULT NULL
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM public.users WHERE userid = p_userid) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT;
        RETURN;
    END IF;
    
    -- Update user status
    UPDATE public.users SET
        isactive = p_isactive,
        islocked = COALESCE(p_islocked, islocked)
    WHERE userid = p_userid;
    
    RETURN QUERY SELECT TRUE, 'User status updated successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- UPDATE LOGIN INFORMATION
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_update_user_login(
    p_userid INT,
    p_success BOOLEAN DEFAULT TRUE,
    p_reset_failed_attempts BOOLEAN DEFAULT FALSE
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM public.users WHERE userid = p_userid) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT;
        RETURN;
    END IF;
    
    IF p_success THEN
        -- Successful login
        UPDATE public.users SET
            lastlogindate = CURRENT_TIMESTAMP,
            failedloginattempts = 0
        WHERE userid = p_userid;
    ELSE
        -- Failed login
        UPDATE public.users SET
            failedloginattempts = failedloginattempts + 1,
            islocked = CASE 
                WHEN failedloginattempts + 1 >= 5 THEN TRUE 
                ELSE islocked 
            END
        WHERE userid = p_userid;
    END IF;
    
    IF p_reset_failed_attempts THEN
        UPDATE public.users SET failedloginattempts = 0 WHERE userid = p_userid;
    END IF;
    
    RETURN QUERY SELECT TRUE, 'Login information updated successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Extended version of sp_um_update_user_login that captures additional login information
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_update_user_login_extended(
    p_userid INT,
    p_success BOOLEAN DEFAULT TRUE,
    p_ip_address VARCHAR(45) DEFAULT NULL,
    p_device_info VARCHAR(255) DEFAULT NULL,
    p_user_agent VARCHAR(255) DEFAULT NULL,
    p_location VARCHAR(100) DEFAULT NULL,
    p_session_id VARCHAR(255) DEFAULT NULL,
    p_reset_failed_attempts BOOLEAN DEFAULT FALSE
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
DECLARE
    v_loginid INT;
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM public.users WHERE userid = p_userid) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT;
        RETURN;
    END IF;
    
    IF p_success THEN
        -- Successful login
        UPDATE public.users SET
            lastlogindate = CURRENT_TIMESTAMP,
            failedloginattempts = 0
        WHERE userid = p_userid;
    ELSE
        -- Failed login
        UPDATE public.users SET
            failedloginattempts = failedloginattempts + 1,
            islocked = CASE 
                WHEN failedloginattempts + 1 >= 5 THEN TRUE 
                ELSE islocked 
            END
        WHERE userid = p_userid;
    END IF;
    
    IF p_reset_failed_attempts THEN
        UPDATE public.users SET failedloginattempts = 0 WHERE userid = p_userid;
    END IF;
    
    -- Insert login attempt record
    INSERT INTO public.user_login_history(
        userid, 
        login_time, 
        success, 
        ip_address, 
        device_info, 
        user_agent, 
        location, 
        session_id
    )
    VALUES(
        p_userid, 
        CURRENT_TIMESTAMP, 
        p_success, 
        p_ip_address, 
        p_device_info, 
        p_user_agent, 
        p_location, 
        p_session_id
    )
    RETURNING loginid INTO v_loginid;
    
    RETURN QUERY SELECT TRUE, 'User login info updated'::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- SET PASSWORD RESET TOKEN
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_set_password_reset_token(
    p_email VARCHAR(100),
    p_reset_token VARCHAR(100),
    p_expiry_hours INT DEFAULT 24
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    userid INT
) AS $$
DECLARE
    v_userid INT;
BEGIN
    -- Check if user exists
    SELECT u.userid INTO v_userid FROM public.users u WHERE u.email = p_email;
    
    IF v_userid IS NULL THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT, 0;
        RETURN;
    END IF;
    
    -- Set reset token
    UPDATE public.users SET
        resetpasswordtoken = p_reset_token,
        resetpasswordexpiry = CURRENT_TIMESTAMP + (p_expiry_hours || ' hours')::INTERVAL
    WHERE userid = v_userid;
    
    RETURN QUERY SELECT TRUE, 'Reset token set successfully'::TEXT, v_userid;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- VERIFY PASSWORD RESET TOKEN
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_verify_password_reset_token(
    p_reset_token VARCHAR(100)
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    userid INT,
    email VARCHAR(100)
) AS $$
DECLARE
    v_userid INT;
    v_email VARCHAR(100);
    v_expiry TIMESTAMP;
BEGIN
    -- Get user by reset token
    SELECT u.userid, u.email, u.resetpasswordexpiry 
    INTO v_userid, v_email, v_expiry
    FROM public.users u 
    WHERE u.resetpasswordtoken = p_reset_token;
    
    IF v_userid IS NULL THEN
        RETURN QUERY SELECT FALSE, 'Invalid reset token'::TEXT, 0, ''::VARCHAR(100);
        RETURN;
    END IF;
    
    IF v_expiry < CURRENT_TIMESTAMP THEN
        RETURN QUERY SELECT FALSE, 'Reset token has expired'::TEXT, 0, ''::VARCHAR(100);
        RETURN;
    END IF;
    
    RETURN QUERY SELECT TRUE, 'Token is valid'::TEXT, v_userid, v_email;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0, ''::VARCHAR(100);
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- SOFT DELETE USER (DEACTIVATE)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_soft_delete_user(p_userid INT)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM public.users WHERE userid = p_userid) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT;
        RETURN;
    END IF;
    
    -- Soft delete (deactivate) user
    UPDATE public.users SET
        isactive = FALSE,
        islocked = TRUE
    WHERE userid = p_userid;
    
    RETURN QUERY SELECT TRUE, 'User deactivated successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- HARD DELETE USER (PERMANENT)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_hard_delete_user(p_userid INT)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
BEGIN
    -- Check if user exists
    IF NOT EXISTS (SELECT 1 FROM public.users WHERE userid = p_userid) THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT;
        RETURN;
    END IF;
    
    -- Hard delete user
    DELETE FROM public.users WHERE userid = p_userid;
    
    RETURN QUERY SELECT TRUE, 'User deleted permanently'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- GET USER STATISTICS
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_get_user_statistics()
RETURNS TABLE(
    total_users BIGINT,
    active_users BIGINT,
    inactive_users BIGINT,
    locked_users BIGINT,
    users_with_2fa BIGINT,
    users_requiring_password_change BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*)::BIGINT as total_users,
        COUNT(CASE WHEN isactive = TRUE THEN 1 END)::BIGINT as active_users,
        COUNT(CASE WHEN isactive = FALSE THEN 1 END)::BIGINT as inactive_users,
        COUNT(CASE WHEN islocked = TRUE THEN 1 END)::BIGINT as locked_users,
        COUNT(CASE WHEN twofactorenabled = TRUE THEN 1 END)::BIGINT as users_with_2fa,
        COUNT(CASE WHEN requirepasswordchange = TRUE THEN 1 END)::BIGINT as users_requiring_password_change
    FROM public.users;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- Example Usage Comments
-- =============================================

/*
-- Create a new user (with auto-generated hash/salt)
SELECT * FROM sp_um_create_user(
    'john_doe', 
    'john@example.com', 
    'John', 
    'Doe', 
    'MyPlainTextPassword123!', -- Plain text password
    '+1234567890',
    'https://example.com/profile.jpg'
);

-- Create a new user (with pre-hashed password from Password Service)
SELECT * FROM sp_um_create_user_with_hash(
    'jane_doe', 
    'jane@example.com', 
    'Jane', 
    'Doe', 
    'pre_hashed_password_here', -- Already hashed by Password Service
    'pre_generated_salt_here',  -- Already generated by Password Service
    '+1234567891',
    'https://example.com/jane_profile.jpg'
);

-- Verify user password (for authentication)
SELECT * FROM sp_um_verify_user_password('john_doe', 'MyPlainTextPassword123!');

-- Get user by ID
SELECT * FROM sp_um_get_user_by_id(1);

-- Get user by username
SELECT * FROM sp_um_get_user_by_username('john_doe');

-- Get all users with pagination
SELECT * FROM sp_um_get_all_users(1, 10, 'john', TRUE);

-- Update user
SELECT * FROM sp_um_update_user(
    1, 
    'john_doe_updated', 
    'john.updated@example.com', 
    'John Updated', 
    'Doe Updated'
);

-- Update password (with auto-generated hash/salt)
SELECT * FROM sp_um_update_user_password(1, 'NewPlainTextPassword456!');

-- Update password (with pre-hashed password from Password Service)
SELECT * FROM sp_um_update_user_password_with_hash(1, 'new_hashed_password', 'new_salt');

-- Update user status
SELECT * FROM sp_um_update_user_status(1, TRUE, FALSE);

-- Update login info (successful login)
SELECT * FROM sp_um_update_user_login(1, TRUE);

-- Set password reset token
SELECT * FROM sp_um_set_password_reset_token('john@example.com', 'reset_token_123', 24);

-- Verify reset token
SELECT * FROM sp_um_verify_password_reset_token('reset_token_123');

-- Soft delete user
SELECT * FROM sp_um_soft_delete_user(1);

-- Get user statistics
SELECT * FROM sp_um_get_user_statistics();

-- =============================================
-- IMPORTANT NOTES FOR PASSWORD SERVICE INTEGRATION:
-- =============================================
-- 
-- 1. The sp_um_create_user() and sp_um_update_user_password() functions include 
--    basic password hashing using PostgreSQL's built-in functions as examples.
--    
-- 2. For production use, replace the password hashing logic with your 
--    Password Service implementation by:
--    - Calling your Password Service from your application layer
--    - Using sp_um_create_user_with_hash() and sp_um_update_user_password_with_hash()
--    - These functions accept pre-hashed passwords and salts
--
-- 3. The sp_um_verify_user_password() function is provided for testing purposes.
--    In production, password verification should be handled by your Password Service.
--
-- 4. Consider implementing these password operations in your application layer:
--    - Password complexity validation
--    - Secure random salt generation
--    - Strong hashing algorithms (bcrypt, scrypt, Argon2)
--    - Password history tracking
--    - Rate limiting for authentication attempts
*/

-- =============================================
-- CREATE USER (Alternative - with pre-hashed password from service)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_create_user_with_hash(
    p_username VARCHAR(50),
    p_email VARCHAR(100),
    p_firstname VARCHAR(50),
    p_lastname VARCHAR(50),
    p_passwordhash VARCHAR(255), -- Already hashed by Password Service
    p_passwordsalt VARCHAR(50),  -- Already generated by Password Service
    p_phonenumber VARCHAR(20) DEFAULT NULL,
    p_profileimageurl VARCHAR(255) DEFAULT NULL,
    p_preferredlanguage VARCHAR(10) DEFAULT 'en-US',
    p_timezone VARCHAR(50) DEFAULT 'UTC',
    p_twofactorenabled BOOLEAN DEFAULT FALSE,
    p_twofactorkey VARCHAR(100) DEFAULT NULL,
    p_notes TEXT DEFAULT NULL
)
RETURNS TABLE(
    userid INT,
    success BOOLEAN,
    message TEXT
) AS $$
DECLARE
    v_userid INT;
BEGIN
    -- Check if username already exists
    IF EXISTS (SELECT 1 FROM public.users WHERE username = p_username) THEN
        RETURN QUERY SELECT 0, FALSE, 'Username already exists'::TEXT;
        RETURN;
    END IF;
    
    -- Check if email already exists
    IF EXISTS (SELECT 1 FROM public.users WHERE email = p_email) THEN
        RETURN QUERY SELECT 0, FALSE, 'Email already exists'::TEXT;
        RETURN;
    END IF;
    
    -- Insert new user with pre-hashed password
    INSERT INTO public.users (
        username, email, firstname, lastname, passwordhash, passwordsalt,
        phonenumber, profileimageurl, preferredlanguage, timezone,
        twofactorenabled, twofactorkey, notes, datecreated, lastpasswordchangedate
    ) VALUES (
        p_username, p_email, p_firstname, p_lastname, p_passwordhash, p_passwordsalt,
        p_phonenumber, p_profileimageurl, p_preferredlanguage, p_timezone,
        p_twofactorenabled, p_twofactorkey, p_notes, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
    ) RETURNING users.userid INTO v_userid;
    
    RETURN QUERY SELECT v_userid, TRUE, 'User created successfully'::TEXT;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT 0, FALSE, SQLERRM::TEXT;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- VERIFY USER PASSWORD (for authentication)
-- =============================================
CREATE OR REPLACE FUNCTION sp_um_verify_user_password(
    p_username VARCHAR(50),
    p_plain_password VARCHAR(255)
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    userid INT,
    user_data JSONB
) AS $$
DECLARE
    v_user_record RECORD;
    v_computed_hash VARCHAR(255);
BEGIN
    -- Get user record
    SELECT u.userid, u.username, u.email, u.firstname, u.lastname, 
           u.passwordhash, u.passwordsalt, u.isactive, u.islocked,
           u.failedloginattempts, u.twofactorenabled
    INTO v_user_record
    FROM public.users u 
    WHERE u.username = p_username;
    
    -- Check if user exists
    IF v_user_record.userid IS NULL THEN
        RETURN QUERY SELECT FALSE, 'User not found'::TEXT, 0, '{}'::JSONB;
        RETURN;
    END IF;
    
    -- Check if account is locked
    IF v_user_record.islocked THEN
        RETURN QUERY SELECT FALSE, 'Account is locked'::TEXT, 0, '{}'::JSONB;
        RETURN;
    END IF;
    
    -- Check if account is active
    IF NOT v_user_record.isactive THEN
        RETURN QUERY SELECT FALSE, 'Account is inactive'::TEXT, 0, '{}'::JSONB;
        RETURN;
    END IF;
    
    -- Compute hash with stored salt
    v_computed_hash := encode(digest(p_plain_password || v_user_record.passwordsalt, 'sha256'), 'hex');
    
    -- Verify password
    IF v_computed_hash = v_user_record.passwordhash THEN
        -- Password is correct
        RETURN QUERY SELECT 
            TRUE, 
            'Authentication successful'::TEXT, 
            v_user_record.userid,
            jsonb_build_object(
                'userid', v_user_record.userid,
                'username', v_user_record.username,
                'email', v_user_record.email,
                'firstname', v_user_record.firstname,
                'lastname', v_user_record.lastname,
                'twofactorenabled', v_user_record.twofactorenabled
            );
    ELSE
        -- Password is incorrect
        RETURN QUERY SELECT FALSE, 'Invalid password'::TEXT, v_user_record.userid, '{}'::JSONB;
    END IF;
    
EXCEPTION
    WHEN OTHERS THEN
        RETURN QUERY SELECT FALSE, SQLERRM::TEXT, 0, '{}'::JSONB;
END;
$$ LANGUAGE plpgsql;

-- Create user_login_history table if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'user_login_history') THEN
        CREATE TABLE public.user_login_history (
            loginid SERIAL PRIMARY KEY,
            userid INTEGER NOT NULL REFERENCES public.users(userid) ON DELETE CASCADE,
            login_time TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            success BOOLEAN NOT NULL DEFAULT TRUE,
            ip_address VARCHAR(45),
            device_info VARCHAR(255),
            user_agent VARCHAR(255),
            location VARCHAR(100),
            session_id VARCHAR(255)
        );

        COMMENT ON TABLE public.user_login_history IS 'Tracks all login attempts with detailed information';
        COMMENT ON COLUMN public.user_login_history.loginid IS 'Unique login attempt identifier';
        COMMENT ON COLUMN public.user_login_history.userid IS 'Reference to the user who attempted login';
        COMMENT ON COLUMN public.user_login_history.login_time IS 'Timestamp of the login attempt';
        COMMENT ON COLUMN public.user_login_history.success IS 'Whether the login attempt was successful';
        COMMENT ON COLUMN public.user_login_history.ip_address IS 'IP address of the login attempt';
        COMMENT ON COLUMN public.user_login_history.device_info IS 'Device information for the login attempt';
        COMMENT ON COLUMN public.user_login_history.user_agent IS 'User agent string for the login attempt';
        COMMENT ON COLUMN public.user_login_history.location IS 'Geographic location information for the login attempt';
        COMMENT ON COLUMN public.user_login_history.session_id IS 'Session ID if a successful login';

        CREATE INDEX idx_user_login_history_userid ON public.user_login_history(userid);
        CREATE INDEX idx_user_login_history_time ON public.user_login_history(login_time);
        CREATE INDEX idx_user_login_history_success ON public.user_login_history(success);
    END IF;
END $$;
