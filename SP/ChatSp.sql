-- ============================================
-- Chat Application Stored Procedures
-- PostgreSQL Functions
-- NOTE: users table columns are lowercase without underscores:
--   userid, username, firstname, lastname, email, profileimageurl, isactive, etc.
-- ============================================

-- 1. Create a private (1-to-1) chat
CREATE OR REPLACE FUNCTION create_private_chat(
    p_user1_id INTEGER,
    p_user2_id INTEGER
) RETURNS TABLE(chat_id INTEGER) AS $$
DECLARE
    v_chat_id INTEGER;
    v_existing_chat_id INTEGER;
BEGIN
    -- Check if private chat already exists between these two users
    SELECT cm1.chat_id INTO v_existing_chat_id
    FROM chat_members cm1
    INNER JOIN chat_members cm2 ON cm1.chat_id = cm2.chat_id
    INNER JOIN chats c ON c.id = cm1.chat_id
    WHERE cm1.user_id = p_user1_id 
      AND cm2.user_id = p_user2_id
      AND c.chat_type = 'private'
      AND c.is_active = TRUE
      AND cm1.is_active = TRUE
      AND cm2.is_active = TRUE
    LIMIT 1;
    
    IF v_existing_chat_id IS NOT NULL THEN
        RETURN QUERY SELECT v_existing_chat_id;
        RETURN;
    END IF;
    
    -- Create new chat
    INSERT INTO chats (chat_type, created_by, date_created)
    VALUES ('private', p_user1_id, CURRENT_TIMESTAMP)
    RETURNING id INTO v_chat_id;
    
    -- Add both members
    INSERT INTO chat_members (chat_id, user_id, role, joined_at, is_active)
    VALUES 
        (v_chat_id, p_user1_id, 'member', CURRENT_TIMESTAMP, TRUE),
        (v_chat_id, p_user2_id, 'member', CURRENT_TIMESTAMP, TRUE);
    
    RETURN QUERY SELECT v_chat_id;
END;
$$ LANGUAGE plpgsql;

-- 2. Create a group chat
CREATE OR REPLACE FUNCTION create_group_chat(
    p_name VARCHAR(255),
    p_created_by INTEGER,
    p_member_ids INTEGER[],
    p_image_url TEXT DEFAULT NULL
) RETURNS TABLE(chat_id INTEGER) AS $$
DECLARE
    v_chat_id INTEGER;
    v_member_id INTEGER;
BEGIN
    -- Create the group chat
    INSERT INTO chats (chat_name, chat_type, chat_image_url, created_by, date_created)
    VALUES (p_name, 'group', p_image_url, p_created_by, CURRENT_TIMESTAMP)
    RETURNING id INTO v_chat_id;
    
    -- Add creator as admin
    INSERT INTO chat_members (chat_id, user_id, role, joined_at, is_active)
    VALUES (v_chat_id, p_created_by, 'admin', CURRENT_TIMESTAMP, TRUE);
    
    -- Add other members
    FOREACH v_member_id IN ARRAY p_member_ids
    LOOP
        IF v_member_id != p_created_by THEN
            INSERT INTO chat_members (chat_id, user_id, role, joined_at, is_active)
            VALUES (v_chat_id, v_member_id, 'member', CURRENT_TIMESTAMP, TRUE)
            ON CONFLICT (chat_id, user_id) DO NOTHING;
        END IF;
    END LOOP;
    
    RETURN QUERY SELECT v_chat_id;
END;
$$ LANGUAGE plpgsql;

-- 3. Send a message
CREATE OR REPLACE FUNCTION send_chat_message(
    p_chat_id INTEGER,
    p_sender_id INTEGER,
    p_message_text TEXT,
    p_message_type VARCHAR(20) DEFAULT 'text',
    p_file_url TEXT DEFAULT NULL,
    p_file_name VARCHAR(500) DEFAULT NULL,
    p_file_size BIGINT DEFAULT NULL,
    p_reply_to_id INTEGER DEFAULT NULL
) RETURNS TABLE(
    message_id INTEGER,
    chat_id INTEGER,
    sender_id INTEGER,
    message_text TEXT,
    message_type VARCHAR,
    file_url TEXT,
    file_name VARCHAR,
    file_size BIGINT,
    reply_to_id INTEGER,
    date_created TIMESTAMP
) AS $$
DECLARE
    v_message_id INTEGER;
