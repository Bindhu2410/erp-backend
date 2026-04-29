-- ============================================
-- Chat Application Database Schema
-- PostgreSQL - Normalized Tables
-- NOTE: users table columns are lowercase without underscores:
--   userid, username, firstname, lastname, email, profileimageurl, isactive, etc.
-- ============================================

-- 1. Chats table (1-to-1 and group chats)
CREATE TABLE IF NOT EXISTS chats (
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
CREATE TABLE IF NOT EXISTS chat_members (
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
CREATE TABLE IF NOT EXISTS chat_messages_v2 (
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
CREATE TABLE IF NOT EXISTS message_status (
    id SERIAL PRIMARY KEY,
    message_id INTEGER NOT NULL REFERENCES chat_messages_v2(id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES users(userid) ON DELETE CASCADE,
    status VARCHAR(20) NOT NULL DEFAULT 'sent' CHECK (status IN ('sent', 'delivered', 'read')),
    status_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(message_id, user_id)
);

-- 5. User Presence table
CREATE TABLE IF NOT EXISTS user_presence (
    user_id INTEGER PRIMARY KEY REFERENCES users(userid) ON DELETE CASCADE,
    is_online BOOLEAN DEFAULT FALSE,
    last_seen TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    connection_id VARCHAR(255)
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_chat_members_chat_id ON chat_members(chat_id);
CREATE INDEX IF NOT EXISTS idx_chat_members_user_id ON chat_members(user_id);
CREATE INDEX IF NOT EXISTS idx_chat_messages_v2_chat_id ON chat_messages_v2(chat_id);
CREATE INDEX IF NOT EXISTS idx_chat_messages_v2_sender_id ON chat_messages_v2(sender_id);
CREATE INDEX IF NOT EXISTS idx_chat_messages_v2_date_created ON chat_messages_v2(date_created DESC);
CREATE INDEX IF NOT EXISTS idx_message_status_message_id ON message_status(message_id);
CREATE INDEX IF NOT EXISTS idx_message_status_user_id ON message_status(user_id);
CREATE INDEX IF NOT EXISTS idx_user_presence_is_online ON user_presence(is_online);
