-- Stored procedure to update user password hash and salt
CREATE OR REPLACE FUNCTION sp_um_update_user_password(
    p_userid INTEGER,
    p_passwordhash VARCHAR(255),
    p_passwordsalt VARCHAR(50),
    p_require_password_change BOOLEAN DEFAULT false
)
RETURNS TABLE(success BOOLEAN, message TEXT)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE public.users
    SET
        passwordhash = p_passwordhash,
        passwordsalt = p_passwordsalt,
        lastpasswordchangedate = CURRENT_TIMESTAMP,
        requirepasswordchange = p_require_password_change
    WHERE userid = p_userid;

    IF FOUND THEN
        RETURN QUERY SELECT true, 'Password updated successfully'::TEXT;
    ELSE
        RETURN QUERY SELECT false, 'User not found'::TEXT;
    END IF;
END;
$$;
-- Fix passwordsalt column width (Base64 of 32 bytes = 44 chars, varchar(50) is too tight)
ALTER TABLE public.users ALTER COLUMN passwordsalt TYPE varchar(100);