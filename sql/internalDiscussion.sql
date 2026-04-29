CREATE TABLE internal_discussion (
    id SERIAL PRIMARY KEY,
    user_created INTEGER NOT NULL,
    date_created TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    comment TEXT NOT NULL CHECK (comment <> ''),
    parent INTEGER DEFAULT NULL,
    stage TEXT NOT NULL,
    stage_item_id TEXT NOT NULL,
    seen_by TEXT,
    user_name varchar(255),

    FOREIGN KEY (user_created) REFERENCES sales_employees(id),
    FOREIGN KEY (user_updated) REFERENCES sales_employees(id),
    FOREIGN KEY (parent) REFERENCES internal_discussion(id) ON DELETE CASCADE
);
 
-- Add indexes for common query patterns
CREATE INDEX idx_internal_discussion_stage_item ON internal_discussion(stage, stage_item_id);
CREATE INDEX idx_internal_discussion_parent ON internal_discussion(parent);
CREATE INDEX idx_internal_discussion_user_created ON internal_discussion(user_created);

-- Add a trigger to automatically update date_updated
CREATE OR REPLACE FUNCTION update_internal_discussion_date_updated()
RETURNS TRIGGER AS $$
BEGIN
    NEW.date_updated = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_internal_discussion_update_timestamp
    BEFORE UPDATE ON internal_discussion
    FOR EACH ROW
    EXECUTE FUNCTION update_internal_discussion_date_updated();