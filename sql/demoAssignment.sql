drop table demo_assignments ;
CREATE TABLE public.demo_assignments (
    id SERIAL PRIMARY KEY,
    user_created INT4 NULL,
    date_created TIMESTAMP NULL,
    user_updated INT4 NULL,
    date_updated TIMESTAMP NULL,
    demo_item_id int REFERENCES public.demo_inventory(id),
    assigned_to_type VARCHAR(20) CHECK (
        assigned_to_type IN ('Customer', 'Sales Rep', 'Event', 'Partner')
    ),
    assigned_to_id INT,
    assignment_start_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expected_return_date TIMESTAMP,
    actual_return_date TIMESTAMP,
    status VARCHAR(30)
       
    
);
-- Step 2: Add foreign key constraints
ALTER TABLE public.demo_assignments
    ADD CONSTRAINT fk_demo_assignments_user_created
    FOREIGN KEY (user_created) REFERENCES users(user_id);
 
ALTER TABLE public.demo_assignments
    ADD CONSTRAINT fk_demo_assignments_user_updated
    FOREIGN KEY (user_updated) REFERENCES users(user_id);
   
ALTER TABLE public.demo_assignments
    ADD CONSTRAINT fk_demo_assignments_assigned_to
    FOREIGN KEY (assigned_to_id) REFERENCES public.users(user_id);

----latest