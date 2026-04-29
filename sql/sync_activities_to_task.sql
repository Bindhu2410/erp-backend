CREATE OR REPLACE FUNCTION public.fn_sync_activity_to_task()
RETURNS trigger AS $$
DECLARE
    v_task_name text;
    v_description text;
    v_due_date date;
    v_priority text;
    v_activity_id text;
    v_exists boolean;
    v_id int;
    v_user_created int;
    v_assigned_to int;
    v_status text;
    v_stage text;
    v_stage_item_id text;
BEGIN
    IF TG_OP = 'DELETE' THEN
        v_id := OLD.id;
    ELSE
        v_id := NEW.id;
        v_user_created := NEW.user_created;
        v_assigned_to := NEW.assignedtouserid;
        v_status := NEW.status;
        v_stage := NEW.stage;
        v_stage_item_id := NEW.stage_item_id;
    END IF;

    v_activity_id := (CASE 
        WHEN TG_TABLE_NAME = 'sales_activity_calls' THEN 'Call-' 
        WHEN TG_TABLE_NAME = 'sales_activity_meetings' THEN 'Meeting-' 
        WHEN TG_TABLE_NAME = 'sales_activity_events' THEN 'Event-' 
        WHEN TG_TABLE_NAME = 'sales_activity_tasks' THEN 'SalesTask-' 
        ELSE '' 
    END) || v_id;

    IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
        -- Prepare common fields
        IF TG_TABLE_NAME = 'sales_activity_calls' THEN
            v_task_name := COALESCE(NEW.call_title, 'Call with ' || COALESCE(NEW.call_with, 'Customer'));
            v_description := NEW.description;
            v_due_date := NEW.call_datetime::date;
            v_priority := NEW.priority;
        ELSIF TG_TABLE_NAME = 'sales_activity_meetings' THEN
            v_task_name := COALESCE(NEW.meeting_title, 'Meeting with ' || COALESCE(NEW.customer_name, 'Customer'));
            v_description := NEW.description;
            v_due_date := NEW.meeting_date_time::date;
            v_priority := 'Medium'; -- Default for meetings
        ELSIF TG_TABLE_NAME = 'sales_activity_events' THEN
            v_task_name := NEW.event_title;
            v_description := NEW.description;
            v_due_date := NEW.start_date;
            v_priority := NEW.priority;
        ELSIF TG_TABLE_NAME = 'sales_activity_tasks' THEN
            v_task_name := NEW.task_name;
            v_description := NEW.description;
            v_due_date := NEW.due_date;
            v_priority := NEW.priority;
        END IF;

        -- Check if task already exists
        SELECT EXISTS (SELECT 1 FROM public.task WHERE activity_id = v_activity_id) INTO v_exists;

        IF v_exists THEN
            -- UPDATE existing task
            UPDATE public.task SET
                task_name = v_task_name,
                description = v_description,
                due_date = v_due_date,
                priority = CASE WHEN v_priority IN ('Low', 'Medium', 'High') THEN v_priority ELSE 'Medium' END,
                activity_status = v_status,
                assignee_id = v_assigned_to,
                updated_at = CURRENT_TIMESTAMP,
                status = CASE WHEN v_status = 'Completed' THEN 'Completed' ELSE status END
            WHERE activity_id = v_activity_id;
        ELSE
            -- INSERT new task
            INSERT INTO public.task (
                task_name, description, task_type, status, priority, due_date, 
                stage, stage_item_id, owner_id, assignee_id, activity_status, activity_id
            ) VALUES (
                v_task_name, v_description, 'Main', 'Pending', 
                CASE WHEN v_priority IN ('Low', 'Medium', 'High') THEN v_priority ELSE 'Medium' END,
                v_due_date, v_stage, v_stage_item_id, v_user_created, v_assigned_to, 
                v_status, v_activity_id
            );
        END IF;
    ELSIF TG_OP = 'DELETE' THEN
        DELETE FROM public.task WHERE activity_id = v_activity_id;
    END IF;

    IF TG_OP = 'DELETE' THEN
        RETURN OLD;
    ELSE
        RETURN NEW;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Triggers
DROP TRIGGER IF EXISTS trg_sync_call_to_task ON public.sales_activity_calls;
CREATE TRIGGER trg_sync_call_to_task AFTER INSERT OR UPDATE OR DELETE ON public.sales_activity_calls FOR EACH ROW EXECUTE FUNCTION public.fn_sync_activity_to_task();

DROP TRIGGER IF EXISTS trg_sync_meeting_to_task ON public.sales_activity_meetings;
CREATE TRIGGER trg_sync_meeting_to_task AFTER INSERT OR UPDATE OR DELETE ON public.sales_activity_meetings FOR EACH ROW EXECUTE FUNCTION public.fn_sync_activity_to_task();

DROP TRIGGER IF EXISTS trg_sync_event_to_task ON public.sales_activity_events;
CREATE TRIGGER trg_sync_event_to_task AFTER INSERT OR UPDATE OR DELETE ON public.sales_activity_events FOR EACH ROW EXECUTE FUNCTION public.fn_sync_activity_to_task();

DROP TRIGGER IF EXISTS trg_sync_sales_task_to_task ON public.sales_activity_tasks;
CREATE TRIGGER trg_sync_sales_task_to_task AFTER INSERT OR UPDATE OR DELETE ON public.sales_activity_tasks FOR EACH ROW EXECUTE FUNCTION public.fn_sync_activity_to_task();
