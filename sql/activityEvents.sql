CREATE TABLE sales_activity_events (
  id SERIAL PRIMARY KEY NOT NULL,
  user_created INTEGER REFERENCES sales_employees(id),
  date_created TIMESTAMP,
  user_updated INTEGER REFERENCES sales_employees(id),
  date_updated TIMESTAMP,
  event_title VARCHAR(255),
  guests TEXT,
  start_date DATE,
  end_date DATE,
  start_time TIME,
  end_time TIME,
  participant TEXT,
  event_location VARCHAR(255),
  description TEXT,
  status VARCHAR(255),
  priority VARCHAR(255),
  file_url VARCHAR(255),
  stage_item_id VARCHAR(255),
  stage VARCHAR(255),
  event_id VARCHAR(255),
  sales_activity_checklists_id INTEGER REFERENCES sales_activity_checklists(id),
  isactive BOOLEAN DEFAULT FALSE,
  comments TEXT,
  assigned_to VARCHAR(255)
);

-- Function to get events by stage and stage item ID
CREATE OR REPLACE FUNCTION get_events_by_stage(
    p_stage VARCHAR(255),
    p_stage_item_id VARCHAR(255)
)
RETURNS TABLE (
    id INTEGER,
    user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    event_title VARCHAR(255),
    guests TEXT,
    start_date DATE,
    end_date DATE,
    start_time TIME,
    end_time TIME,
    participant TEXT,
    event_location VARCHAR(255),
    description TEXT,
    status VARCHAR(255),
    priority VARCHAR(255),
    file_url VARCHAR(255),
    stage_item_id VARCHAR(255),
    stage VARCHAR(255),
    event_id VARCHAR(255),
    sales_activity_checklists_id INTEGER,
    isactive BOOLEAN,
    comments TEXT,
    assigned_to VARCHAR(255)
) AS $$
BEGIN
    RETURN QUERY
    SELECT e.*
    FROM sales_activity_events e
    WHERE e.stage = p_stage
    AND CASE 
        WHEN p_stage = 'Lead' THEN e.stage_item_id = p_stage_item_id::text
        ELSE e.stage_item_id = p_stage_item_id
    END
    ORDER BY e.start_date, e.start_time;
END;
$$ LANGUAGE plpgsql;
