CREATE TABLE chat_messages (
    id VARCHAR(36) PRIMARY KEY,
    "user" VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    group_name VARCHAR(255)
);

CREATE INDEX idx_chat_messages_timestamp ON chat_messages(timestamp);
CREATE INDEX idx_chat_messages_group ON chat_messages(group_name);