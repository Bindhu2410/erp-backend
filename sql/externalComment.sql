CREATE TABLE "sales_external_comments"
("id" SERIAL PRIMARY KEY NOT NULL, 
"user_created" int, 
"date_created" TIMESTAMP NULL, 
"user_updated" int, 
"date_updated" TIMESTAMP NULL,
"title" varchar(255) NULL, 
"description" TEXT, 
"date_time" TIMESTAMP NULL,
"stage" VARCHAR(255) NULL,
"stage_item_id" varchar(255) null,
"isactive" BOOLEAN NOT NULL DEFAULT FALSE, 
"activity_id" varchar(255) null,
CONSTRAINT fk_external_comments_user_created foreign key ("user_created") references sales_employees(id),
CONSTRAINT fk_external_comments_user_updated FOREIGN KEY (user_updated) REFERENCES sales_employees(id)
);

-- Create stored procedure for managing activity external comments
CREATE OR REPLACE PROCEDURE sp_manage_activity_external_comments(
    p_user_id INT,
    p_title VARCHAR(255),
    p_description TEXT,
    p_stage VARCHAR(255),
    p_stage_item_id VARCHAR(255),
    p_activity_id VARCHAR(255),
    p_activity_type VARCHAR(50),
    p_comment_id INT DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_title VARCHAR(255);
    v_description TEXT;
BEGIN
    -- Set title based on activity type if not provided
    v_title := COALESCE(p_title, 'New ' || p_activity_type || ' Comment Added');
    
    -- Set description combining activity type and comment
    v_description := COALESCE(p_description, '');
    
    -- If comment_id is provided, update existing comment
    IF p_comment_id IS NOT NULL THEN
        UPDATE sales_external_comments
        SET 
            user_updated = p_user_id,
            date_updated = CURRENT_TIMESTAMP,
            title = v_title,
            description = v_description,
            date_time = CURRENT_TIMESTAMP,
            stage = COALESCE(p_stage, stage),
            stage_item_id = COALESCE(p_stage_item_id, stage_item_id),
            activity_id = COALESCE(p_activity_id, activity_id)
        WHERE id = p_comment_id;
    ELSE
        -- Insert new comment
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
            p_user_id,
            CURRENT_TIMESTAMP,
            v_title,
            v_description,
            CURRENT_TIMESTAMP,
            p_stage,
            p_stage_item_id,
            true,
            p_activity_id
        );
    END IF;
END;
$$;



