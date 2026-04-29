   CREATE TABLE demo_inventory (
    id SERIAL PRIMARY KEY,   
    item_id int REFERENCES inventory_items(id),
    status VARCHAR(50) CHECK (status IN ('In Stock', 'Demo Assigned', 'In Transit', 'Under Repair', 'Returned', 'Decommissioned')),
    condition VARCHAR(50) CHECK (condition IN ('New', 'Like New', 'Used', 'Damaged', 'Not Working')),
    demo_start_date TIMESTAMP,
    demo_expected_end_date TIMESTAMP,
    demo_actual_end_date TIMESTAMP,
    assigned_to_type VARCHAR(20) CHECK (assigned_to_type IN ('Customer', 'Sales Rep', 'Event', 'Partner')),
    notes Varchar(255),
    original_cost DECIMAL(18,2),
    current_value DECIMAL(18,2),
    last_inspection_date TIMESTAMP,
    last_maintenance_date TIMESTAMP,
     user_created int4 NULL,
    date_created timestamp NULL,
    user_updated int4 NULL,
    date_updated timestamp NULL

);
alter table demo_inventory 
 ADD CONSTRAINT fk_demo_inventory_user_created FOREIGN KEY (user_created) REFERENCES public.sales_employees(id),
    ADD CONSTRAINT fk_demo_inventory_user_updated FOREIGN KEY (user_updated) REFERENCES public.sales_employees(id);
 