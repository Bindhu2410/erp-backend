-- Test script to verify user-based lead functionality
-- Run this after applying the main migration

-- 1. Test user-specific lead ID generation
SELECT 'Testing user-specific lead ID generation...' as test_step;

-- Test for user 1
SELECT get_next_user_lead_id(1) as user1_next_id;
-- Should return LD00001 if no leads exist for user 1

-- Test for user 2  
SELECT get_next_user_lead_id(2) as user2_next_id;
-- Should return LD00001 if no leads exist for user 2

-- 2. Test user-based lead grid function
SELECT 'Testing user-based lead grid function...' as test_step;

-- Test grid for user 1 (should return only user 1's leads)
SELECT COUNT(*) as user1_leads_count 
FROM sales_lead_grid_by_user(1, NULL, NULL, NULL, NULL, NULL, NULL, 1, 10, 'id', 'DESC');

-- Test grid for user 2 (should return only user 2's leads)  
SELECT COUNT(*) as user2_leads_count
FROM sales_lead_grid_by_user(2, NULL, NULL, NULL, NULL, NULL, NULL, 1, 10, 'id', 'DESC');

-- 3. Test user-based lead cards count
SELECT 'Testing user-based lead cards count...' as test_step;

-- Test cards for user 1
SELECT * FROM get_user_lead_cards_count(1);

-- Test cards for user 2
SELECT * FROM get_user_lead_cards_count(2);

-- 4. Test foreign key constraints
SELECT 'Testing foreign key constraints...' as test_step;

-- Verify foreign key constraints exist
SELECT 
    tc.constraint_name,
    tc.table_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY'
    AND tc.table_name = 'sales_lead'
    AND kcu.column_name IN ('user_created', 'user_updated');

-- 5. Test indexes exist
SELECT 'Testing indexes exist...' as test_step;

SELECT 
    indexname,
    tablename,
    indexdef
FROM pg_indexes 
WHERE tablename = 'sales_lead' 
    AND indexname LIKE 'idx_sales_lead%';

-- 6. Insert sample data for testing (optional)
SELECT 'Creating sample test data...' as test_step;

-- Insert sample users if they don't exist
INSERT INTO users (userid, username, email, is_active, date_created)
VALUES 
    (1, 'testuser1', 'user1@test.com', true, NOW()),
    (2, 'testuser2', 'user2@test.com', true, NOW())
ON CONFLICT (userid) DO NOTHING;

-- Insert sample leads for testing
INSERT INTO sales_lead (
    user_created, date_created, user_updated, date_updated,
    customer_name, lead_source, lead_id, status, isactive
) VALUES 
    (1, NOW(), 1, NOW(), 'Test Customer 1', 'Website', 'LD00001', 'New', true),
    (1, NOW(), 1, NOW(), 'Test Customer 2', 'Referral', 'LD00002', 'New', true),
    (2, NOW(), 2, NOW(), 'Test Customer 3', 'Email', 'LD00001', 'New', true),
    (2, NOW(), 2, NOW(), 'Test Customer 4', 'Phone', 'LD00002', 'New', true)
ON CONFLICT DO NOTHING;

-- 7. Verify data isolation
SELECT 'Verifying user data isolation...' as test_step;

-- Count leads per user
SELECT 
    user_created,
    COUNT(*) as lead_count,
    string_agg(lead_id, ', ' ORDER BY lead_id) as lead_ids
FROM sales_lead 
WHERE customer_name LIKE 'Test Customer%'
GROUP BY user_created
ORDER BY user_created;

-- Test search within user data
SELECT 'Testing search within user data...' as test_step;

-- Search for user 1's leads
SELECT id, lead_id, customer_name, user_created
FROM sales_lead_grid_by_user(1, 'Test Customer', NULL, NULL, NULL, NULL, NULL, 1, 10, 'id', 'DESC');

-- Search for user 2's leads  
SELECT id, lead_id, customer_name, user_created
FROM sales_lead_grid_by_user(2, 'Test Customer', NULL, NULL, NULL, NULL, NULL, 1, 10, 'id', 'DESC');

SELECT 'All tests completed!' as test_result;
