-- Create attendance table
CREATE TABLE public.attendance (
    id serial4 NOT NULL,
    user_created int4 NULL,
    date_created timestamp NULL,
    user_updated int4 NULL,
    date_updated timestamp NULL,
    employee_id int4 NOT NULL,
    attendance_date date NOT NULL,
    check_in_time time NULL,
    check_out_time time NULL,
    status varchar(50) NULL,
    remarks text NULL,
    CONSTRAINT attendance_pkey PRIMARY KEY (id),
    CONSTRAINT unique_attendance_date UNIQUE (employee_id, attendance_date)
);

-- Add foreign key constraint to employees table
ALTER TABLE public.attendance ADD CONSTRAINT attendance_employee_id_fkey 
    FOREIGN KEY (employee_id) REFERENCES public.employees(id);

-- Add foreign key constraints to users table for audit fields
ALTER TABLE public.attendance ADD CONSTRAINT attendance_user_created_fkey 
    FOREIGN KEY (user_created) REFERENCES public.users(userid);

ALTER TABLE public.attendance ADD CONSTRAINT attendance_user_updated_fkey 
    FOREIGN KEY (user_updated) REFERENCES public.users(userid);

-- Create indexes for better query performance
CREATE INDEX idx_attendance_employee_id ON public.attendance(employee_id);
CREATE INDEX idx_attendance_date ON public.attendance(attendance_date);
CREATE INDEX idx_attendance_employee_date ON public.attendance(employee_id, attendance_date);

-- Status enum values: Present, Absent, Late, Half Day, Leave
-- Example insert:
-- INSERT INTO public.attendance (employee_id, attendance_date, check_in_time, check_out_time, status, user_created, date_created)
-- VALUES (1, CURRENT_DATE, '09:00:00', '17:00:00', 'Present', 1, CURRENT_TIMESTAMP);
