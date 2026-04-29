-- Get by ID
CREATE OR REPLACE FUNCTION sp_get_cs_intercompany_account_by_id(p_intercompany_account_id integer)
RETURNS TABLE (
    intercompany_account_id integer,
    relationship_id integer,
    transaction_type varchar(100),
    company1_receivable_account_id integer,
    company2_payable_account_id integer,
    company1_tax_treatment_rule text,
    company2_tax_treatment_rule text,
    created_at timestamptz,
    updated_at timestamptz
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ia.intercompany_account_id,
        ia.relationship_id,
        ia.transaction_type,
        ia.company1_receivable_account_id,
        ia.company2_payable_account_id,
        ia.company1_tax_treatment_rule,
        ia.company2_tax_treatment_rule,
        ia.created_at,
        ia.updated_at
    FROM cs_intercompany_accounts ia
    WHERE ia.intercompany_account_id = p_intercompany_account_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_get_cs_intercompany_accounts_by_relationship(
    p_relationship_id INTEGER,
    p_search_text VARCHAR DEFAULT NULL
)
RETURNS TABLE (
    intercompany_account_id INTEGER,
    relationship_id INTEGER,
    transaction_type VARCHAR,
    gl_account_code VARCHAR,
    description TEXT,
    is_active BOOLEAN,
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ,
    total_records INTEGER,
    filtered_records INTEGER
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_total INTEGER;
BEGIN
    -- Get total records
    SELECT COUNT(*) INTO v_total
    FROM cs_intercompany_accounts
    WHERE relationship_id = p_relationship_id;

    -- Get filtered results and filtered count
    RETURN QUERY
    WITH filtered_data AS (
        SELECT *
        FROM cs_intercompany_accounts
        WHERE relationship_id = p_relationship_id
        AND (
            p_search_text IS NULL
            OR transaction_type ILIKE '%' || p_search_text || '%'
        )
    )
    SELECT
        ia.intercompany_account_id,
        ia.relationship_id,
        ia.transaction_type,
        ia.gl_account_code,
        ia.description,
        ia.is_active,
        ia.created_at,
        ia.updated_at,
        v_total AS total_records,
        COUNT(*) OVER() AS filtered_records
    FROM filtered_data ia
    ORDER BY ia.transaction_type;
END;
$$;


-- Create
CREATE OR REPLACE PROCEDURE sp_create_cs_intercompany_account(
    p_relationship_id integer,
    p_transaction_type varchar(100),
    p_company1_receivable_account_id integer,
    p_company2_payable_account_id integer,
    p_company1_tax_treatment_rule text,
    p_company2_tax_treatment_rule text,
    INOUT p_intercompany_account_id integer
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Check if relationship_id and transaction_type combination already exists
    IF EXISTS (
        SELECT 1 FROM cs_intercompany_accounts 
        WHERE relationship_id = p_relationship_id 
        AND transaction_type = p_transaction_type
    ) THEN
        RAISE EXCEPTION 'An intercompany account with this relationship and transaction type already exists';
    END IF;

    INSERT INTO cs_intercompany_accounts (
        relationship_id,
        transaction_type,
        company1_receivable_account_id,
        company2_payable_account_id,
        company1_tax_treatment_rule,
        company2_tax_treatment_rule
    )
    VALUES (
        p_relationship_id,
        p_transaction_type,
        p_company1_receivable_account_id,
        p_company2_payable_account_id,
        p_company1_tax_treatment_rule,
        p_company2_tax_treatment_rule
    )
    RETURNING intercompany_account_id INTO p_intercompany_account_id;

    -- Update updated_at timestamp
    UPDATE cs_intercompany_accounts
    SET updated_at = CURRENT_TIMESTAMP
    WHERE intercompany_account_id = p_intercompany_account_id;
END;
$$;

-- Update
CREATE OR REPLACE PROCEDURE sp_update_cs_intercompany_account(
    p_intercompany_account_id integer,
    p_relationship_id integer,
    p_transaction_type varchar(100),
    p_company1_receivable_account_id integer,
    p_company2_payable_account_id integer,
    p_company1_tax_treatment_rule text,
    p_company2_tax_treatment_rule text,
    INOUT p_success boolean
)
LANGUAGE plpgsql
AS $$
BEGIN
    p_success := false;
    
    -- Check if the record exists
    IF NOT EXISTS (SELECT 1 FROM cs_intercompany_accounts WHERE intercompany_account_id = p_intercompany_account_id) THEN
        RAISE EXCEPTION 'Intercompany account not found';
    END IF;

    -- Check for duplicate relationship_id and transaction_type combination
    IF EXISTS (
        SELECT 1 
        FROM cs_intercompany_accounts 
        WHERE relationship_id = p_relationship_id 
        AND transaction_type = p_transaction_type
        AND intercompany_account_id != p_intercompany_account_id
    ) THEN
        RAISE EXCEPTION 'An intercompany account with this relationship and transaction type already exists';
    END IF;

    UPDATE cs_intercompany_accounts
    SET 
        relationship_id = p_relationship_id,
        transaction_type = p_transaction_type,
        company1_receivable_account_id = p_company1_receivable_account_id,
        company2_payable_account_id = p_company2_payable_account_id,
        company1_tax_treatment_rule = p_company1_tax_treatment_rule,
        company2_tax_treatment_rule = p_company2_tax_treatment_rule,
        updated_at = CURRENT_TIMESTAMP
    WHERE intercompany_account_id = p_intercompany_account_id;

    p_success := true;
EXCEPTION
    WHEN OTHERS THEN
        p_success := false;
        RAISE;
END;
$$;

-- Delete
CREATE OR REPLACE PROCEDURE sp_delete_cs_intercompany_account(
    p_intercompany_account_id integer,
    INOUT p_success boolean
)
LANGUAGE plpgsql
AS $$
BEGIN
    p_success := false;
    
    -- Check if the record exists
    IF NOT EXISTS (SELECT 1 FROM cs_intercompany_accounts WHERE intercompany_account_id = p_intercompany_account_id) THEN
        RAISE EXCEPTION 'Intercompany account not found';
    END IF;

    DELETE FROM cs_intercompany_accounts
    WHERE intercompany_account_id = p_intercompany_account_id;

    p_success := true;
EXCEPTION
    WHEN OTHERS THEN
        p_success := false;
        RAISE;
END;
$$;
