-- public.financial_statement_templates definition

-- Drop table

-- DROP TABLE public.financial_statement_templates;

CREATE TABLE public.financial_statement_templates (
	template_id serial4 NOT NULL,
	template_code varchar(20) NOT NULL,
	template_name varchar(100) NOT NULL,
	template_type varchar(30) NOT NULL,
	template_description text NULL,
	accounting_standard varchar(20) DEFAULT 'INDIAN_GAAP'::character varying NULL,
	is_default bool DEFAULT false NULL,
	is_active bool DEFAULT true NULL,
	created_by int4 NOT NULL,
	created_date timestamp DEFAULT CURRENT_TIMESTAMP NULL,
	modified_by int4 NULL,
	modified_date timestamp NULL,
	CONSTRAINT financial_statement_templates_accounting_standard_check CHECK (((accounting_standard)::text = ANY ((ARRAY['INDIAN_GAAP'::character varying, 'IFRS'::character varying, 'US_GAAP'::character varying])::text[]))),
	CONSTRAINT financial_statement_templates_pkey PRIMARY KEY (template_id),
	CONSTRAINT financial_statement_templates_template_code_key UNIQUE (template_code),
	CONSTRAINT financial_statement_templates_template_type_check CHECK (((template_type)::text = ANY ((ARRAY['BALANCE_SHEET'::character varying, 'INCOME_STATEMENT'::character varying, 'CASH_FLOW'::character varying])::text[])))
);
CREATE UNIQUE INDEX uq_fin_stmt_template_default ON public.financial_statement_templates USING btree (template_type, accounting_standard) WHERE (is_default = true);


-- DROP FUNCTION public.sp_create_financial_statement_template(varchar, varchar, varchar, text, int4, varchar, bool, bool);

CREATE OR REPLACE FUNCTION public.sp_create_financial_statement_template(p_template_code character varying, p_template_name character varying, p_template_type character varying, p_template_description text, p_created_by integer, p_accounting_standard character varying DEFAULT 'INDIAN_GAAP'::character varying, p_is_default boolean DEFAULT false, p_is_active boolean DEFAULT true)
 RETURNS integer
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_template_id INTEGER;
BEGIN
    -- Enforce uniqueness of default template per type + standard
    IF p_is_default THEN
        IF EXISTS (
            SELECT 1 FROM financial_statement_templates
            WHERE template_type = p_template_type
              AND accounting_standard = p_accounting_standard
              AND is_default = TRUE
        ) THEN
            RAISE EXCEPTION 'A default template already exists for type % and standard %', p_template_type, p_accounting_standard;
        END IF;
    END IF;

    INSERT INTO financial_statement_templates (
        template_code, template_name, template_type, template_description,
        accounting_standard, is_default, is_active, created_by
    )
    VALUES (
        p_template_code, p_template_name, p_template_type, p_template_description,
        p_accounting_standard, p_is_default, p_is_active, p_created_by
    )
    RETURNING template_id INTO v_template_id;

    RETURN v_template_id;
END;
$function$
;

-- DROP FUNCTION public.sp_get_financial_statement_template_by_id(int4);

CREATE OR REPLACE FUNCTION public.sp_get_financial_statement_template_by_id(p_template_id integer)
 RETURNS TABLE(template_id integer, template_code character varying, template_name character varying, template_type character varying, template_description text, accounting_standard character varying, is_default boolean, is_active boolean, created_by integer, created_date timestamp without time zone, modified_by integer, modified_date timestamp without time zone)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT 
        t.template_id,
        t.template_code,
        t.template_name,
        t.template_type,
        t.template_description,
        t.accounting_standard,
        t.is_default,
        t.is_active,
        t.created_by,
        t.created_date,
        t.modified_by,
        t.modified_date
    FROM financial_statement_templates t
    WHERE t.template_id = p_template_id;
END;
$function$
;


-- DROP FUNCTION public.sp_get_all_financial_statement_templates();

CREATE OR REPLACE FUNCTION public.sp_get_all_financial_statement_templates()
 RETURNS TABLE(template_id integer, template_code character varying, template_name character varying, template_type character varying, template_description text, accounting_standard character varying, is_default boolean, is_active boolean, created_by integer, created_date timestamp without time zone, modified_by integer, modified_date timestamp without time zone)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT *
    FROM financial_statement_templates
    ORDER BY template_id;
END;
$function$
;


-- DROP PROCEDURE public.sp_update_financial_statement_template(int4, varchar, text, varchar, varchar, bool, bool, int4);

CREATE OR REPLACE PROCEDURE public.sp_update_financial_statement_template(IN p_template_id integer, IN p_template_name character varying, IN p_template_description text, IN p_accounting_standard character varying, IN p_template_type character varying, IN p_is_default boolean, IN p_is_active boolean, IN p_modified_by integer)
 LANGUAGE plpgsql
AS $procedure$
BEGIN
    -- Ensure only one default template per type + standard
    IF p_is_default THEN
        IF EXISTS (
            SELECT 1 FROM financial_statement_templates
            WHERE template_type = p_template_type
              AND accounting_standard = p_accounting_standard
              AND is_default = TRUE
              AND template_id != p_template_id
        ) THEN
            RAISE EXCEPTION 'A default template already exists for type % and standard %', p_template_type, p_accounting_standard;
        END IF;
    END IF;

    UPDATE financial_statement_templates
    SET
        template_name = p_template_name,
        template_description = p_template_description,
        accounting_standard = p_accounting_standard,
        template_type = p_template_type,
        is_default = p_is_default,
        is_active = p_is_active,
        modified_by = p_modified_by,
        modified_date = CURRENT_TIMESTAMP
    WHERE template_id = p_template_id;
END;
$procedure$
;

-- DROP PROCEDURE public.sp_delete_financial_statement_template(int4);

CREATE OR REPLACE PROCEDURE public.sp_delete_financial_statement_template(IN p_template_id integer)
 LANGUAGE plpgsql
AS $procedure$
BEGIN
    DELETE FROM financial_statement_templates
    WHERE template_id = p_template_id;
END;
$procedure$
;