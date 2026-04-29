-- public.currency_exchange_rates definition

-- Drop table

-- DROP TABLE public.currency_exchange_rates;

CREATE TABLE public.currency_exchange_rates (
	exchange_rate_id serial4 NOT NULL,
	company_id int4 NOT NULL,
	from_currency_id int4 NOT NULL,
	to_currency_id int4 NOT NULL,
	rate_date date NOT NULL,
	exchange_rate numeric(15, 8) NOT NULL,
	rate_type varchar(20) DEFAULT 'SPOT'::character varying NULL,
	rate_source varchar(50) NULL,
	is_active bool DEFAULT true NULL,
	effective_from_date date NOT NULL,
	effective_to_date date NULL,
	created_by int4 NOT NULL,
	created_date timestamp DEFAULT CURRENT_TIMESTAMP NULL,
	modified_by int4 NULL,
	modified_date timestamp NULL,
	CONSTRAINT currency_exchange_rates_check CHECK ((from_currency_id <> to_currency_id)),
	CONSTRAINT currency_exchange_rates_check1 CHECK (((effective_to_date IS NULL) OR (effective_to_date >= effective_from_date))),
	CONSTRAINT currency_exchange_rates_exchange_rate_check CHECK ((exchange_rate > (0)::numeric)),
	CONSTRAINT currency_exchange_rates_pkey PRIMARY KEY (exchange_rate_id)
);
CREATE INDEX idx_currency_exchange_rates_active_date ON public.currency_exchange_rates USING btree (from_currency_id, to_currency_id, rate_date, is_active);
CREATE INDEX idx_currency_exchange_rates_company ON public.currency_exchange_rates USING btree (company_id);


-- public.currency_exchange_rates foreign keys

ALTER TABLE public.currency_exchange_rates ADD CONSTRAINT currency_exchange_rates_from_currency_id_fkey FOREIGN KEY (from_currency_id) REFERENCES public.currencies(currency_id);
ALTER TABLE public.currency_exchange_rates ADD CONSTRAINT currency_exchange_rates_to_currency_id_fkey FOREIGN KEY (to_currency_id) REFERENCES public.currencies(currency_id);


-- DROP FUNCTION public.sp_insert_currency_exchange_rate(int4, int4, int4, date, numeric, date, int4, varchar, varchar, date);

CREATE OR REPLACE FUNCTION public.sp_insert_currency_exchange_rate(p_company_id integer, p_from_currency_id integer, p_to_currency_id integer, p_rate_date date, p_exchange_rate numeric, p_effective_from_date date, p_created_by integer, p_rate_type character varying DEFAULT 'SPOT'::character varying, p_rate_source character varying DEFAULT 'MANUAL'::character varying, p_effective_to_date date DEFAULT NULL::date)
 RETURNS void
 LANGUAGE plpgsql
AS $function$
BEGIN
    -- Insert original rate
    INSERT INTO currency_exchange_rates (
        company_id, from_currency_id, to_currency_id, rate_date,
        exchange_rate, rate_type, rate_source,
        effective_from_date, effective_to_date,
        created_by
    ) VALUES (
        p_company_id, p_from_currency_id, p_to_currency_id, p_rate_date,
        p_exchange_rate, p_rate_type, p_rate_source,
        p_effective_from_date, p_effective_to_date,
        p_created_by
    );

    -- Insert reverse rate if not already present
    IF NOT EXISTS (
        SELECT 1 FROM currency_exchange_rates
        WHERE company_id = p_company_id
          AND from_currency_id = p_to_currency_id
          AND to_currency_id = p_from_currency_id
          AND rate_date = p_rate_date
    ) THEN
        INSERT INTO currency_exchange_rates (
            company_id, from_currency_id, to_currency_id, rate_date,
            exchange_rate, rate_type, rate_source,
            effective_from_date, effective_to_date,
            created_by
        ) VALUES (
            p_company_id, p_to_currency_id, p_from_currency_id, p_rate_date,
            ROUND(1 / p_exchange_rate, 8), p_rate_type, p_rate_source,
            p_effective_from_date, p_effective_to_date,
            p_created_by
        );
    END IF;
END;
$function$
;


-- DROP FUNCTION public.sp_update_currency_exchange_rate(int4, numeric, varchar, varchar, date, date, int4);

CREATE OR REPLACE FUNCTION public.sp_update_currency_exchange_rate(p_exchange_rate_id integer, p_exchange_rate numeric, p_rate_type character varying, p_rate_source character varying, p_effective_from_date date, p_effective_to_date date, p_modified_by integer)
 RETURNS void
 LANGUAGE plpgsql
