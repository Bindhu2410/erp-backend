-- Trigger: Close related task when lead is converted to opportunity
DROP TRIGGER IF EXISTS trg_close_task_on_lead_conversion ON public.sales_lead;

CREATE TRIGGER trg_close_task_on_lead_conversion
AFTER UPDATE OF status ON public.sales_lead
FOR EACH ROW
WHEN ((NEW.status = 'Converted' OR NEW.status = 'Disqualified') AND OLD.status IS DISTINCT FROM NEW.status)
EXECUTE FUNCTION public.close_task_on_lead_conversion();
----------------------------------------------------------------------
-- TRIGGER FUNCTION: Close related task when lead is converted to opportunity
CREATE OR REPLACE FUNCTION public.close_task_on_lead_conversion()
RETURNS TRIGGER AS $$
BEGIN
        -- When a lead is converted to opportunity, close the related task
        UPDATE public.task
             SET status = 'Closed', updated_at = CURRENT_TIMESTAMP
         WHERE stage = 'Lead'
             AND stage_item_id = NEW.id::text
             AND status != 'Closed';
        RETURN NEW;
END;
$$ LANGUAGE plpgsql;
----------------------------------------------------------------------
drop table task cascade;
-- Corrected public.tasks table schema
CREATE TABLE public.task (
    id SERIAL PRIMARY KEY,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    user_created INT REFERENCES public.users(userid) ON DELETE SET NULL,
    user_updated INT REFERENCES public.users(userid) ON DELETE SET NULL,
    task_id VARCHAR(255) UNIQUE, -- External/ERP reference if needed
    task_name VARCHAR(255) NOT NULL,
    parent_task_id INT REFERENCES public.task(id) ON DELETE CASCADE,
    description varchar(255),
    task_type VARCHAR(50) NOT NULL CHECK (task_type IN ('Main','Subtask','Dependent')),
    status VARCHAR(50) NOT NULL,
    priority VARCHAR(50) CHECK (priority IN ('Low','Medium','High')),
    due_date DATE,
    stage VARCHAR(255),
    stage_item_id VARCHAR(255),
    owner_id INT NOT NULL REFERENCES public.users(userid) ON DELETE CASCADE,
    assignee_id INT REFERENCES public.users(userid) ON DELETE SET NULL
);

-- Corrected Indexes for faster querying
CREATE INDEX idx_tasks_task_type ON public.task(task_type);
CREATE INDEX idx_tasks_status ON public.task(status);
CREATE INDEX idx_tasks_owner_id ON public.task(owner_id);
CREATE INDEX idx_tasks_assignee_id ON public.task(assignee_id);
CREATE INDEX idx_tasks_parent_task_id ON public.task(parent_task_id);

----------------------------------------------------------------------
drop table sales_activity_checklists cascade;
-- Corrected public.sales_activity_checklists table schema
CREATE TABLE public.sales_activity_checklists (
    id SERIAL PRIMARY KEY,
    user_created INT REFERENCES public.users(userid) ON DELETE SET NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    user_updated INT REFERENCES public.users(userid) ON DELETE SET NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    description varchar(255),
    done BOOLEAN DEFAULT FALSE,
    check_list_title VARCHAR(255),
    task_id INT REFERENCES public.task(id) ON DELETE CASCADE
);

-- Corrected Index for quick lookup by task
CREATE INDEX idx_checklists_task_id ON public.sales_activity_checklists(task_id);
-----------------------------------------------------------------------------------------------
-- TRIGGER FUNCTION: Set quotation to Negotiation when task is created by salesrep and assigned to salesmanager
CREATE OR REPLACE FUNCTION public.set_quotation_negotiation_on_task()
RETURNS TRIGGER AS $$
DECLARE
    is_salesrep BOOLEAN;
    is_salesmanager BOOLEAN;
