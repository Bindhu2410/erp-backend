-- ============================================
-- Chat Application - Complete Setup Script
-- Run this ONCE to set up all chat tables
-- Uses correct users table columns: userid, firstname, lastname, profileimageurl, isactive
-- ============================================

-- Drop old functions if they exist (from previous attempts)
DROP FUNCTION IF EXISTS create_private_chat(INTEGER, INTEGER);
DROP FUNCTION IF EXISTS create_group_chat(VARCHAR, INTEGER, INTEGER[], TEXT);
DROP FUNCTION IF EXISTS send_chat_message(INTEGER, INTEGER, TEXT, VARCHAR, TEXT, VARCHAR, BIGINT, INTEGER);
DROP FUNCTION IF EXISTS get_chat_history(INTEGER, INTEGER, INTEGER, INTEGER);
DROP FUNCTION IF EXISTS get_user_chats(INTEGER);
DROP FUNCTION IF EXISTS mark_messages_as_read(INTEGER, INTEGER);
DROP FUNCTION IF EXISTS search_chat_messages(INTEGER, VARCHAR, INTEGER, INTEGER, INTEGER);
DROP FUNCTION IF EXISTS add_group_member(INTEGER, INTEGER, INTEGER);
DROP FUNCTION IF EXISTS remove_group_member(INTEGER, INTEGER, INTEGER);
DROP FUNCTION IF EXISTS update_user_presence(INTEGER, BOOLEAN, VARCHAR);
DROP FUNCTION IF EXISTS get_chat_members(INTEGER);

-- Drop old tables if they exist (order matters for FK dependencies)
DROP TABLE IF EXISTS message_status CASCADE;
DROP TABLE IF EXISTS chat_messages_v2 CASCADE;
DROP TABLE IF EXISTS chat_members CASCADE;
DROP TABLE IF EXISTS user_presence CASCADE;
DROP TABLE IF EXISTS chats CASCADE;

-- 1. Chats table
CREATE TABLE chats (
    id SERIAL PRIMARY KEY,
    chat_name VARCHAR(255),
    chat_type VARCHAR(20) NOT NULL DEFAULT 'private' CHECK (chat_type IN ('private', 'group')),
    chat_image_url TEXT,
    created_by INTEGER REFERENCES users(userid),
    date_created TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    date_updated TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE
);

-- 2. Chat Members table
CREATE TABLE chat_members (
    id SERIAL PRIMARY KEY,
    chat_id INTEGER NOT NULL REFERENCES chats(id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES users(userid) ON DELETE CASCADE,
    role VARCHAR(20) NOT NULL DEFAULT 'member' CHECK (role IN ('admin', 'member')),
    joined_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    left_at TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    is_muted BOOLEAN DEFAULT FALSE,
    UNIQUE(chat_id, user_id)
);

-- 3. Messages table
CREATE TABLE chat_messages_v2 (
    id SERIAL PRIMARY KEY,
    chat_id INTEGER NOT NULL REFERENCES chats(id) ON DELETE CASCADE,
    sender_id INTEGER NOT NULL REFERENCES users(userid),
    message_text TEXT,
    message_type VARCHAR(20) NOT NULL DEFAULT 'text' CHECK (message_type IN ('text', 'image', 'file', 'audio', 'video', 'system')),
    file_url TEXT,
    file_name VARCHAR(500),
    file_size BIGINT,
    reply_to_id INTEGER REFERENCES chat_messages_v2(id),
    is_edited BOOLEAN DEFAULT FALSE,
    is_deleted BOOLEAN DEFAULT FALSE,
    date_created TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    date_updated TIMESTAMP
);

-- 4. Message Status table (read receipts)
CREATE TABLE message_status (
    id SERIAL PRIMARY KEY,
    message_id INTEGER NOT NULL REFERENCES chat_messages_v2(id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES users(userid) ON DELETE CASCADE,
    status VARCHAR(20) NOT NULL DEFAULT 'sent' CHECK (status IN ('sent', 'delivered', 'read')),
    status_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(message_id, user_id)
);

-- 5. User Presence table
CREATE TABLE user_presence (
    user_id INTEGER PRIMARY KEY REFERENCES users(userid) ON DELETE CASCADE,
    is_online BOOLEAN DEFAULT FALSE,
    last_seen TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    connection_id VARCHAR(255)
);

-- Indexes
CREATE INDEX idx_chat_members_chat_id ON chat_members(chat_id);
CREATE INDEX idx_chat_members_user_id ON chat_members(user_id);
CREATE INDEX idx_chat_messages_v2_chat_id ON chat_messages_v2(chat_id);
CREATE INDEX idx_chat_messages_v2_sender_id ON chat_messages_v2(sender_id);
CREATE INDEX idx_chat_messages_v2_date_created ON chat_messages_v2(date_created DESC);
CREATE INDEX idx_message_status_message_id ON message_status(message_id);
CREATE INDEX idx_message_status_user_id ON message_status(user_id);
CREATE INDEX idx_user_presence_is_online ON user_presence(is_online);

-- Verify
SELECT 'Chat tables created successfully!' AS status;
SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name IN ('chats', 'chat_members', 'chat_messages_v2', 'message_status', 'user_presence') ORDER BY table_name;
