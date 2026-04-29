-- Migration to remove products functionality from opportunity
BEGIN;

-- Drop existing functions
DROP FUNCTION IF EXISTS get_active_opportunities;
DROP FUNCTION IF EXISTS get_opportunity_by_id;
DROP FUNCTION IF EXISTS get_opportunities_by_lead_id;

-- Recreate get_active_opportunities without products
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
    contactName VARCHAR(255),
    contactMobileNo VARCHAR(255)
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
        o.contact_mobile_no
    FROM sales_opportunities o
    WHERE o.isactive = true
    ORDER BY o.date_created DESC;
END;
$$;

-- Recreate get_opportunity_by_id without products
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
    contactMobileNo VARCHAR(255)
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
        o.contact_mobile_no
    FROM sales_opportunities o
    WHERE o.id = p_id
    AND o.isactive = true;
END;
$$;

-- Recreate get_opportunities_by_lead_id without products
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
    contactMobileNo VARCHAR(255)
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
        o.contact_mobile_no
    FROM sales_opportunities o
    WHERE o.lead_id = p_lead_id
    AND o.isactive = true
    ORDER BY o.date_created DESC;
END;
$$;

COMMIT;
