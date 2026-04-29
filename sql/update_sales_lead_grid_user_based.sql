-- Update sales_lead_grid function to include user-based filtering
-- Run this script to update the function with user ID parameter

-- First drop the old function signature
DROP FUNCTION IF EXISTS sales_lead_grid(text,text[],text[],text[],text[],text[],integer,integer,text,text);

-- Apply the updated function from leadGrid.sql
-- This is already defined in leadGrid.sql with the user ID parameter

-- Grant permissions
GRANT EXECUTE ON FUNCTION sales_lead_grid TO PUBLIC;

-- Verify the function exists
SELECT proname, pg_get_function_arguments(oid) as arguments 
FROM pg_proc 
WHERE proname = 'sales_lead_grid';