BEGIN
    -- Check if the task is for a quotation
    IF (NEW.stage = 'Quotation' AND NEW.stage_item_id IS NOT NULL) THEN
        -- Check if owner is salesrep and assignee is salesmanager
        SELECT EXISTS (
            SELECT 1 FROM public.userroles ur
            JOIN public.roles r ON ur.roleid = r.roleid
            WHERE ur.userid = NEW.owner_id AND r.rolename IN ('SalesRep', 'Sales Representative')
        ) INTO is_salesrep;
        SELECT EXISTS (
            SELECT 1 FROM public.userroles ur
            JOIN public.roles r ON ur.roleid = r.roleid
            WHERE ur.userid = NEW.assignee_id AND r.rolename IN ('SalesManager', 'Sales Manager')
        ) INTO is_salesmanager;

        IF is_salesrep AND is_salesmanager THEN
            -- Update the quotation status to 'Negotiation'
            UPDATE public.sales_quotations
            SET status = 'Negotiation'
            WHERE id = NEW.stage_item_id::int;
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger: On insert of task for quotation, set status to Negotiation if conditions met
DROP TRIGGER IF EXISTS trg_set_quotation_negotiation_on_task ON public.task;
CREATE TRIGGER trg_set_quotation_negotiation_on_task
AFTER INSERT ON public.task
FOR EACH ROW
EXECUTE FUNCTION public.set_quotation_negotiation_on_task();
----------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION public.generate_task_id()
RETURNS TRIGGER AS $$
DECLARE
    next_seq INT;
    year_part TEXT := TO_CHAR(CURRENT_DATE, 'YYYY');
    prefix TEXT := 'TSK-';
    new_task_id TEXT;
BEGIN
    -- Get the next sequence number for the current year
    SELECT COALESCE(MAX(CAST(SPLIT_PART(task_id, '-', 3) AS INTEGER)), 0) + 1
      INTO next_seq
      FROM public.task
     WHERE task_id LIKE prefix || year_part || '-%';

    new_task_id := prefix || year_part || '-' || LPAD(next_seq::TEXT, 3, '0');
    NEW.task_id := new_task_id;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_generate_task_id ON public.task;
CREATE TRIGGER trg_generate_task_id
BEFORE INSERT ON public.task
FOR EACH ROW
WHEN (NEW.task_id IS NULL OR NEW.task_id = '')
EXECUTE FUNCTION public.generate_task_id();

----------------------------------------------------------------------
CREATE OR REPLACE FUNCTION public.create_task_on_lead_insert()
RETURNS TRIGGER AS $$
DECLARE
    is_child boolean;
BEGIN
    -- Case 1: Sales Manager assigns to child (only if assigned_to is not null and not self)
    IF (NEW.assigned_to IS NOT NULL AND NEW.assigned_to <> NEW.user_created) THEN
        SELECT EXISTS (
            SELECT 1 FROM public.get_salesmanager_child_userids(NEW.user_created)
            WHERE userid = NEW.assigned_to
        ) INTO is_child;

        IF is_child THEN
            INSERT INTO public.task (
                task_name,
                description,
                task_type,
                status,
                priority,
                due_date,
                owner_id,
                assignee_id,
                stage,
                stage_item_id
            ) VALUES (
                'Follow up Lead: ' || COALESCE(NEW.customer_name, ''),
                COALESCE(NEW.comments, 'Auto-generated task for lead assignment (Lead ID: ' || NEW.id || ')'),
                'Main',
                'Open',
                'Medium',
                NULL,
                NEW.user_created,
                NEW.assigned_to,
                'Lead',
                NEW.id::text
            );
            RAISE NOTICE 'Task created and assigned to user ID % for lead ID %', NEW.assigned_to, NEW.id;
        END IF;
    END IF;

    -- Case 2: Self-assigned (SalesRep or any user creates lead for themselves)
    IF (NEW.assigned_to IS NULL OR NEW.assigned_to = NEW.user_created) THEN
        INSERT INTO public.task (
            task_name,
            description,
            task_type,
            status,
            priority,
            due_date,
            owner_id,
            assignee_id,
            stage,
            stage_item_id
        ) VALUES (
            'Follow up Lead: ' || COALESCE(NEW.customer_name, ''),
            COALESCE(NEW.comments, 'Auto-generated task for self-assigned lead (Lead ID: ' || NEW.id || ')'),
            'Main',
            'Open',
            'Medium',
            NULL,
            NEW.user_created,
            NEW.user_created,
            'Lead',
            NEW.id::text
        );
        RAISE NOTICE 'Self-assigned follow-up task created for user ID % for lead ID %', NEW.user_created, NEW.id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

----------------------------------------------------------------------
-- Trigger and function: Create task for finance when new purchase requisition is created
-- Assumes purchase_requisitions table exists with id, pr_number, description, status, requested_by, etc.
-- You may need to adjust 'assignee_id' to match your finance department user/group
CREATE OR REPLACE FUNCTION public.create_task_on_pr_insert()
RETURNS TRIGGER AS $$
DECLARE
    finance_user_id INT;
