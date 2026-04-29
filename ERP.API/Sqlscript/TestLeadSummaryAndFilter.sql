-- Test script for the Lead Summary and Filter functionality

-- Test Summary Cards function
SELECT * FROM public.sp_sales_lead_summary_cards();

-- Test Filter function with various parameters
-- 1. No filters, just sorting and pagination
SELECT 
    id, customer_name, territory_name, status, score, lead_type, 
    created_date, contact_name, contact_email, priority, total_count
FROM public.sp_sales_lead_filter(
    NULL, -- p_territory
    NULL, -- p_customer_name
    NULL, -- p_status
    NULL, -- p_score
    NULL, -- p_lead_type
    'customer_name', -- p_sort_field
    'ASC', -- p_sort_direction
    1, -- p_page_number
    10 -- p_page_size
);

-- 2. Filter by Status and Territory
SELECT 
    id, customer_name, territory_name, status, score, lead_type, 
    created_date, contact_name, contact_email, priority, total_count
FROM public.sp_sales_lead_filter(
    'North', -- p_territory
    NULL, -- p_customer_name
    'New', -- p_status
    NULL, -- p_score
    NULL, -- p_lead_type
    'created_date', -- p_sort_field (changed from date_created to match the new field name)
    'DESC', -- p_sort_direction
    1, -- p_page_number
    10 -- p_page_size
);

-- 3. Filter by Lead Type and Customer Name (partial match)
SELECT 
    id, customer_name, territory_name, status, score, lead_type, 
    created_date, contact_name, contact_email, priority, total_count
FROM public.sp_sales_lead_filter(
    NULL, -- p_territory
    'Hospital', -- p_customer_name (partial match)
    NULL, -- p_status
    NULL, -- p_score
    'Medical', -- p_lead_type
    'score', -- p_sort_field
    'DESC', -- p_sort_direction
    1, -- p_page_number
    10 -- p_page_size
);

-- Test My Leads function
SELECT 
    id, customer_name, territory_name, status, score, lead_type, 
    created_date, contact_name, contact_email, priority, total_count
FROM public.sp_sales_lead_my_leads(
    1, -- p_user_id (replace with actual user ID)
    'created_date', -- p_sort_field (changed from date_created to match the new field name)
    'DESC', -- p_sort_direction
    1, -- p_page_number
    10 -- p_page_size
);

-- Test Dropdown Options function
SELECT * FROM public.sp_sales_lead_dropdown_options();