BEGIN
    -- Insert the message
    INSERT INTO chat_messages_v2 (chat_id, sender_id, message_text, message_type, file_url, file_name, file_size, reply_to_id, date_created)
    VALUES (p_chat_id, p_sender_id, p_message_text, p_message_type, p_file_url, p_file_name, p_file_size, p_reply_to_id, CURRENT_TIMESTAMP)
    RETURNING id INTO v_message_id;
    
    -- Create message_status for all chat members except sender
    INSERT INTO message_status (message_id, user_id, status, status_at)
    SELECT v_message_id, cm.user_id, 'sent', CURRENT_TIMESTAMP
    FROM chat_members cm
    WHERE cm.chat_id = p_chat_id 
      AND cm.user_id != p_sender_id
      AND cm.is_active = TRUE;
    
    -- Update the chat's date_updated
    UPDATE chats SET date_updated = CURRENT_TIMESTAMP WHERE id = p_chat_id;
    
    RETURN QUERY
    SELECT m.id, m.chat_id, m.sender_id, m.message_text, m.message_type,
           m.file_url, m.file_name, m.file_size, m.reply_to_id, m.date_created
    FROM chat_messages_v2 m
    WHERE m.id = v_message_id;
END;
$$ LANGUAGE plpgsql;

-- 4. Get chat history with pagination
CREATE OR REPLACE FUNCTION get_chat_history(
    p_chat_id INTEGER,
    p_user_id INTEGER,
    p_page INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 50
) RETURNS TABLE(
    message_id INTEGER,
    chat_id INTEGER,
    sender_id INTEGER,
    sender_name VARCHAR,
    sender_avatar TEXT,
    message_text TEXT,
    message_type VARCHAR,
    file_url TEXT,
    file_name VARCHAR,
    file_size BIGINT,
    reply_to_id INTEGER,
    reply_text TEXT,
    reply_sender_name VARCHAR,
    is_edited BOOLEAN,
    is_deleted BOOLEAN,
    date_created TIMESTAMP,
    read_by_count BIGINT,
    total_recipients BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        m.id AS message_id,
        m.chat_id,
        m.sender_id,
        (u.firstname || ' ' || u.lastname)::VARCHAR AS sender_name,
        u.profileimageurl AS sender_avatar,
        CASE WHEN m.is_deleted THEN '[Message deleted]'::TEXT ELSE m.message_text END,
        m.message_type,
        m.file_url,
        m.file_name,
        m.file_size,
        m.reply_to_id,
        rm.message_text AS reply_text,
        (ru.firstname || ' ' || ru.lastname)::VARCHAR AS reply_sender_name,
        m.is_edited,
        m.is_deleted,
        m.date_created,
        (SELECT COUNT(*) FROM message_status ms WHERE ms.message_id = m.id AND ms.status = 'read') AS read_by_count,
        (SELECT COUNT(*) FROM message_status ms WHERE ms.message_id = m.id) AS total_recipients
    FROM chat_messages_v2 m
    INNER JOIN users u ON u.userid = m.sender_id
    LEFT JOIN chat_messages_v2 rm ON rm.id = m.reply_to_id
    LEFT JOIN users ru ON ru.userid = rm.sender_id
    WHERE m.chat_id = p_chat_id
    ORDER BY m.date_created DESC
    LIMIT p_page_size
    OFFSET (p_page - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

-- 5. Mark messages as read
CREATE OR REPLACE FUNCTION mark_messages_as_read(
    p_chat_id INTEGER,
    p_user_id INTEGER
) RETURNS INTEGER AS $$
DECLARE
    v_count INTEGER;
BEGIN
    UPDATE message_status ms
    SET status = 'read', status_at = CURRENT_TIMESTAMP
    FROM chat_messages_v2 m
    WHERE ms.message_id = m.id
      AND m.chat_id = p_chat_id
      AND ms.user_id = p_user_id
      AND ms.status != 'read';
    
    GET DIAGNOSTICS v_count = ROW_COUNT;
    RETURN v_count;
END;
$$ LANGUAGE plpgsql;

-- 6. Get user's chat list
CREATE OR REPLACE FUNCTION get_user_chats(
    p_user_id INTEGER
) RETURNS TABLE(
    chat_id INTEGER,
    chat_name VARCHAR,
    chat_type VARCHAR,
    chat_image_url TEXT,
    last_message TEXT,
    last_message_type VARCHAR,
    last_message_sender VARCHAR,
    last_message_time TIMESTAMP,
    unread_count BIGINT,
    member_count BIGINT,
    is_muted BOOLEAN,
    other_user_id INTEGER,
    other_user_name VARCHAR,
    other_user_avatar TEXT,
    other_user_online BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        c.id AS chat_id,
        c.chat_name,
        c.chat_type,
        c.chat_image_url,
        lm.message_text AS last_message,
        lm.message_type AS last_message_type,
        (lu.firstname || ' ' || lu.lastname)::VARCHAR AS last_message_sender,
        lm.date_created AS last_message_time,
        (SELECT COUNT(*) 
         FROM message_status ms2 
         INNER JOIN chat_messages_v2 m2 ON m2.id = ms2.message_id
         WHERE m2.chat_id = c.id AND ms2.user_id = p_user_id AND ms2.status != 'read'
        ) AS unread_count,
        (SELECT COUNT(*) FROM chat_members cm3 WHERE cm3.chat_id = c.id AND cm3.is_active = TRUE) AS member_count,
        cm.is_muted,
        -- For private chats, get the other user's info
        ou.userid AS other_user_id,
        (ou.firstname || ' ' || ou.lastname)::VARCHAR AS other_user_name,
        ou.profileimageurl AS other_user_avatar,
        COALESCE(up.is_online, FALSE) AS other_user_online
    FROM chats c
    INNER JOIN chat_members cm ON cm.chat_id = c.id AND cm.user_id = p_user_id AND cm.is_active = TRUE
    -- Get last message
    LEFT JOIN LATERAL (
        SELECT m.message_text, m.message_type, m.sender_id, m.date_created
        FROM chat_messages_v2 m
        WHERE m.chat_id = c.id
        ORDER BY m.date_created DESC
        LIMIT 1
    ) lm ON TRUE
    LEFT JOIN users lu ON lu.userid = lm.sender_id
    -- For private chats, get other user
    LEFT JOIN chat_members cm2 ON cm2.chat_id = c.id AND cm2.user_id != p_user_id AND cm2.is_active = TRUE AND c.chat_type = 'private'
    LEFT JOIN users ou ON ou.userid = cm2.user_id
    LEFT JOIN user_presence up ON up.user_id = ou.userid
    WHERE c.is_active = TRUE
    ORDER BY COALESCE(lm.date_created, c.date_created) DESC;
END;
$$ LANGUAGE plpgsql;

-- 7. Search messages
CREATE OR REPLACE FUNCTION search_chat_messages(
    p_user_id INTEGER,
    p_search_term VARCHAR(500),
    p_chat_id INTEGER DEFAULT NULL,
    p_page INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 20
) RETURNS TABLE(
    message_id INTEGER,
    chat_id INTEGER,
    chat_name VARCHAR,
    sender_name VARCHAR,
    message_text TEXT,
    message_type VARCHAR,
    date_created TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        m.id AS message_id,
        m.chat_id,
        COALESCE(c.chat_name, (ou.firstname || ' ' || ou.lastname))::VARCHAR AS chat_name,
        (u.firstname || ' ' || u.lastname)::VARCHAR AS sender_name,
        m.message_text,
        m.message_type,
        m.date_created
    FROM chat_messages_v2 m
    INNER JOIN chats c ON c.id = m.chat_id
    INNER JOIN chat_members cm ON cm.chat_id = c.id AND cm.user_id = p_user_id AND cm.is_active = TRUE
    INNER JOIN users u ON u.userid = m.sender_id
    LEFT JOIN chat_members cm2 ON cm2.chat_id = c.id AND cm2.user_id != p_user_id AND cm2.is_active = TRUE AND c.chat_type = 'private'
    LEFT JOIN users ou ON ou.userid = cm2.user_id
    WHERE m.message_text ILIKE '%' || p_search_term || '%'
      AND m.is_deleted = FALSE
      AND (p_chat_id IS NULL OR m.chat_id = p_chat_id)
    ORDER BY m.date_created DESC
    LIMIT p_page_size
    OFFSET (p_page - 1) * p_page_size;
END;
$$ LANGUAGE plpgsql;

-- 8. Add member to group
CREATE OR REPLACE FUNCTION add_group_member(
    p_chat_id INTEGER,
    p_user_id INTEGER,
    p_added_by INTEGER
) RETURNS BOOLEAN AS $$
DECLARE
    v_chat_type VARCHAR(20);
BEGIN
    SELECT chat_type INTO v_chat_type FROM chats WHERE id = p_chat_id AND is_active = TRUE;
    
    IF v_chat_type != 'group' THEN
        RETURN FALSE;
    END IF;
    
    INSERT INTO chat_members (chat_id, user_id, role, joined_at, is_active)
    VALUES (p_chat_id, p_user_id, 'member', CURRENT_TIMESTAMP, TRUE)
    ON CONFLICT (chat_id, user_id) 
    DO UPDATE SET is_active = TRUE, left_at = NULL, joined_at = CURRENT_TIMESTAMP;
    
    -- Insert system message
    INSERT INTO chat_messages_v2 (chat_id, sender_id, message_text, message_type, date_created)
    VALUES (p_chat_id, p_added_by, 
            (SELECT firstname || ' ' || lastname FROM users WHERE userid = p_user_id) || ' was added to the group',
            'system', CURRENT_TIMESTAMP);
    
    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

-- 9. Remove member from group
CREATE OR REPLACE FUNCTION remove_group_member(
    p_chat_id INTEGER,
    p_user_id INTEGER,
    p_removed_by INTEGER
) RETURNS BOOLEAN AS $$
BEGIN
    UPDATE chat_members 
    SET is_active = FALSE, left_at = CURRENT_TIMESTAMP
    WHERE chat_id = p_chat_id AND user_id = p_user_id;
    
    -- Insert system message
    INSERT INTO chat_messages_v2 (chat_id, sender_id, message_text, message_type, date_created)
    VALUES (p_chat_id, p_removed_by,
            (SELECT firstname || ' ' || lastname FROM users WHERE userid = p_user_id) || ' was removed from the group',
            'system', CURRENT_TIMESTAMP);
    
    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

-- 10. Update user presence
CREATE OR REPLACE FUNCTION update_user_presence(
    p_user_id INTEGER,
    p_is_online BOOLEAN,
    p_connection_id VARCHAR(255) DEFAULT NULL
) RETURNS VOID AS $$
BEGIN
    INSERT INTO user_presence (user_id, is_online, last_seen, connection_id)
    VALUES (p_user_id, p_is_online, CURRENT_TIMESTAMP, p_connection_id)
    ON CONFLICT (user_id)
    DO UPDATE SET 
        is_online = p_is_online,
        last_seen = CURRENT_TIMESTAMP,
        connection_id = CASE WHEN p_is_online THEN p_connection_id ELSE NULL END;
END;
$$ LANGUAGE plpgsql;

-- 11. Get chat members
CREATE OR REPLACE FUNCTION get_chat_members(
    p_chat_id INTEGER
) RETURNS TABLE(
    user_id INTEGER,
    user_name VARCHAR,
    avatar TEXT,
    role VARCHAR,
    is_online BOOLEAN,
    last_seen TIMESTAMP,
    joined_at TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u.userid,
        (u.firstname || ' ' || u.lastname)::VARCHAR AS user_name,
        u.profileimageurl AS avatar,
        cm.role,
        COALESCE(up.is_online, FALSE) AS is_online,
        up.last_seen,
        cm.joined_at
    FROM chat_members cm
    INNER JOIN users u ON u.userid = cm.user_id
    LEFT JOIN user_presence up ON up.user_id = u.userid
    WHERE cm.chat_id = p_chat_id AND cm.is_active = TRUE
    ORDER BY cm.role DESC, u.firstname;
END;
$$ LANGUAGE plpgsql;