BEGIN
    -- Set this to the user ID of the finance department or use a lookup if needed
    finance_user_id := (
        SELECT ur.userid
        FROM public.userroles ur
        JOIN public.roles r ON ur.roleid = r.roleid
        WHERE r.rolename ILIKE 'Finance Department'
        LIMIT 1
    );
    IF (NEW.status = 'Pending') THEN
        INSERT INTO public.task (
            task_name,
            description,
            task_type,
            status,
            priority,
            due_date,
            owner_id,
            assignee_id,
            stage,
            stage_item_id
        ) VALUES (
            'Approve Purchase Requisition: ' || NEW.purchase_requisition_id,
            COALESCE(NEW.description, 'Auto-generated task for PR approval (PR ID: ' || NEW.id || ')'),
            'Main',
            'Pending',
            'Medium',
            NULL,
            NEW.user_created,
            finance_user_id,
            'PurchaseRequisition',
            NEW.id::text
        );
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_create_task_on_pr_insert ON public.purchase_requisitions;
CREATE TRIGGER trg_create_task_on_pr_insert
AFTER INSERT ON public.purchase_requisitions
FOR EACH ROW
EXECUTE FUNCTION public.create_task_on_pr_insert();

----------------------------------------------------------------------------------------
-- Trigger and function: Create task for Finance when new claim is created
CREATE OR REPLACE FUNCTION public.create_task_on_claim_insert()
RETURNS TRIGGER AS $$
DECLARE
    finance_user_id INT;
    owner_id INT;
BEGIN
    -- Lookup a user in the Finance Department (role name may vary)
    finance_user_id := (
        SELECT ur.userid
        FROM public.userroles ur
        JOIN public.roles r ON ur.roleid = r.roleid
        WHERE r.rolename ILIKE 'Finance Department'
        LIMIT 1
    );

    -- If we found a finance user, create a task notifying finance to review the claim.
    -- Note: Claim per-item data (amount, bill_url) moved to claim_items table. Avoid referencing
    -- removed columns here; the claim_items triggers will update the task with aggregated info.
    -- Determine a non-null owner for the task: prefer claim creator, else finance user, else any existing user
    owner_id := COALESCE(NEW.user_created, finance_user_id, (SELECT userid FROM public.users LIMIT 1));
    IF owner_id IS NULL THEN
        RAISE WARNING 'No owner user found when creating task for claim ID %; task insert may fail due to NOT NULL constraint', NEW.id;
    END IF;

    INSERT INTO public.task (
        task_name,
        description,
        task_type,
        status,
        priority,
        due_date,
        owner_id,
        assignee_id,
        stage,
        stage_item_id
    ) VALUES (
        'Review Claim: ' || COALESCE(NEW.claim_no::text, NEW.id::text),
        COALESCE('Claim submitted by ' || COALESCE(NEW.user_name, 'Unknown') ||
                 ' | Claim ID: ' || NEW.id::text,
                 'Auto-generated task for claim (ID: ' || NEW.id::text || ')'),
        'Main',
        'Pending',
        'Medium',
        NULL,
        owner_id,
        finance_user_id,
        'Claim',
        NEW.id::text
    );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_create_task_on_claim_insert ON public.claims;
CREATE TRIGGER trg_create_task_on_claim_insert
AFTER INSERT ON public.claims
FOR EACH ROW
EXECUTE FUNCTION public.create_task_on_claim_insert();

----------------------------------------------------------------------------

-- Update task description (amount/bill) whenever claim_items change (insert/update/delete)
CREATE OR REPLACE FUNCTION public.update_task_on_claim_items_change()
RETURNS TRIGGER AS $$
DECLARE
    cid INT;
    total_amount NUMERIC := 0;
    bill_list TEXT := NULL;
BEGIN
    IF TG_OP = 'DELETE' THEN
        cid := OLD.claim_id;
    ELSE
        cid := NEW.claim_id;
    END IF;

    SELECT COALESCE(SUM(amount), 0) INTO total_amount FROM public.claim_items WHERE claim_id = cid;
    SELECT string_agg(DISTINCT bill_url, ', ') FILTER (WHERE bill_url IS NOT NULL) INTO bill_list FROM public.claim_items WHERE claim_id = cid;

    -- Update the task description for the Claim task created earlier
    UPDATE public.task t
    SET description = COALESCE('Claim submitted by ' || COALESCE(c.user_name, 'Unknown') ||
                 ' | Amount: ' || total_amount::text ||
                 ' | Claim ID: ' || c.id::text ||
                 COALESCE(' | Bill: ' || bill_list, ''), t.description)
    FROM public.claims c
    WHERE t.stage = 'Claim' AND t.stage_item_id = cid::text AND c.id = cid;

    RETURN NULL; -- triggers that modify other tables typically return NULL
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_update_task_on_claim_items_change ON public.claim_items;
CREATE TRIGGER trg_update_task_on_claim_items_change
AFTER INSERT OR UPDATE OR DELETE ON public.claim_items
FOR EACH ROW
EXECUTE FUNCTION public.update_task_on_claim_items_change();

