CREATE TABLE sales_activity_meetings (
    id SERIAL PRIMARY KEY,
    user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    meeting_type VARCHAR(255),
    customer_name VARCHAR(255),
    customer_id VARCHAR(255),
    meeting_title VARCHAR(255),
    description TEXT,
    meeting_date_time TIMESTAMP,
    duration TIME,
    status VARCHAR(255),
    participant VARCHAR(255),
    file_url VARCHAR(255),
    stage_item_id VARCHAR(255),
    parent_meeting VARCHAR(255),
    stage VARCHAR(255),
    activity_check_lists_id INT,
    activity_parent_meetings_id INT,
    city VARCHAR(255),
    area VARCHAR(255),
    address TEXT,
    comments VARCHAR(255),
    delegate VARCHAR(255),
    assigned_to VARCHAR(255),
    FOREIGN KEY (user_created) REFERENCES sales_employees(id),
    FOREIGN KEY (user_updated) REFERENCES sales_employees(id),
    FOREIGN KEY (activity_check_lists_id) REFERENCES sales_activity_checklists(id),
    FOREIGN KEY (activity_parent_meetings_id) REFERENCES sales_activity_meetings(id)
);

CREATE TABLE "sales_activity_checklists"
("id" SERIAL PRIMARY KEY NOT NULL, 
"user_created"INTEGER REFERENCES sales_employees(id), 
"date_created" TIMESTAMP NULL,
"user_updated" INTEGER REFERENCES sales_employees(id), 
"date_updated" TIMESTAMP NULL, 
"description" TEXT, 
"done" boolean NULL,
"check_list_title" varchar(255) null);

-- Drop the existing function first
DROP FUNCTION IF EXISTS get_meetings_by_stage(VARCHAR, VARCHAR);

-- Recreate the function with updated return type
CREATE OR REPLACE FUNCTION get_meetings_by_stage(
    p_stage VARCHAR(255),
    p_stage_item_id VARCHAR(255)
)
RETURNS TABLE (
    id INTEGER,
    user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    meeting_type VARCHAR(255),
    customer_name VARCHAR(255),
    customer_id VARCHAR(255),
    meeting_title VARCHAR(255),
    description TEXT,
    meeting_date_time TIMESTAMP,
    duration TIME,
    status VARCHAR(255),
    participant VARCHAR(255),
    file_url VARCHAR(255),
    stage_item_id VARCHAR(255),
    parent_meeting VARCHAR(255),
    stage VARCHAR(255),
    activity_check_lists_id INT,
    activity_parent_meetings_id INT,
    city VARCHAR(255),
    area VARCHAR(255),
    address TEXT,
    comments VARCHAR(255),
    delegate VARCHAR(255),
    assigned_to VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT m.*
    FROM sales_activity_meetings m
    WHERE m.stage = p_stage
    AND CASE 
        WHEN p_stage = 'Lead' THEN m.stage_item_id = p_stage_item_id::text
        ELSE m.stage_item_id = p_stage_item_id
    END
    ORDER BY m.meeting_date_time DESC;
END;
$$ LANGUAGE plpgsql;

-- Trigger function to create external comments from activities
CREATE OR REPLACE FUNCTION fn_create_external_comment_from_activity()
RETURNS TRIGGER AS $$
DECLARE
    v_description text;
    v_date_time timestamp;
BEGIN
    -- Skip if comments is empty, 'string', or only whitespace
    IF NEW.comments IS NULL 
        OR NEW.comments = '' 
        OR NEW.comments = 'string' 
        OR LENGTH(TRIM(NEW.comments)) = 0 THEN
        RETURN NEW;
    END IF;

    -- Set date_time based on activity type
    IF TG_TABLE_NAME = 'sales_activity_calls' THEN
        v_date_time := COALESCE(NEW.call_datetime, NEW.date_created, CURRENT_TIMESTAMP);
    ELSIF TG_TABLE_NAME = 'sales_activity_meetings' THEN
        v_date_time := COALESCE(NEW.meeting_date_time, NEW.date_created, CURRENT_TIMESTAMP);
    ELSIF TG_TABLE_NAME = 'sales_activity_tasks' THEN
        v_date_time := COALESCE(NEW.due_date::timestamp, NEW.date_created, CURRENT_TIMESTAMP);
    ELSIF TG_TABLE_NAME = 'sales_activity_events' THEN
        v_date_time := COALESCE((NEW.start_date + NEW.start_time)::timestamp, NEW.date_created, CURRENT_TIMESTAMP);
    ELSE
        v_date_time := CURRENT_TIMESTAMP;
    END IF;

    -- Set description to be exactly the comment entered
    v_description := TRIM(NEW.comments);

    -- Create external comment
    INSERT INTO sales_external_comments (
        user_created,
        date_created,
        title,
        description,
        date_time,
        stage,
        stage_item_id,
        isactive,
        activity_id
    ) VALUES (
        NEW.user_created,
        COALESCE(NEW.date_created, CURRENT_TIMESTAMP),
        'comment added',
        v_description,
        v_date_time,
        NEW.stage,
        NEW.stage_item_id,
        true,
        NEW.id::text
    );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Drop existing triggers if they exist
DROP TRIGGER IF EXISTS tr_create_external_comment_meeting ON sales_activity_meetings;
DROP TRIGGER IF EXISTS tr_create_external_comment_call ON sales_activity_calls;
DROP TRIGGER IF EXISTS tr_create_external_comment_task ON sales_activity_tasks;
DROP TRIGGER IF EXISTS tr_create_external_comment_event ON sales_activity_events;

-- Create triggers for each activity type
CREATE TRIGGER tr_create_external_comment_meeting
    AFTER INSERT OR UPDATE ON sales_activity_meetings
    FOR EACH ROW
    WHEN (NEW.comments IS NOT NULL AND NEW.comments != '')
    EXECUTE FUNCTION fn_create_external_comment_from_activity();

CREATE TRIGGER tr_create_external_comment_call
    AFTER INSERT OR UPDATE ON sales_activity_calls
    FOR EACH ROW
    WHEN (NEW.comments IS NOT NULL AND NEW.comments != '')
    EXECUTE FUNCTION fn_create_external_comment_from_activity();

CREATE TRIGGER tr_create_external_comment_task
    AFTER INSERT OR UPDATE ON sales_activity_tasks
    FOR EACH ROW
    WHEN (NEW.comments IS NOT NULL AND NEW.comments != '')
    EXECUTE FUNCTION fn_create_external_comment_from_activity();

CREATE TRIGGER tr_create_external_comment_event
    AFTER INSERT OR UPDATE ON sales_activity_events
    FOR EACH ROW
    WHEN (NEW.comments IS NOT NULL AND NEW.comments != '')
    EXECUTE FUNCTION fn_create_external_comment_from_activity();

-- Drop unused columns
ALTER TABLE public.sales_activity_meetings
    DROP COLUMN IF EXISTS isactive,
    DROP COLUMN IF EXISTS group_with,
    DROP COLUMN IF EXISTS schedule_check_date,
    DROP COLUMN IF EXISTS customer_list_page_no,
    ALTER COLUMN participant TYPE varchar(255);