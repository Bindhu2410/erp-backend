-- Consolidated stored procedures for Sales Opportunities

-- Function to get sales products by opportunity
CREATE OR REPLACE FUNCTION get_sales_products_by_opportunity(p_opportunity_id INTEGER)
RETURNS TABLE (
    id INTEGER,
    userCreated INTEGER,
    dateCreated TIMESTAMP,
    userUpdated INTEGER,
    dateUpdated TIMESTAMP,
    makeId INTEGER,
    makeName VARCHAR(255),
    modelId INTEGER,
    modelName VARCHAR(255),
    productId INTEGER,
    productName VARCHAR(255),
    categoryId INTEGER,
    categoryName VARCHAR(255),
    itemCode VARCHAR(255),
    itemName VARCHAR(255),
    qty INTEGER,
    amount NUMERIC(18,2),
    stage VARCHAR(255),
    stageItemId BIGINT,
    inventoryItemsId BIGINT,
    isactive BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        sp.id,
        COALESCE(sp.user_created, 0) as userCreated,
        sp.date_created as dateCreated,
        COALESCE(sp.user_updated, 0) as userUpdated,
        sp.date_updated as dateUpdated,
        COALESCE(m.id, 0) as makeId,
        COALESCE(m.name, '') as makeName,
        COALESCE(md.id, 0) as modelId,
        COALESCE(md.name, '') as modelName,
        COALESCE(p.id, 0) as productId,
        COALESCE(p.name, '') as productName,
        COALESCE(c.id, 0) as categoryId,
        COALESCE(c.name, '') as categoryName,
        COALESCE(i.item_code, '') as itemCode,
        COALESCE(i.item_name, '') as itemName,
        COALESCE(sp.qty, 0) as qty,
        CAST(COALESCE(sp.amount, 0) AS NUMERIC(18,2)) as amount,
        COALESCE(sp.stage, '') as stage,
        CAST(sp.stage_item_id AS BIGINT) as stageItemId,
        sp.inventory_items_id as inventoryItemsId,
        sp.isactive
        -- Removed im.uom reference (column does not exist)
        -- Removed im.uom reference (column does not exist)
        
        -- Removed im.uom reference (column does not exist)
        -- Removed im.uom reference (column does not exist)
        
    FROM sales_product sp
    LEFT JOIN inventory_items i ON sp.item_id = i.id
    LEFT JOIN makes m ON i.make_id = m.id
    LEFT JOIN models md ON i.model_id = md.id
    LEFT JOIN products p ON i.product_id = p.id
    LEFT JOIN categories c ON i.category_id = c.id
    WHERE sp.stage_item_id::VARCHAR = p_opportunity_id::VARCHAR
    AND sp.is_active = true
    AND upper(sp.stage) = 'OPPORTUNITY'
    AND (i.isactive IS NULL OR i.isactive = true)
    ORDER BY sp.id;
END;
$$;

-- Function to get all active opportunities
CREATE OR REPLACE FUNCTION get_active_opportunities()
RETURNS TABLE (
    id INTEGER,
    userCreated INTEGER,
    dateCreated TIMESTAMP,
    userUpdated INTEGER,
    dateUpdated TIMESTAMP,
    status VARCHAR(255),
    expectedCompletion DATE,
    opportunityType VARCHAR(255),
    opportunityFor VARCHAR(255),
    customerId VARCHAR(255),
    customerName VARCHAR(255),
    customerType VARCHAR(255),
    opportunityName VARCHAR(255),
    opportunityId VARCHAR(255),
    comments TEXT,
    isactive BOOLEAN,
    leadId VARCHAR(255),
    salesRepresentativeId INTEGER,
    contactName VARCHAR(255),    contactMobileNo VARCHAR(255)
) 
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        o.id,
        o.user_created,
        o.date_created,
        o.user_updated,
        o.date_updated,
        o.status,
        o.expected_completion,
        o.opportunity_type,
        o.opportunity_for,
        o.customer_id,
        o.customer_name,
        o.customer_type,
        o.opportunity_name,
        o.opportunity_id,
        o.comments,
        o.isactive,
        o.lead_id,
        o.sales_representative_id,
        o.contact_name,
        o.contact_mobile_no,
        COALESCE(
            (
                SELECT json_agg(p)
                FROM (
                    SELECT 
                        sp.id AS "Id",
                        COALESCE(sp.user_created, 0) as "UserCreated",
                        sp.date_created as "DateCreated",
                        COALESCE(sp.user_updated, 0) as "UserUpdated",
                        sp.date_updated as "DateUpdated",
                        COALESCE(m.id, 0) as "MakeId",
                        COALESCE(m.name, '') as "MakeName",
                        COALESCE(md.id, 0) as "ModelId",
                        COALESCE(md.name, '') as "ModelName",
                        COALESCE(p.id, 0) as "ProductId",
                        COALESCE(p.name, '') as "ProductName",
                        COALESCE(c.id, 0) as "CategoryId",
                        COALESCE(c.name, '') as "CategoryName",
                        COALESCE(i.item_code, '') as "ItemCode",
                        COALESCE(i.item_name, '') as "ItemName",
                        COALESCE(sp.qty, 0) as "Quantity",
                        CAST(COALESCE(sp.amount, 0) AS NUMERIC(18,2)) as "Amount",
                        COALESCE(sp.stage, '') as "Stage",
                        CAST(sp.stage_item_id AS BIGINT) as "StageItemId",
                        sp.inventory_items_id as "InventoryItemsId",
                        sp.isactive as "IsActive"
                    FROM sales_products sp
                    LEFT JOIN inventory_items i ON sp.inventory_items_id = i.id
                    LEFT JOIN makes m ON i.make_id = m.id
                    LEFT JOIN models md ON i.model_id = md.id
                    LEFT JOIN products p ON i.product_id = p.id
                    LEFT JOIN categories c ON i.category_id = c.id
                    WHERE sp.stage_item_id = o.id
                    AND sp.isactive = true
                    AND upper(sp.stage) = 'OPPORTUNITY'
                    ORDER BY sp.id
                ) p
            ),
            '[]'::json
        ) as products
    FROM sales_opportunities o
    WHERE o.isactive = true
    ORDER BY o.date_created DESC;
END;
$$;

-- Function to get opportunity by ID
CREATE OR REPLACE FUNCTION get_opportunity_by_id(p_id INTEGER)
RETURNS TABLE (
    id INTEGER,
    userCreated INTEGER,
    dateCreated TIMESTAMP,
    userUpdated INTEGER,
    dateUpdated TIMESTAMP,
    status VARCHAR(255),
    expectedCompletion DATE,
    opportunityType VARCHAR(255),
    opportunityFor VARCHAR(255),
    customerId VARCHAR(255),
    customerName VARCHAR(255),
    customerType VARCHAR(255),
    opportunityName VARCHAR(255),
    opportunityId VARCHAR(255),
    comments TEXT,
    isactive BOOLEAN,
    leadId VARCHAR(255),
    salesRepresentativeId INTEGER,
    contactName VARCHAR(255),
    contactMobileNo VARCHAR(255),
    products JSON
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        o.id,
        o.user_created,
        o.date_created,
        o.user_updated,
        o.date_updated,
        o.status,
        o.expected_completion,
        o.opportunity_type,
        o.opportunity_for,
        o.customer_id,
        o.customer_name,
        o.customer_type,
        o.opportunity_name,
        o.opportunity_id,
        o.comments,
        o.isactive,
        o.lead_id,
        o.sales_representative_id,
        o.contact_name,
        o.contact_mobile_no,
        COALESCE(
            (
                SELECT json_agg(p)
                FROM (
                    SELECT 
                        sp.id AS "Id",
                        COALESCE(sp.user_created, 0) as "UserCreated",
                        sp.date_created as "DateCreated",
                        COALESCE(sp.user_updated, 0) as "UserUpdated",
                        sp.date_updated as "DateUpdated",
                        COALESCE(m.id, 0) as "MakeId",
                        COALESCE(m.name, '') as "MakeName",
                        COALESCE(md.id, 0) as "ModelId",
                        COALESCE(md.name, '') as "ModelName",
                        COALESCE(p.id, 0) as "ProductId",
                        COALESCE(p.name, '') as "ProductName",
                        COALESCE(c.id, 0) as "CategoryId",
                        COALESCE(c.name, '') as "CategoryName",
                        COALESCE(i.item_code, '') as "ItemCode",
                        COALESCE(i.item_name, '') as "ItemName",
                        COALESCE(sp.qty, 0) as "Quantity",
                        CAST(COALESCE(sp.amount, 0) AS NUMERIC(18,2)) as "Amount",
                        COALESCE(sp.stage, '') as "Stage",
                        CAST(sp.stage_item_id AS BIGINT) as "StageItemId",
                        sp.inventory_items_id as "InventoryItemsId",
                        sp.isactive as "IsActive"
                    FROM sales_products sp
                    LEFT JOIN inventory_items i ON sp.inventory_items_id = i.id
                    LEFT JOIN makes m ON i.make_id = m.id
                    LEFT JOIN models md ON i.model_id = md.id
                    LEFT JOIN products p ON i.product_id = p.id
                    LEFT JOIN categories c ON i.category_id = c.id
                    WHERE sp.stage_item_id = o.id
                    AND sp.isactive = true
                    AND upper(sp.stage) = 'OPPORTUNITY'
                    ORDER BY sp.id
                ) p
            ),
            '[]'::json
        ) as products
    FROM sales_opportunities o
    WHERE o.id = p_id AND o.isactive = true;
END;
$$;

-- Function to get opportunity by ID with exact response format
CREATE OR REPLACE FUNCTION get_opportunity_by_id_v2(p_id INTEGER)
RETURNS TABLE (
    id INTEGER,
    userCreated INTEGER,
    dateCreated TIMESTAMP,
    userUpdated INTEGER,
    dateUpdated TIMESTAMP,
    status VARCHAR(255),
    expectedCompletion DATE,
    opportunityType VARCHAR(255),
    opportunityFor VARCHAR(255),
    customerId VARCHAR(255),
    customerName VARCHAR(255),
    customerType VARCHAR(255),
    opportunityName VARCHAR(255),
    opportunityId VARCHAR(255),
    comments TEXT,
    isActive BOOLEAN,
    leadId VARCHAR(255),
    salesRepresentativeId INTEGER,
    contactName VARCHAR(255),
    contactMobileNo VARCHAR(255),
    products JSON
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COALESCE(o.id, 0) as id,
        COALESCE(o.user_created, 0) as userCreated,
        o.date_created as dateCreated,
        COALESCE(o.user_updated, 0) as userUpdated,
        o.date_updated as dateUpdated,
        COALESCE(o.status, 'string') as status,
        o.expected_completion as expectedCompletion,
        COALESCE(o.opportunity_type, 'string') as opportunityType,
        COALESCE(o.opportunity_for, 'string') as opportunityFor,
        COALESCE(o.customer_id, 'string') as customerId,
        COALESCE(o.customer_name, 'string') as customerName,
        COALESCE(o.customer_type, 'string') as customerType,
        COALESCE(o.opportunity_name, 'string') as opportunityName,
        COALESCE(o.opportunity_id, 'string') as opportunityId,
        COALESCE(o.comments, 'string') as comments,
        COALESCE(o.isactive, true) as isActive,
        COALESCE(o.lead_id, 'string') as leadId,
        COALESCE(o.sales_representative_id, 0) as salesRepresentativeId,
        COALESCE(o.contact_name, 'string') as contactName,
        COALESCE(o.contact_mobile_no, 'string') as contactMobileNo,        COALESCE(
            (
                SELECT json_agg(json_build_object(
                    'Id', COALESCE(sp.id, 0),
                    'UserCreated', COALESCE(sp.user_created, 0),
                    'DateCreated', sp.date_created,
                    'UserUpdated', COALESCE(sp.user_updated, 0),
                    'DateUpdated', sp.date_updated,
                    'MakeId', COALESCE(m.id, 0),
                    'MakeName', COALESCE(m.name, ''),
                    'ModelId', COALESCE(md.id, 0),
                    'ModelName', COALESCE(md.name, ''),
                    'ProductId', COALESCE(p.id, 0),
                    'ProductName', COALESCE(p.name, ''),
                    'CategoryId', COALESCE(c.id, 0),
                    'CategoryName', COALESCE(c.name, ''),
                    'ItemCode', COALESCE(i.item_code, ''),
                    'ItemName', COALESCE(i.item_name, ''),
                    'Quantity', COALESCE(sp.qty, 0),
                    'Amount', CAST(COALESCE(sp.amount, 0) AS NUMERIC(18,2)),
                    'Stage', COALESCE(sp.stage, ''),
                    'StageItemId', CAST(sp.stage_item_id AS BIGINT),
                    'InventoryItemsId', sp.inventory_items_id,
                    'IsActive', COALESCE(sp.isactive, true)
                ))
                FROM sales_products sp
                LEFT JOIN inventory_items i ON sp.inventory_items_id = i.id
                LEFT JOIN makes m ON i.make_id = m.id
                LEFT JOIN models md ON i.model_id = md.id
                LEFT JOIN products p ON i.product_id = p.id
                LEFT JOIN categories c ON i.category_id = c.id
                WHERE sp.stage_item_id = o.id
                AND sp.isactive = true
                AND upper(sp.stage) = 'OPPORTUNITY'
                ORDER BY sp.id
            ),
            '[]'::json
        ) as products
    FROM sales_opportunities o
    WHERE o.id = p_id AND o.isactive = true;
END;
$$;

-- Function to get opportunities by lead ID
CREATE OR REPLACE FUNCTION get_opportunities_by_lead_id(p_lead_id VARCHAR(255))
RETURNS TABLE (
    id INTEGER,
    userCreated INTEGER,
    dateCreated TIMESTAMP,
    userUpdated INTEGER,
    dateUpdated TIMESTAMP,
    status VARCHAR(255),
    expectedCompletion DATE,
    opportunityType VARCHAR(255),
    opportunityFor VARCHAR(255),
    customerId VARCHAR(255),
    customerName VARCHAR(255),
    customerType VARCHAR(255),
    opportunityName VARCHAR(255),
    opportunityId VARCHAR(255),
    comments TEXT,
    isactive BOOLEAN,
    leadId VARCHAR(255),
    salesRepresentativeId INTEGER,
    contactName VARCHAR(255),
    contactMobileNo VARCHAR(255),
    products JSON
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        o.id,
        o.user_created,
        o.date_created,
        o.user_updated,
        o.date_updated,
        o.status,
        o.expected_completion,
        o.opportunity_type,
        o.opportunity_for,
        o.customer_id,
        o.customer_name,
        o.customer_type,
        o.opportunity_name,
        o.opportunity_id,
        o.comments,
        o.isactive,
        o.lead_id,
        o.sales_representative_id,
        o.contact_name,
        o.contact_mobile_no,
        COALESCE(
            (
                SELECT json_agg(p)
                FROM (
                    SELECT 
                        sp.id AS "Id",
                        COALESCE(sp.user_created, 0) as "UserCreated",
                        sp.date_created as "DateCreated",
                        COALESCE(sp.user_updated, 0) as "UserUpdated",
                        sp.date_updated as "DateUpdated",
                        COALESCE(m.id, 0) as "MakeId",
                        COALESCE(m.name, '') as "MakeName",
                        COALESCE(md.id, 0) as "ModelId",
                        COALESCE(md.name, '') as "ModelName",
                        COALESCE(p.id, 0) as "ProductId",
                        COALESCE(p.name, '') as "ProductName",
                        COALESCE(c.id, 0) as "CategoryId",
                        COALESCE(c.name, '') as "CategoryName",
                        COALESCE(i.item_code, '') as "ItemCode",
                        COALESCE(i.item_name, '') as "ItemName",
                        COALESCE(sp.qty, 0) as "Quantity",
                        CAST(COALESCE(sp.amount, 0) AS NUMERIC(18,2)) as "Amount",
                        COALESCE(sp.stage, '') as "Stage",
                        CAST(sp.stage_item_id AS BIGINT) as "StageItemId",
                        sp.inventory_items_id as "InventoryItemsId",
                        sp.isactive as "IsActive"
                    FROM sales_products sp
                    LEFT JOIN inventory_items i ON sp.inventory_items_id = i.id
                    LEFT JOIN makes m ON i.make_id = m.id
                    LEFT JOIN models md ON i.model_id = md.id
                    LEFT JOIN products p ON i.product_id = p.id
                    LEFT JOIN categories c ON i.category_id = c.id
                    WHERE sp.stage_item_id = o.id
                    AND sp.isactive = true
                    AND upper(sp.stage) = 'OPPORTUNITY'
                    ORDER BY sp.id
                ) p
            ),
            '[]'::json
        ) as products
    FROM sales_opportunities o
    WHERE o.lead_id = p_lead_id
    AND o.isactive = true
    ORDER BY o.date_created DESC;
END;
$$;

-- Procedure to create a new opportunity
CREATE OR REPLACE PROCEDURE create_opportunity(
    IN p_user_created INTEGER,
    IN p_status VARCHAR(255),
    IN p_expected_completion DATE,
    IN p_opportunity_type VARCHAR(255),
    IN p_opportunity_for VARCHAR(255),
    IN p_customer_id VARCHAR(255),
    IN p_customer_name VARCHAR(255),
    IN p_customer_type VARCHAR(255),
    IN p_opportunity_name VARCHAR(255),
    IN p_opportunity_id VARCHAR(255),
    IN p_comments TEXT,
    IN p_lead_id VARCHAR(255),
    IN p_sales_representative_id INTEGER,
    IN p_contact_name VARCHAR(255),
    IN p_contact_mobile_no VARCHAR(255),
    OUT new_id INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO sales_opportunities(
        user_created,
        date_created,
        status,
        expected_completion,
        opportunity_type,
        opportunity_for,
        customer_id,
        customer_name,
        customer_type,
        opportunity_name,
        opportunity_id,
        comments,
        isactive,
        lead_id,
        sales_representative_id,
        contact_name,
        contact_mobile_no
    ) VALUES (
        p_user_created,
        CURRENT_TIMESTAMP,
        p_status,
        p_expected_completion,
        p_opportunity_type,
        p_opportunity_for,
        p_customer_id,
        p_customer_name,
        p_customer_type,
        p_opportunity_name,
        p_opportunity_id,
        p_comments,
        true,
        p_lead_id,
        p_sales_representative_id,
        p_contact_name,
        p_contact_mobile_no
    ) RETURNING id INTO new_id;
END;
$$;

-- Procedure to update an opportunity
CREATE OR REPLACE PROCEDURE update_opportunity(
    IN p_id INTEGER,
    IN p_user_updated INTEGER,
    IN p_status VARCHAR(255),
    IN p_expected_completion DATE,
    IN p_opportunity_type VARCHAR(255),
    IN p_opportunity_for VARCHAR(255),
    IN p_customer_id VARCHAR(255),
    IN p_customer_name VARCHAR(255),
    IN p_customer_type VARCHAR(255),
    IN p_opportunity_name VARCHAR(255),
    IN p_opportunity_id VARCHAR(255),
    IN p_comments TEXT,
    IN p_lead_id VARCHAR(255),
    IN p_sales_representative_id INTEGER,
    IN p_contact_name VARCHAR(255),
    IN p_contact_mobile_no VARCHAR(255)
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE sales_opportunities
    SET
        user_updated = p_user_updated,
        date_updated = CURRENT_TIMESTAMP,
        status = p_status,
        expected_completion = p_expected_completion,
        opportunity_type = p_opportunity_type,
        opportunity_for = p_opportunity_for,
        customer_id = p_customer_id,
        customer_name = p_customer_name,
        customer_type = p_customer_type,
        opportunity_name = p_opportunity_name,
        opportunity_id = p_opportunity_id,
        comments = p_comments,
        lead_id = p_lead_id,
        sales_representative_id = p_sales_representative_id,
        contact_name = p_contact_name,
        contact_mobile_no = p_contact_mobile_no
    WHERE id = p_id AND isactive = true;
END;
$$;

-- Procedure to soft delete an opportunity
CREATE OR REPLACE PROCEDURE delete_opportunity(
    IN p_id INTEGER,
    IN p_user_updated INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE sales_opportunities
    SET
        isactive = false,
        user_updated = p_user_updated,
        date_updated = CURRENT_TIMESTAMP
    WHERE id = p_id AND isactive = true;
END;
$$;

-- First drop the existing function
DROP FUNCTION IF EXISTS get_opportunities_by_lead_id(VARCHAR);

-- Function to get sales demos by id
CREATE OR REPLACE FUNCTION public.fn_get_sales_demo_by_id(
    IN p_id INTEGER
)
RETURNS TABLE (
    id INTEGER,
    user_created INTEGER,
    date_created TIMESTAMP,
    user_updated INTEGER,
    date_updated TIMESTAMP,
    user_id INTEGER,
    demo_date TIMESTAMP,
    status VARCHAR(100),
    address_id INTEGER,
    opportunity_id INTEGER,
    customer_id INTEGER,
    demo_contact VARCHAR(255),
    customer_name VARCHAR(255),
    demo_name VARCHAR(255),
    demo_approach VARCHAR(255),
    demo_outcome VARCHAR(255),
    demo_feedback VARCHAR(255),
    comments VARCHAR(255),
    presenter_id INTEGER,
    demo_products JSON
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        sd.id,
        sd.user_created,
        sd.date_created,
        sd.user_updated,
        sd.date_updated,
        sd.user_id,
        sd.demo_date,
        sd.status,
        sd.address_id,
        sd.opportunity_id,
        sd.customer_id,
        sd.demo_contact,
        sd.customer_name,
        sd.demo_name,
        sd.demo_approach,
        sd.demo_outcome,
        sd.demo_feedback,
        sd.comments,
        sd.presenter_id,
        COALESCE(
            (
                SELECT json_agg(json_build_object(
                    'Id', COALESCE(sp.id, 0),
                    'UserCreated', COALESCE(sp.user_created, 0),
                    'DateCreated', sp.date_created,
                    'UserUpdated', COALESCE(sp.user_updated, 0),
                    'DateUpdated', sp.date_updated,
                    'MakeId', COALESCE(m.id, 0),
                    'MakeName', COALESCE(m.name, ''),
                    'ModelId', COALESCE(md.id, 0),
                    'ModelName', COALESCE(md.name, ''),
                    'ProductId', COALESCE(p.id, 0),
                    'ProductName', COALESCE(p.name, ''),
                    'CategoryId', COALESCE(c.id, 0),
                    'CategoryName', COALESCE(c.name, ''),
                    'ItemCode', COALESCE(i.item_code, ''),
                    'ItemName', COALESCE(i.item_name, ''),
                    'Quantity', COALESCE(sp.qty, 0),
                    'Amount', CAST(COALESCE(sp.amount, 0) AS NUMERIC(18,2)),
                    'Stage', COALESCE(sp.stage, ''),
                    'StageItemId', CAST(sp.stage_item_id AS BIGINT),
                    'InventoryItemsId', sp.inventory_items_id,
                    'IsActive', COALESCE(sp.isactive, true)
                ))
                FROM sales_products sp
                LEFT JOIN inventory_items i ON sp.inventory_items_id = i.id
                LEFT JOIN makes m ON i.make_id = m.id
                LEFT JOIN models md ON i.model_id = md.id
                LEFT JOIN products p ON i.product_id = p.id
                LEFT JOIN categories c ON i.category_id = c.id
                WHERE sp.stage_item_id::VARCHAR = sd.id::VARCHAR
                AND sp.isactive = true
                AND upper(sp.stage) = 'DEMO'
                AND (i.isactive IS NULL OR i.isactive = true)
                ORDER BY sp.id
            ),
            '[]'::json
        ) as demo_products
    FROM public.sales_demos sd
    WHERE sd.id = p_id;
END;
$$ LANGUAGE plpgsql;

-- Drop any existing versions of the function
DROP FUNCTION IF EXISTS public.fn_get_sales_demos(VARCHAR, VARCHAR[], VARCHAR[], VARCHAR[], INTEGER, INTEGER, TEXT, TEXT);

-- Function to get sales demos with pagination and filtering
CREATE OR REPLACE FUNCTION public.fn_get_sales_demos(
    p_search_text VARCHAR DEFAULT NULL,
    p_customer_names VARCHAR[] DEFAULT NULL,
    p_demo_types VARCHAR[] DEFAULT NULL,
    p_statuses VARCHAR[] DEFAULT NULL,
    p_page_number INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 10,
    p_order_by TEXT DEFAULT 'date_created'::TEXT,
    p_order_direction TEXT DEFAULT 'DESC'::TEXT
)
RETURNS TABLE (
    "TotalRecords" INTEGER,
    "Id" INTEGER,
    "CustomerName" VARCHAR,
    "DemoName" VARCHAR,
    "DemoType" VARCHAR,
    "Status" VARCHAR,
    "DemoDateTime" TIMESTAMP,
    "DemoContact" VARCHAR,
    "DemoApproach" VARCHAR,
    "DemoOutcome" VARCHAR,
    "DemoFeedback" VARCHAR,
    "Comments" VARCHAR,
    "OpportunityId" INTEGER,
    "PresenterId" INTEGER,
    "PresenterName" VARCHAR,
    "DateCreated" TIMESTAMP,
    "DateUpdated" TIMESTAMP,
    "AddressId" INTEGER,
    "CustomerId" INTEGER,
    "UserId" INTEGER,
    "Products" JSON
) 
LANGUAGE plpgsql
AS $$
DECLARE
    v_where_clause TEXT := ' WHERE 1=1';
    v_total_records INTEGER;
    v_offset INTEGER;
    v_query TEXT;
    v_valid_page_size INTEGER;
BEGIN
    -- Validate and set page size limit
    v_valid_page_size := LEAST(COALESCE(NULLIF(p_page_size, 0), 10), 1000);
    
    -- Calculate offset for pagination
    v_offset := (p_page_number - 1) * v_valid_page_size;
    
    -- Add search filter with case-insensitive matching
    IF p_search_text IS NOT NULL AND p_search_text != '' AND p_search_text != 'string' THEN
        v_where_clause := v_where_clause || ' AND (
            LOWER(sd.customer_name) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sd.demo_name) LIKE ''%'' || LOWER($1) || ''%'' OR 
            LOWER(sd.demo_type) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sd.status) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sd.demo_contact) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sd.demo_approach) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sd.demo_outcome) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sd.demo_feedback) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(sd.comments) LIKE ''%'' || LOWER($1) || ''%'' OR
            LOWER(u.username) LIKE ''%'' || LOWER($1) || ''%''
        )';
    END IF;

    -- Apply array filters
    IF p_customer_names IS NOT NULL AND array_length(p_customer_names, 1) > 0 
        AND NOT (array_length(p_customer_names, 1) = 1 
        AND (p_customer_names[1] IS NULL OR p_customer_names[1] = 'string' OR p_customer_names[1] = '')) THEN
        v_where_clause := v_where_clause || ' AND EXISTS (
            SELECT 1 FROM unnest($2::varchar[]) AS cn 
            WHERE LOWER(sd.customer_name) LIKE ''%'' || LOWER(cn) || ''%''
        )';
    END IF;

    IF p_demo_types IS NOT NULL AND array_length(p_demo_types, 1) > 0 
        AND NOT (array_length(p_demo_types, 1) = 1 
        AND (p_demo_types[1] IS NULL OR p_demo_types[1] = 'string' OR p_demo_types[1] = '')) THEN
        v_where_clause := v_where_clause || ' AND EXISTS (
            SELECT 1 FROM unnest($3::varchar[]) AS dt 
            WHERE LOWER(sd.demo_type) LIKE ''%'' || LOWER(dt) || ''%''
        )';
    END IF;

    IF p_statuses IS NOT NULL AND array_length(p_statuses, 1) > 0 
        AND NOT (array_length(p_statuses, 1) = 1 
        AND (p_statuses[1] IS NULL OR p_statuses[1] = 'string' OR p_statuses[1] = '')) THEN
        v_where_clause := v_where_clause || ' AND EXISTS (
            SELECT 1 FROM unnest($4::varchar[]) AS s 
            WHERE LOWER(sd.status) LIKE ''%'' || LOWER(s) || ''%''
        )';
    END IF;

    -- Default order by if invalid
    IF p_order_by IS NULL OR p_order_by = '' THEN 
        p_order_by := 'date_created';
    END IF;

    -- Validate order direction
    IF p_order_direction IS NULL OR p_order_direction NOT IN ('ASC', 'DESC') THEN
        p_order_direction := 'DESC';
    END IF;

    -- Get total records count
    EXECUTE 'SELECT COUNT(*) FROM public.sales_demos sd 
             LEFT JOIN public.users u ON sd.presenter_id = u.user_id' || v_where_clause
    INTO v_total_records
    USING 
        CASE WHEN p_search_text IS NOT NULL AND p_search_text != '' AND p_search_text != 'string' THEN p_search_text ELSE NULL END,
        CASE WHEN p_customer_names IS NOT NULL AND array_length(p_customer_names, 1) > 0 
             AND NOT (array_length(p_customer_names, 1) = 1 AND (p_customer_names[1] IS NULL OR p_customer_names[1] = 'string' OR p_customer_names[1] = '')) 
             THEN p_customer_names ELSE NULL END,
        CASE WHEN p_demo_types IS NOT NULL AND array_length(p_demo_types, 1) > 0 
             AND NOT (array_length(p_demo_types, 1) = 1 AND (p_demo_types[1] IS NULL OR p_demo_types[1] = 'string' OR p_demo_types[1] = '')) 
             THEN p_demo_types ELSE NULL END,
        CASE WHEN p_statuses IS NOT NULL AND array_length(p_statuses, 1) > 0 
             AND NOT (array_length(p_statuses, 1) = 1 AND (p_statuses[1] IS NULL OR p_statuses[1] = 'string' OR p_statuses[1] = '')) 
             THEN p_statuses ELSE NULL END;
    
    -- Build and execute main query with product data
    RETURN QUERY EXECUTE '
    WITH base_query AS (
        SELECT 
            sd.id,
            sd.customer_name,
            sd.demo_name,
            sd.demo_type,
            sd.status,
            sd.demo_date AS demo_datetime,
            sd.demo_contact,
            sd.demo_approach,
            sd.demo_outcome,
            sd.demo_feedback,
            sd.comments,
            sd.opportunity_id,
            sd.presenter_id,
            u.username AS presenter_name,
            sd.date_created,
            sd.date_updated,
            sd.address_id,
            sd.customer_id,
            sd.user_id,
            ' || v_total_records || ' AS total_records,
            COALESCE(
                (
                    SELECT json_agg(json_build_object(
                        ''Id'', COALESCE(sp.id, 0),
                        ''UserCreated'', COALESCE(sp.user_created, 0),
                        ''DateCreated'', sp.date_created,
                        ''UserUpdated'', COALESCE(sp.user_updated, 0),
                        ''DateUpdated'', sp.date_updated,
                        ''MakeId'', COALESCE(m.id, 0),
                        ''MakeName'', COALESCE(m.name, ''''),
                        ''ModelId'', COALESCE(md.id, 0),
                        ''ModelName'', COALESCE(md.name, ''''),
                        ''ProductId'', COALESCE(p.id, 0),
                        ''ProductName'', COALESCE(p.name, ''''),
                        ''CategoryId'', COALESCE(c.id, 0),
                        ''CategoryName'', COALESCE(c.name, ''''),
                        ''ItemCode'', COALESCE(i.item_code, ''''),
                        ''ItemName'', COALESCE(i.item_name, ''''),
                        ''Quantity'', COALESCE(sp.qty, 0),
                        ''Amount'', CAST(COALESCE(sp.amount, 0) AS NUMERIC(18,2)),
                        ''Stage'', COALESCE(sp.stage, ''''),
                        ''StageItemId'', CAST(sp.stage_item_id AS BIGINT),
                        ''InventoryItemsId'', sp.inventory_items_id,
                        ''IsActive'', COALESCE(sp.isactive, true)
                    ))
                    FROM sales_products sp
                    LEFT JOIN inventory_items i ON sp.inventory_items_id = i.id
                    LEFT JOIN makes m ON i.make_id = m.id
                    LEFT JOIN models md ON i.model_id = md.id
                    LEFT JOIN products p ON i.product_id = p.id
                    LEFT JOIN categories c ON i.category_id = c.id
                    WHERE sp.stage_item_id::VARCHAR = sd.id::VARCHAR
                    AND sp.isactive = true
                    AND upper(sp.stage) = ''DEMO''
                    AND (i.isactive IS NULL OR i.isactive = true)
                    ORDER BY sp.id
                ),
                ''[]''::json
            ) as products
        FROM public.sales_demos sd
        LEFT JOIN public.users u ON sd.presenter_id = u.user_id
        ' || v_where_clause || '
        ORDER BY sd.' || quote_ident(p_order_by) || ' ' || p_order_direction || '
        LIMIT ' || v_valid_page_size || '
        OFFSET ' || v_offset || '
    )
    SELECT 
        total_records AS "TotalRecords",
        id AS "Id",
        customer_name AS "CustomerName",
        demo_name AS "DemoName",
        demo_type AS "DemoType",
        status AS "Status",
        demo_datetime AS "DemoDateTime",
        demo_contact AS "DemoContact",
        demo_approach AS "DemoApproach",
        demo_outcome AS "DemoOutcome",
        demo_feedback AS "DemoFeedback",
        comments AS "Comments",
        opportunity_id AS "OpportunityId",
        presenter_id AS "PresenterId",
        presenter_name AS "PresenterName",
        date_created AS "DateCreated",
        date_updated AS "DateUpdated",
        address_id AS "AddressId",
        customer_id AS "CustomerId",
        user_id AS "UserId",
        products AS "Products"
    FROM base_query'
    USING 
        CASE WHEN p_search_text IS NOT NULL AND p_search_text != '' AND p_search_text != 'string' THEN p_search_text ELSE NULL END,
        CASE WHEN p_customer_names IS NOT NULL AND array_length(p_customer_names, 1) > 0 
             AND NOT (array_length(p_customer_names, 1) = 1 AND (p_customer_names[1] IS NULL OR p_customer_names[1] = 'string' OR p_customer_names[1] = '')) 
             THEN p_customer_names ELSE NULL END,
        CASE WHEN p_demo_types IS NOT NULL AND array_length(p_demo_types, 1) > 0 
             AND NOT (array_length(p_demo_types, 1) = 1 AND (p_demo_types[1] IS NULL OR p_demo_types[1] = 'string' OR p_demo_types[1] = '')) 
             THEN p_demo_types ELSE NULL END,
        CASE WHEN p_statuses IS NOT NULL AND array_length(p_statuses, 1) > 0 
             AND NOT (array_length(p_statuses, 1) = 1 AND (p_statuses[1] IS NULL OR p_statuses[1] = 'string' OR p_statuses[1] = '')) 
             THEN p_statuses ELSE NULL END;
END;
$$;