----------------------------------------------------------------------
-- Trigger and function: Create task for inventory department when new demo is created
-- Notifies inventory department about demo with demo date, demo time, customer name, and item details
CREATE OR REPLACE FUNCTION public.create_task_on_demo_insert()
RETURNS TRIGGER AS $$
DECLARE
    inventory_user_id INT;
    task_description TEXT;
BEGIN
    -- Get a user from the Inventory Department role
    inventory_user_id := (
        SELECT ur.userid
        FROM public.userroles ur
        JOIN public.roles r ON ur.roleid = r.roleid
        WHERE r.rolename ILIKE 'Inventory Department'
        LIMIT 1
    );

    IF inventory_user_id IS NOT NULL THEN
        -- Build description with demo details: demo_date, demo_time, customer_name
        task_description := 'Demo scheduled for customer: ' || COALESCE(NEW.customer_name, 'N/A') || 
                           ' | Demo Date: ' || COALESCE(NEW.demo_date::text, 'N/A') ||
                           ' | Demo Time: ' || COALESCE(NEW.demo_time::text, 'N/A') ||
                           ' | Demo ID: ' || NEW.id;

        -- Create a task for inventory department
        INSERT INTO public.task (
            task_name,
            description,
            task_type,
            status,
            priority,
            due_date,
            owner_id,
            assignee_id,
            stage,
            stage_item_id
        ) VALUES (
            'Demo Notification: ' || COALESCE(NEW.customer_name, 'Demo'),
            task_description,
            'Main',
            'Open',
            'Medium',
            NEW.demo_date::date,
            NEW.user_created,
            inventory_user_id,
            'Demo',
            NEW.id::text
        );

        RAISE NOTICE 'Task created and assigned to Inventory Department (user ID: %) for demo ID: %', inventory_user_id, NEW.id;
    ELSE
        RAISE WARNING 'No Inventory Department user found. Task not created for demo ID: %', NEW.id;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_create_task_on_demo_insert ON public.sales_demos;
CREATE TRIGGER trg_create_task_on_demo_insert
AFTER INSERT ON public.sales_demos
FOR EACH ROW
EXECUTE FUNCTION public.create_task_on_demo_insert();

----------------------------------------------------------------------
-- Trigger and function: Update task description when demo items are added
-- This adds comprehensive item details including BOM info to the task description for better tracking
-- NOTE: Currently DISABLED - Task is only created when demo is created, not updated when items are added
-- To enable, uncomment the trigger at the bottom
CREATE OR REPLACE FUNCTION public.update_task_on_demo_items_insert()
RETURNS TRIGGER AS $$
DECLARE
    task_record RECORD;
    item_details_text TEXT;
    item_record RECORD;
    category_name VARCHAR(255);
    group_name VARCHAR(255);
    make_name VARCHAR(255);
    model_name VARCHAR(255);
    product_name VARCHAR(255);
    uom_name VARCHAR(50);
