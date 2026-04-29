-- Email System Setup Script
-- Run this script to set up the email system database tables

-- First run the OAuth states table
\i OAuthStatesTable.sql

-- Then run the main email system tables
\i EmailSystemTables.sql

-- Verify tables were created
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
  AND table_name LIKE 'email_%' 
  OR table_name = 'oauth_states'
ORDER BY table_name;
