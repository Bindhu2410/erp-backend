-- Updated fix for "function public.fn_get_filtered_sales_deals(integer, unknown, unknown, unknown, unknown, unknown, unknown, text, text, integer, integer) does not exist" error

-- First, add the division_id column to sales_deals table if it doesn't exist
DO $$ 
BEGIN
  -- Check if the column exists before attempting to add it
  IF NOT EXISTS (
    SELECT FROM information_schema.columns 
    WHERE table_name = 'sales_deals' 
    AND column_name = 'division_id'
  ) THEN
    -- Add the division_id column
    ALTER TABLE public.sales_deals 
    ADD COLUMN division_id INT DEFAULT NULL;
  END IF;
END $$;

-- Drop existing functions that we're going to recreate
DROP FUNCTION IF EXISTS public.fn_get_filtered_sales_deals;
DROP FUNCTION IF EXISTS public.fn_count_filtered_sales_deals;

-- Recreate the filtered sales deals function with correct parameters and table references
CREATE OR REPLACE FUNCTION public.fn_get_filtered_sales_deals(
    IN p_territory_id INT DEFAULT NULL,
    IN p_zone_id INT DEFAULT NULL,
    IN p_division_id INT DEFAULT NULL,
    IN p_customer_name VARCHAR DEFAULT NULL,
    IN p_status VARCHAR DEFAULT NULL,
    IN p_score VARCHAR DEFAULT NULL,
    IN p_lead_type VARCHAR DEFAULT NULL,
    IN p_search_term VARCHAR DEFAULT NULL,
    IN p_sort_column VARCHAR DEFAULT 'id',
    IN p_sort_direction VARCHAR DEFAULT 'ASC',
    IN p_page_number INT DEFAULT 1,
    IN p_page_size INT DEFAULT 10
)
RETURNS TABLE (
    id INT,
    deal_id INT,
    clinic_hospital_individual VARCHAR,
    lead_id VARCHAR,
    amount FLOAT8,
    status VARCHAR,
    customer_name VARCHAR,
    closing_date DATE,
    territory VARCHAR,
    contact_name VARCHAR,
    payment_status VARCHAR,
    expected_revenue FLOAT8,
    deal_age VARCHAR,
    contact_phone VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INT;
BEGIN
    v_offset := (p_page_number - 1) * p_page_size;
    
    RETURN QUERY
    SELECT 
        sd.id,
        sd.id AS deal_id,
        sd.deal_for AS clinic_hospital_individual,
        COALESCE(so.lead_id, CASE WHEN so.sales_leads_id IS NOT NULL THEN 'LD-' || so.sales_leads_id::TEXT ELSE NULL END, 'N/A') AS lead_id,
        sd.amount,
        sd.status,
        COALESCE(so.customer_name, sd.deal_name, 'N/A') AS customer_name,
        sd.close_date AS closing_date,
        COALESCE(st.name, 'N/A') AS territory,
        COALESCE(so.contact_name, 'N/A') AS contact_name,
        'N/A' AS payment_status, -- Placeholder for actual payment status if available
        sd.expected_revenue,
        sd.deal_age,
        COALESCE(so.contact_mobile_no, 'N/A') AS contact_phone
    FROM 
        public.sales_deals sd
    LEFT JOIN 
        public.sales_opportunities so ON sd.opportunities_id = so.id
    LEFT JOIN
        public.sales_territories st ON sd.territory_id = st.id
    WHERE 
        sd.isactive = TRUE
        AND (p_territory_id IS NULL OR sd.territory_id = p_territory_id)
        AND (p_zone_id IS NULL OR (st.zone_id = p_zone_id))
        AND (p_division_id IS NULL OR sd.division_id = p_division_id)
        AND (p_status IS NULL OR sd.status = p_status)
        AND (p_lead_type IS NULL OR sd.deal_for = p_lead_type)
        AND (p_customer_name IS NULL OR 
             COALESCE(so.customer_name, sd.deal_name, '') ILIKE '%' || p_customer_name || '%')
        AND (p_search_term IS NULL OR 
             sd.deal_name ILIKE '%' || p_search_term || '%' OR
             COALESCE(sd.comments, '') ILIKE '%' || p_search_term || '%' OR
             sd.status ILIKE '%' || p_search_term || '%')
    ORDER BY
        CASE WHEN p_sort_column = 'id' AND p_sort_direction = 'ASC' THEN sd.id END ASC,
        CASE WHEN p_sort_column = 'id' AND p_sort_direction = 'DESC' THEN sd.id END DESC,
        CASE WHEN p_sort_column = 'customer_name' AND p_sort_direction = 'ASC' THEN COALESCE(so.customer_name, sd.deal_name) END ASC,
        CASE WHEN p_sort_column = 'customer_name' AND p_sort_direction = 'DESC' THEN COALESCE(so.customer_name, sd.deal_name) END DESC,
        CASE WHEN p_sort_column = 'amount' AND p_sort_direction = 'ASC' THEN sd.amount END ASC,
        CASE WHEN p_sort_column = 'amount' AND p_sort_direction = 'DESC' THEN sd.amount END DESC,
        CASE WHEN p_sort_column = 'closing_date' AND p_sort_direction = 'ASC' THEN sd.close_date END ASC,
        CASE WHEN p_sort_column = 'closing_date' AND p_sort_direction = 'DESC' THEN sd.close_date END DESC,
        CASE WHEN p_sort_column = 'status' AND p_sort_direction = 'ASC' THEN sd.status END ASC,
        CASE WHEN p_sort_column = 'status' AND p_sort_direction = 'DESC' THEN sd.status END DESC,
        CASE WHEN p_sort_column = 'territory' AND p_sort_direction = 'ASC' THEN st.name END ASC,
        CASE WHEN p_sort_column = 'territory' AND p_sort_direction = 'DESC' THEN st.name END DESC,
        sd.id ASC  -- Default fallback sort
    LIMIT p_page_size OFFSET v_offset;
END;
$$;

-- Recreate the count function with correct parameters and table references
CREATE OR REPLACE FUNCTION public.fn_count_filtered_sales_deals(
    IN p_territory_id INT DEFAULT NULL,
    IN p_zone_id INT DEFAULT NULL,
    IN p_division_id INT DEFAULT NULL,
    IN p_customer_name VARCHAR DEFAULT NULL,
    IN p_status VARCHAR DEFAULT NULL,
    IN p_score VARCHAR DEFAULT NULL,
    IN p_lead_type VARCHAR DEFAULT NULL,
    IN p_search_term VARCHAR DEFAULT NULL
)
RETURNS INT
LANGUAGE plpgsql
AS $$
DECLARE
    v_count INT;
BEGIN
    SELECT 
        COUNT(*)
    INTO 
        v_count
    FROM 
        public.sales_deals sd
    LEFT JOIN 
        public.sales_opportunities so ON sd.opportunities_id = so.id
    LEFT JOIN
        public.sales_territories st ON sd.territory_id = st.id
    WHERE 
        sd.isactive = TRUE
        AND (p_territory_id IS NULL OR sd.territory_id = p_territory_id)
        AND (p_zone_id IS NULL OR (st.zone_id = p_zone_id))
        AND (p_division_id IS NULL OR sd.division_id = p_division_id)
        AND (p_status IS NULL OR sd.status = p_status)
        AND (p_lead_type IS NULL OR sd.deal_for = p_lead_type)
        AND (p_customer_name IS NULL OR 
             COALESCE(so.customer_name, sd.deal_name, '') ILIKE '%' || p_customer_name || '%')
        AND (p_search_term IS NULL OR 
             sd.deal_name ILIKE '%' || p_search_term || '%' OR
             COALESCE(sd.comments, '') ILIKE '%' || p_search_term || '%' OR
             sd.status ILIKE '%' || p_search_term || '%');
    
    RETURN v_count;
END;
$$;

-- Update the stored procedures to include division_id parameter
CREATE OR REPLACE PROCEDURE public.sp_create_sales_deal(
    IN p_user_created INT,
    IN p_status VARCHAR(255),
    IN p_deal_name VARCHAR(255),
    IN p_amount FLOAT8,
    IN p_expected_revenue FLOAT8,
    IN p_deal_age VARCHAR(255),
    IN p_deal_for VARCHAR(255),
    IN p_close_date DATE,
    IN p_comments TEXT,
    IN p_opportunities_id INT4,
    IN p_sales_representative_id INT4,
    IN p_territory_id INT4,
    IN p_area_id INT4,
    IN p_city_id INT4,
    IN p_district_id INT4,
    IN p_state_id INT4,
    IN p_pincode_id INT4,
    IN p_division_id INT4,
    OUT p_id INT4
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO public.sales_deals (
        user_created, date_created, status, deal_name, amount, 
        expected_revenue, deal_age, deal_for, close_date, isactive, 
        comments, opportunities_id, sales_representative_id, territory_id, 
        area_id, city_id, district_id, state_id, pincode_id, division_id
    )
    VALUES (
        p_user_created, NOW(), p_status, p_deal_name, p_amount, 
        p_expected_revenue, p_deal_age, p_deal_for, p_close_date, TRUE, 
        p_comments, p_opportunities_id, p_sales_representative_id, p_territory_id, 
        p_area_id, p_city_id, p_district_id, p_state_id, p_pincode_id, p_division_id
    )
    RETURNING id INTO p_id;
END;
$$;

-- Also update the update procedure to support division_id
CREATE OR REPLACE PROCEDURE public.sp_update_sales_deal(
    IN p_id INT4,
    IN p_user_updated INT4,
    IN p_status VARCHAR(255),
    IN p_deal_name VARCHAR(255),
    IN p_amount FLOAT8,
    IN p_expected_revenue FLOAT8,
    IN p_deal_age VARCHAR(255),
    IN p_deal_for VARCHAR(255),
    IN p_close_date DATE,
    IN p_comments TEXT,
    IN p_opportunities_id INT4,
    IN p_sales_representative_id INT4,
    IN p_territory_id INT4,
    IN p_area_id INT4,
    IN p_city_id INT4,
    IN p_district_id INT4,
    IN p_state_id INT4,
    IN p_pincode_id INT4,
    IN p_division_id INT4,
    OUT p_success BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE public.sales_deals SET
        user_updated = p_user_updated,
        date_updated = NOW(),
        status = p_status,
        deal_name = p_deal_name,
        amount = p_amount,
        expected_revenue = p_expected_revenue,
        deal_age = p_deal_age,
        deal_for = p_deal_for,
        close_date = p_close_date,
        comments = p_comments,
        opportunities_id = p_opportunities_id,
        sales_representative_id = p_sales_representative_id,
        territory_id = p_territory_id,
        area_id = p_area_id,
        city_id = p_city_id,
        district_id = p_district_id,
        state_id = p_state_id,
        pincode_id = p_pincode_id,
        division_id = p_division_id
    WHERE id = p_id AND isactive = TRUE;
    
    IF FOUND THEN
        p_success := TRUE;
    ELSE
        p_success := FALSE;
    END IF;
END;
$$;
