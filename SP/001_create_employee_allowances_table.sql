-- ============================================================================
-- Migration Script: Add employee_allowances table for multiple allowances
-- Database: PostgreSQL
-- Purpose: Support multiple allowance details per employee
-- ============================================================================

-- Create the employee_allowances table if it doesn't exist
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

-- Create composite index for querying active allowances
CREATE INDEX IF NOT EXISTS idx_employee_allowance_active 
ON employee_allowances(employee_id, active);

-- ============================================================================
-- Optional: Add 'active' column to 'employees' table if it doesn't exist
-- This is for consistency with other entities that use soft delete
-- ============================================================================

-- Check if 'active' column exists in employees table
-- If not, uncomment and run the following:
-- ALTER TABLE employees ADD COLUMN IF NOT EXISTS active BOOLEAN DEFAULT true;

-- ============================================================================
-- Verify the table structure
-- ============================================================================

-- Run this to verify the table was created successfully:
-- SELECT * FROM information_schema.columns 
-- WHERE table_name = 'employee_allowances' 
-- ORDER BY ordinal_position;

-- ============================================================================
-- Optional: Migrate data from old single allowance fields (if needed)
-- ============================================================================

-- If you want to migrate existing allowance data from the employees table:
-- INSERT INTO employee_allowances (employee_id, allow_type, allow_amount, allow_eff_from, allow_eff_to, user_created, date_created, user_updated, date_updated, active)
-- SELECT 
--     id as employee_id,
--     allowance_type as allow_type,
--     allowance_amount as allow_amount,
--     allow_eff_from,
--     allow_eff_to,
--     user_created,
--     date_created,
--     user_updated,
--     date_updated,
--     COALESCE(active, true) as active
-- FROM employees
-- WHERE allowance_type IS NOT NULL OR allowance_amount IS NOT NULL;
