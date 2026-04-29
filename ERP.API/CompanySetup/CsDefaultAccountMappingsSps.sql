-- Drop existing procedures if they exist
DROP PROCEDURE IF EXISTS sp_create_cs_default_account_mapping;
DROP PROCEDURE IF EXISTS sp_update_cs_default_account_mapping;
DROP PROCEDURE IF EXISTS sp_delete_cs_default_account_mapping;
DROP PROCEDURE IF EXISTS sp_get_cs_default_account_mapping_by_id;
DROP PROCEDURE IF EXISTS sp_get_cs_default_account_mappings_by_company;

-- Create a new default account mapping
CREATE OR REPLACE PROCEDURE sp_create_cs_default_account_mapping(
    p_company_id INT,
    p_transaction_type VARCHAR(100),
    p_default_debit_account_id INT,
    p_default_credit_account_id INT,
    INOUT p_mapping_id INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Check if mapping already exists for company and transaction type
    IF EXISTS (
        SELECT 1 
        FROM cs_default_account_mappings 
        WHERE company_id = p_company_id 
        AND transaction_type = p_transaction_type
    ) THEN
        RAISE EXCEPTION 'Mapping already exists for this company and transaction type';
    END IF;

    -- Insert new mapping
    INSERT INTO cs_default_account_mappings (
        company_id,
        transaction_type,
        default_debit_account_id,
        default_credit_account_id
    )
    VALUES (
        p_company_id,
        p_transaction_type,
        p_default_debit_account_id,
        p_default_credit_account_id
    )
    RETURNING mapping_id INTO p_mapping_id;
END;
$$;

-- Update an existing default account mapping
CREATE OR REPLACE PROCEDURE sp_update_cs_default_account_mapping(
    p_mapping_id INT,
    p_company_id INT,
    p_transaction_type VARCHAR(100),
    p_default_debit_account_id INT,
    p_default_credit_account_id INT,
    INOUT p_success BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    p_success := FALSE;
    
    -- Check if mapping exists with different ID for same company and transaction type
    IF EXISTS (
        SELECT 1 
        FROM cs_default_account_mappings 
        WHERE company_id = p_company_id 
        AND transaction_type = p_transaction_type
        AND mapping_id != p_mapping_id
    ) THEN
        RAISE EXCEPTION 'Another mapping already exists for this company and transaction type';
    END IF;

    -- Update mapping
    UPDATE cs_default_account_mappings
    SET company_id = p_company_id,
        transaction_type = p_transaction_type,
        default_debit_account_id = p_default_debit_account_id,
        default_credit_account_id = p_default_credit_account_id,
        updated_at = CURRENT_TIMESTAMP
    WHERE mapping_id = p_mapping_id;

    IF FOUND THEN
        p_success := TRUE;
    END IF;
END;
$$;

-- Delete a default account mapping
CREATE OR REPLACE PROCEDURE sp_delete_cs_default_account_mapping(
    p_mapping_id INT,
    INOUT p_success BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    p_success := FALSE;
    
    DELETE FROM cs_default_account_mappings
    WHERE mapping_id = p_mapping_id;
    
    IF FOUND THEN
        p_success := TRUE;
    END IF;
END;
$$;

-- Get a default account mapping by ID
CREATE OR REPLACE FUNCTION sp_get_cs_default_account_mapping_by_id(
    p_mapping_id INT
)
RETURNS TABLE (
    mapping_id INT,
    company_id INT,
    transaction_type VARCHAR(100),
    default_debit_account_id INT,
    default_credit_account_id INT,
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        dam.mapping_id,
        dam.company_id,
        dam.transaction_type,
        dam.default_debit_account_id,
        dam.default_credit_account_id,
        dam.created_at,
        dam.updated_at
    FROM cs_default_account_mappings dam
    WHERE dam.mapping_id = p_mapping_id;
END;
$$;

-- Get all default account mappings for a company
CREATE OR REPLACE FUNCTION sp_get_cs_default_account_mappings_by_company(
    p_company_id INT,
    p_search_text VARCHAR = NULL
)
RETURNS TABLE (
    mapping_id INT,
    company_id INT,
    transaction_type VARCHAR(100),
    default_debit_account_id INT,
    default_credit_account_id INT,
    debit_account_name VARCHAR(255),
    credit_account_name VARCHAR(255),
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        dam.mapping_id,
        dam.company_id,
        dam.transaction_type,
        dam.default_debit_account_id,
        dam.default_credit_account_id,
        debit_coa.account_name as debit_account_name,
        credit_coa.account_name as credit_account_name,
        dam.created_at,
        dam.updated_at
    FROM cs_default_account_mappings dam
    LEFT JOIN cs_chart_of_accounts debit_coa ON dam.default_debit_account_id = debit_coa.account_id
    LEFT JOIN cs_chart_of_accounts credit_coa ON dam.default_credit_account_id = credit_coa.account_id
    WHERE dam.company_id = p_company_id
    AND (
        p_search_text IS NULL
        OR dam.transaction_type ILIKE '%' || p_search_text || '%'
        OR debit_coa.account_name ILIKE '%' || p_search_text || '%'
        OR credit_coa.account_name ILIKE '%' || p_search_text || '%'
    )
    ORDER BY dam.transaction_type;
END;
$$;
