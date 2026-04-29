-- =============================================================================
-- JOURNAL ENTRY TEMPLATES MODULE (FS-ACC-017) - ENHANCED DESIGN
-- =============================================================================

-- Master table: journal_entry_templates
CREATE TABLE journal_entry_templates (
    template_id SERIAL PRIMARY KEY,
    company_id INTEGER NOT NULL,
    template_code VARCHAR(20) NOT NULL,
    template_name VARCHAR(100) NOT NULL,
    template_description TEXT,
    template_category_id INTEGER REFERENCES journal_template_categories(category_id),
    frequency VARCHAR(20) CHECK (frequency IN ('MONTHLY', 'QUARTERLY', 'YEARLY', 'ADHOC')),
    is_active BOOLEAN DEFAULT TRUE,
    auto_reverse BOOLEAN DEFAULT FALSE,
    auto_reverse_days INTEGER CHECK (auto_reverse IS FALSE OR auto_reverse_days > 0),
    approval_required BOOLEAN DEFAULT FALSE,
    approval_workflow_id INTEGER,
    auto_generate BOOLEAN DEFAULT FALSE,
    next_generation_date DATE CHECK (auto_generate IS FALSE OR next_generation_date > CURRENT_DATE),
    last_generated_date DATE,
    generation_count INTEGER DEFAULT 0,
    tags TEXT[],
    created_by INTEGER NOT NULL,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    modified_by INTEGER,
    modified_date TIMESTAMP,
    UNIQUE (company_id, template_code)
);

-- Stored Procedures for CRUD Operations on journal_entry_templates

-- Create
CREATE OR REPLACE FUNCTION sp_create_journal_entry_template(
    p_company_id INT,
    p_template_code VARCHAR,
    p_template_name VARCHAR,
    p_template_description TEXT,
    p_template_category_id INT,
    p_frequency VARCHAR,
    p_is_active BOOLEAN,
    p_auto_reverse BOOLEAN,
    p_auto_reverse_days INT,
    p_approval_required BOOLEAN,
    p_approval_workflow_id INT,
    p_auto_generate BOOLEAN,
    p_next_generation_date DATE,
    p_last_generated_date DATE,
    p_generation_count INT,
    p_tags TEXT[],
    p_created_by INT
)
RETURNS INT AS $$
DECLARE
    v_template_id INT;
BEGIN
    INSERT INTO journal_entry_templates (
        company_id, template_code, template_name, template_description, template_category_id, frequency,
        is_active, auto_reverse, auto_reverse_days, approval_required, approval_workflow_id,
        auto_generate, next_generation_date, last_generated_date, generation_count,
        tags, created_by
    )
    VALUES (
        p_company_id, p_template_code, p_template_name, p_template_description, p_template_category_id, p_frequency,
        COALESCE(p_is_active, TRUE), COALESCE(p_auto_reverse, FALSE), p_auto_reverse_days,
        COALESCE(p_approval_required, FALSE), p_approval_workflow_id,
        COALESCE(p_auto_generate, FALSE), p_next_generation_date, p_last_generated_date,
        COALESCE(p_generation_count, 0), p_tags, p_created_by
    )
    RETURNING template_id INTO v_template_id;

    RETURN v_template_id;
END;
$$ LANGUAGE plpgsql;

-- Read (Get by ID)
CREATE OR REPLACE FUNCTION sp_get_journal_entry_template_by_id(
    p_template_id INT
)
RETURNS TABLE (
    template_id INT,
    company_id INT,
    template_code VARCHAR,
    template_name VARCHAR,
    template_description TEXT,
    template_category_id INT,
    frequency VARCHAR,
    is_active BOOLEAN,
    auto_reverse BOOLEAN,
    auto_reverse_days INT,
    approval_required BOOLEAN,
    approval_workflow_id INT,
    auto_generate BOOLEAN,
    next_generation_date DATE,
    last_generated_date DATE,
    generation_count INT,
    tags TEXT[],
    created_by INT,
    created_date TIMESTAMP,
    modified_by INT,
    modified_date TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM journal_entry_templates
    WHERE template_id = p_template_id;
END;
$$ LANGUAGE plpgsql;

-- Read (Get all)
CREATE OR REPLACE FUNCTION sp_get_all_journal_entry_templates()
RETURNS TABLE (
    template_id INT,
    company_id INT,
    template_code VARCHAR,
    template_name VARCHAR,
    template_description TEXT,
    template_category_id INT,
    frequency VARCHAR,
    is_active BOOLEAN,
    auto_reverse BOOLEAN,
    auto_reverse_days INT,
    approval_required BOOLEAN,
    approval_workflow_id INT,
    auto_generate BOOLEAN,
    next_generation_date DATE,
    last_generated_date DATE,
    generation_count INT,
    tags TEXT[],
    created_by INT,
    created_date TIMESTAMP,
    modified_by INT,
    modified_date TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM journal_entry_templates;
END;
$$ LANGUAGE plpgsql;

-- Update
CREATE OR REPLACE PROCEDURE sp_update_journal_entry_template(
    p_template_id INT,
    p_template_name VARCHAR,
    p_template_description TEXT,
    p_template_category_id INT,
    p_frequency VARCHAR,
    p_is_active BOOLEAN,
    p_auto_reverse BOOLEAN,
    p_auto_reverse_days INT,
    p_approval_required BOOLEAN,
    p_approval_workflow_id INT,
    p_auto_generate BOOLEAN,
    p_next_generation_date DATE,
    p_last_generated_date DATE,
    p_generation_count INT,
    p_tags TEXT[],
    p_modified_by INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE journal_entry_templates
    SET
        template_name = p_template_name,
        template_description = p_template_description,
        template_category_id = p_template_category_id,
        frequency = p_frequency,
        is_active = p_is_active,
        auto_reverse = p_auto_reverse,
        auto_reverse_days = p_auto_reverse_days,
        approval_required = p_approval_required,
        approval_workflow_id = p_approval_workflow_id,
        auto_generate = p_auto_generate,
        next_generation_date = p_next_generation_date,
        last_generated_date = p_last_generated_date,
        generation_count = p_generation_count,
        tags = p_tags,
        modified_by = p_modified_by,
        modified_date = CURRENT_TIMESTAMP
    WHERE template_id = p_template_id;
END;
$$;

-- Delete
CREATE OR REPLACE PROCEDURE sp_delete_journal_entry_template(
    p_template_id INT
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM journal_entry_templates
    WHERE template_id = p_template_id;
END;
$$;