BEGIN
    -- Get the task for this demo
    SELECT * INTO task_record
    FROM public.task
    WHERE stage = 'Demo' AND stage_item_id = NEW.demo_id::text AND status = 'Open'
    LIMIT 1;

    IF task_record IS NOT NULL THEN
        -- Get comprehensive item details from item_master with all lookups
        SELECT 
            im.item_name,
            im.item_description,
            im.item_code,
            im.specification,
            im.unit_price,
            im.tax_percentage,
            im.hsn,
            im.cat_no,
            c.category_name,
            ig.group_name,
            mk.make_name,
            md.model_name,
            pr.product_name,
            u.uom_name
        INTO item_record
        FROM public.item_master im
        LEFT JOIN public.categories c ON im.category_id = c.id
        LEFT JOIN public.inventory_group ig ON im.group_id = ig.id
        LEFT JOIN public.make mk ON im.make_id = mk.id
        LEFT JOIN public.model md ON im.model_id = md.id
        LEFT JOIN public.product pr ON im.product_id = pr.id
        LEFT JOIN public.uom u ON im.uom_id = u.id
        WHERE im.id = NEW.item_id
        LIMIT 1;

        -- Build comprehensive item details text
        item_details_text := '';
        
        -- Add item name
        IF item_record IS NOT NULL AND item_record.item_name IS NOT NULL THEN
            item_details_text := item_details_text || 'Item: ' || item_record.item_name;
        ELSE
            item_details_text := item_details_text || 'Item ID: ' || NEW.item_id;
        END IF;

        -- Add item code
        IF item_record IS NOT NULL AND item_record.item_code IS NOT NULL THEN
            item_details_text := item_details_text || ' (Code: ' || item_record.item_code || ')';
        END IF;

        -- Add quantity
        item_details_text := item_details_text || ' | Qty: ' || COALESCE(NEW.qty::text, '1');

        -- Add UOM
        IF item_record IS NOT NULL AND item_record.uom_name IS NOT NULL THEN
            item_details_text := item_details_text || ' | UOM: ' || item_record.uom_name;
        END IF;

        -- Add unit price if available
        IF NEW.unit_price IS NOT NULL AND NEW.unit_price > 0 THEN
            item_details_text := item_details_text || ' | Unit Price: ₹' || NEW.unit_price;
        ELSIF item_record IS NOT NULL AND item_record.unit_price IS NOT NULL AND item_record.unit_price > 0 THEN
            item_details_text := item_details_text || ' | Unit Price: ₹' || item_record.unit_price;
        END IF;

        -- Add category
        IF item_record IS NOT NULL AND item_record.category_name IS NOT NULL THEN
            item_details_text := item_details_text || ' | Category: ' || item_record.category_name;
        END IF;

        -- Add group
        IF item_record IS NOT NULL AND item_record.group_name IS NOT NULL THEN
            item_details_text := item_details_text || ' | Group: ' || item_record.group_name;
        END IF;

        -- Add make
        IF item_record IS NOT NULL AND item_record.make_name IS NOT NULL THEN
            item_details_text := item_details_text || ' | Make: ' || item_record.make_name;
        END IF;

        -- Add model
        IF item_record IS NOT NULL AND item_record.model_name IS NOT NULL THEN
            item_details_text := item_details_text || ' | Model: ' || item_record.model_name;
        END IF;

        -- Add product
        IF item_record IS NOT NULL AND item_record.product_name IS NOT NULL THEN
            item_details_text := item_details_text || ' | Product: ' || item_record.product_name;
        END IF;

        -- Add HSN
        IF item_record IS NOT NULL AND item_record.hsn IS NOT NULL THEN
            item_details_text := item_details_text || ' | HSN: ' || item_record.hsn;
        END IF;

        -- Add Catalogue Number
        IF item_record IS NOT NULL AND item_record.cat_no IS NOT NULL THEN
            item_details_text := item_details_text || ' | Cat No: ' || item_record.cat_no;
        END IF;

        -- Add tax percentage
        IF item_record IS NOT NULL AND item_record.tax_percentage IS NOT NULL THEN
            item_details_text := item_details_text || ' | Tax: ' || item_record.tax_percentage || '%';
        END IF;

        -- Update the task description to include comprehensive item details
        -- Append items separated by semicolon for multiple items
        IF task_record.description LIKE '%| Items:%' THEN
            -- Items already exist, append with semicolon separator
            UPDATE public.task
            SET description = description || '; ' || item_details_text,
                updated_at = CURRENT_TIMESTAMP
            WHERE id = task_record.id;
        ELSE
            -- First item, add the Items label
            UPDATE public.task
            SET description = description || ' | Items: ' || item_details_text,
                updated_at = CURRENT_TIMESTAMP
            WHERE id = task_record.id;
        END IF;

        RAISE NOTICE 'Task updated with comprehensive item details for demo ID: %', NEW.demo_id;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

----------------------------------------------------------------------
-- Trigger: Create/Update task when demo items are added
-- When items are inserted into sales_demo_items, the task description is updated with item details
DROP TRIGGER IF EXISTS trg_update_task_on_demo_items_insert ON public.sales_demo_items;
CREATE TRIGGER trg_update_task_on_demo_items_insert
AFTER INSERT ON public.sales_demo_items
FOR EACH ROW
EXECUTE FUNCTION public.update_task_on_demo_items_insert();

