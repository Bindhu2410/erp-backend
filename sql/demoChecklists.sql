CREATE TABLE demo_checklists (
    id SERIAL PRIMARY KEY,
    checklist_id int not null references demo_checklist_items(id),
    checklist_name VARCHAR(255) NOT NULL,
    demo_id INTEGER NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT FALSE,
    FOREIGN KEY (demo_id) REFERENCES sales_demos(id) ON DELETE CASCADE
);

CREATE TABLE public.demo_checklist_items (
	id serial4 NOT NULL,
	checklist_name varchar(255) NOT NULL,
	created_at timestamp DEFAULT CURRENT_TIMESTAMP NULL,
	updated_at timestamp DEFAULT CURRENT_TIMESTAMP NULL,
	is_active bool DEFAULT false NULL,
	CONSTRAINT demo_checklist_items_pkey PRIMARY KEY (id)
);

INSERT INTO demo_checklist_items (checklist_name)
VALUES 
('Verify Equipment Installation'),
('Check Power Supply Connections'),
('Inspect Device Calibration'),
('Validate Software Installation'),
('Confirm User Training Completion'),
('Review Safety Protocols'),
('Test Communication Modules'),
('Inspect Packaging Materials'),
('Check User Manual Availability'),
('Capture Customer Feedback');