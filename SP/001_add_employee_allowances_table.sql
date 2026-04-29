-- Migration: Add employee_allowances table for multiple allowances per employee

-- Create the employee_allowances table
CREATE TABLE IF NOT EXISTS employee_allowances (
    id SERIAL PRIMARY KEY,
    employee_id INTEGER NOT NULL,
    allow_type VARCHAR(100),
    allow_amount NUMERIC(12,2),
    allow_eff_from DATE,
    allow_eff_to DATE,
    active BOOLEAN DEFAULT true,
    user_created INTEGER,
    date_created TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (employee_id) REFERENCES employees(id) ON DELETE CASCADE
);

-- Create index on employee_id for faster lookups
CREATE INDEX IF NOT EXISTS idx_employee_allowance_employee_id 
ON employee_allowances(employee_id);

-- Drop the old allowance columns from employees table if they exist
-- (optional - comment out if you want to keep for backward compatibility)
-- ALTER TABLE employees DROP COLUMN IF EXISTS allow_eff_from;
-- ALTER TABLE employees DROP COLUMN IF EXISTS allow_eff_to;
-- ALTER TABLE employees DROP COLUMN IF EXISTS allowance_type;
-- ALTER TABLE employees DROP COLUMN IF EXISTS allowance_amount;

-- Note: The above ALTER commands are commented because removing columns is a breaking change
-- Keep the old columns for now to avoid data loss. They will be deprecated.
-- You can remove them in a future migration after data migration is complete.