AS $function$
BEGIN
    UPDATE currency_exchange_rates
    SET exchange_rate = p_exchange_rate,
        rate_type = p_rate_type,
        rate_source = p_rate_source,
        effective_from_date = p_effective_from_date,
        effective_to_date = p_effective_to_date,
        modified_by = p_modified_by,
        modified_date = CURRENT_TIMESTAMP
    WHERE exchange_rate_id = p_exchange_rate_id;
END;
$function$
;


-- DROP FUNCTION public.sp_get_currency_exchange_rate_by_id(int4);

CREATE OR REPLACE FUNCTION public.sp_get_currency_exchange_rate_by_id(p_exchange_rate_id integer)
 RETURNS TABLE(exchange_rate_id integer, company_id integer, from_currency_id integer, to_currency_id integer, rate_date date, exchange_rate numeric, rate_type character varying, rate_source character varying, is_active boolean, effective_from_date date, effective_to_date date, created_by integer, created_date timestamp without time zone, modified_by integer, modified_date timestamp without time zone)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT
        cer.exchange_rate_id,
        cer.company_id,
        cer.from_currency_id,
        cer.to_currency_id,
        cer.rate_date,
        cer.exchange_rate,
        cer.rate_type,
        cer.rate_source,
        cer.is_active,
        cer.effective_from_date,
        cer.effective_to_date,
        cer.created_by,
        cer.created_date,
        cer.modified_by,
        cer.modified_date
    FROM currency_exchange_rates cer
    WHERE cer.exchange_rate_id = p_exchange_rate_id;
END;
$function$
;


-- DROP FUNCTION public.sp_get_currency_exchange_rate_by_companyid(int4);

CREATE OR REPLACE FUNCTION public.sp_get_currency_exchange_rate_by_companyid(p_company_id integer)
 RETURNS TABLE(exchange_rate_id integer, company_id integer, from_currency_id integer, to_currency_id integer, rate_date date, exchange_rate numeric, rate_type character varying, rate_source character varying, is_active boolean, effective_from_date date, effective_to_date date, created_by integer, created_date timestamp without time zone, modified_by integer, modified_date timestamp without time zone)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    SELECT
        cer.exchange_rate_id,
        cer.company_id,
        cer.from_currency_id,
        cer.to_currency_id,
        cer.rate_date,
        cer.exchange_rate,
        cer.rate_type,
        cer.rate_source,
        cer.is_active,
        cer.effective_from_date,
        cer.effective_to_date,
        cer.created_by,
        cer.created_date,
        cer.modified_by,
        cer.modified_date
    FROM currency_exchange_rates cer
    WHERE cer.company_id = p_company_id;
END;
$function$
;


-- DROP FUNCTION public.sp_get_all_currency_exchange_rates(bool);

CREATE OR REPLACE FUNCTION public.sp_get_all_currency_exchange_rates(p_only_active boolean DEFAULT false)
 RETURNS TABLE(exchange_rate_id integer, company_id integer, from_currency_id integer, to_currency_id integer, rate_date date, exchange_rate numeric, rate_type character varying, rate_source character varying, is_active boolean, effective_from_date date, effective_to_date date, created_by integer, created_date timestamp without time zone, modified_by integer, modified_date timestamp without time zone)
 LANGUAGE plpgsql
AS $function$
BEGIN
    IF p_only_active THEN
        RETURN QUERY
        SELECT *
        FROM currency_exchange_rates
        WHERE is_active = TRUE
        ORDER BY rate_date DESC;
    ELSE
        RETURN QUERY
        SELECT *
        FROM currency_exchange_rates
        ORDER BY rate_date DESC;
    END IF;
END;
$function$
;


-- DROP FUNCTION public.sp_delete_currency_exchange_rate(int4, text);

CREATE OR REPLACE FUNCTION public.sp_delete_currency_exchange_rate(p_exchange_rate_id integer, p_username text)
 RETURNS void
 LANGUAGE plpgsql
AS $function$
DECLARE
    v_user_id INTEGER;
BEGIN
    -- Lookup user_id from username if not null
    IF p_username IS NOT NULL THEN
        SELECT user_id INTO v_user_id
        FROM users
        WHERE username = p_username;
    ELSE
        v_user_id := NULL;
    END IF;

    -- Update record with soft delete
    UPDATE currency_exchange_rates
    SET is_active = FALSE,
        modified_by = v_user_id,
        modified_date = CURRENT_TIMESTAMP
    WHERE exchange_rate_id = p_exchange_rate_id;
END;
$function$
;