CREATE TABLE public.geographical_divisions (
	division_id bigserial NOT NULL,
	division_name varchar(100) NOT NULL,
	division_type varchar(50) NOT NULL,
	parent_division_id int8 NULL,
	created_at timestamptz DEFAULT CURRENT_TIMESTAMP NULL,
	updated_at timestamptz DEFAULT CURRENT_TIMESTAMP NULL,
	created_by int8 NULL,
	updated_by int8 NULL,
	CONSTRAINT geographical_divisions_division_name_division_type_parent_d_key UNIQUE (division_name, division_type, parent_division_id),
	CONSTRAINT geographical_divisions_division_type_check CHECK (((division_type)::text = ANY (ARRAY[('Country'::character varying)::text, ('State'::character varying)::text, ('Territory'::character varying)::text, ('District'::character varying)::text, ('City'::character varying)::text, ('Pincode'::character varying)::text, ('Area'::character varying)::text]))),
	CONSTRAINT geographical_divisions_pkey PRIMARY KEY (division_id),
	CONSTRAINT geographical_divisions_parent_division_id_fkey FOREIGN KEY (parent_division_id) REFERENCES public.geographical_divisions(division_id) ON DELETE CASCADE
);
CREATE INDEX idx_geo_divisions_name ON public.geographical_divisions USING btree (division_name);
CREATE INDEX idx_geo_divisions_parent_id ON public.geographical_divisions USING btree (parent_division_id);
CREATE INDEX idx_geo_divisions_type ON public.geographical_divisions USING btree (division_type);



-- Fix for ambiguous column reference in the hierarchy procedure
CREATE OR REPLACE FUNCTION sp_get_geographical_hierarchy_by_pincode(
    p_pincode VARCHAR
)
RETURNS TABLE (
    division_id BIGINT,
    parent_division_id BIGINT,
    division_name VARCHAR,
    division_type VARCHAR,
    level INT
) AS $$
BEGIN
    RETURN QUERY
    WITH RECURSIVE hierarchy_up AS (
        -- Start from the provided Pincode
        SELECT 
            gd.division_id, gd.parent_division_id, gd.division_name, gd.division_type, 0 AS level
        FROM geographical_divisions gd
        WHERE gd.division_type = 'Pincode' AND gd.division_name = p_pincode

        UNION ALL

        -- Recurse upward
        SELECT 
            gd.division_id, gd.parent_division_id, gd.division_name, gd.division_type, h.level - 1 AS level
        FROM geographical_divisions gd
        INNER JOIN hierarchy_up h ON gd.division_id = h.parent_division_id
    ),
    hierarchy_down AS (
        -- Start from the provided Pincode
        SELECT 
            gd.division_id, gd.parent_division_id, gd.division_name, gd.division_type, 0 AS level
        FROM geographical_divisions gd
        WHERE gd.division_type = 'Pincode' AND gd.division_name = p_pincode

        UNION ALL

        -- Recurse downward
        SELECT 
            gd.division_id, gd.parent_division_id, gd.division_name, gd.division_type, h.level + 1 AS level
        FROM geographical_divisions gd
        INNER JOIN hierarchy_down h ON gd.parent_division_id = h.division_id
    )

    SELECT * FROM hierarchy_up
    UNION
    SELECT * FROM hierarchy_down
    ORDER BY level;

END;
$$ LANGUAGE plpgsql;



CREATE OR REPLACE FUNCTION sp_create_geographical_division(
    p_division_name VARCHAR,
    p_division_type VARCHAR,
    p_parent_division_id BIGINT DEFAULT NULL,
    p_created_by BIGINT DEFAULT NULL
)
RETURNS BIGINT AS $$
DECLARE
    v_division_id BIGINT;
BEGIN
    INSERT INTO public.geographical_divisions (
        division_name,
        division_type,
        parent_division_id,
        created_by,
        updated_by
    ) VALUES (
        p_division_name,
        p_division_type,
        p_parent_division_id,
        p_created_by,
        p_created_by
    )
    RETURNING division_id INTO v_division_id;

    RETURN v_division_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION sp_get_geographical_division_by_id(
    p_division_id BIGINT
)
RETURNS TABLE (
    division_id BIGINT,
    division_name VARCHAR,
    division_type VARCHAR,
    parent_division_id BIGINT,
    created_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ,
    created_by BIGINT,
    updated_by BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        gd.division_id,
        gd.division_name,
        gd.division_type,
        gd.parent_division_id,
        gd.created_at,
        gd.updated_at,
        gd.created_by,
        gd.updated_by
    FROM public.geographical_divisions gd
    WHERE gd.division_id = p_division_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION sp_update_geographical_division(
    p_division_id BIGINT,
    p_division_name VARCHAR,
    p_division_type VARCHAR,
    p_parent_division_id BIGINT DEFAULT NULL,
    p_updated_by BIGINT DEFAULT NULL
)
RETURNS BOOLEAN AS $$
DECLARE
    v_rowcount INT;
BEGIN
    UPDATE public.geographical_divisions
    SET 
        division_name = p_division_name,
        division_type = p_division_type,
        parent_division_id = p_parent_division_id,
        updated_at = CURRENT_TIMESTAMP,
        updated_by = p_updated_by
    WHERE division_id = p_division_id;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;
    RETURN v_rowcount > 0;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION sp_delete_geographical_division(
    p_division_id BIGINT
)
RETURNS BOOLEAN AS $$
DECLARE
    v_rowcount INT;
BEGIN
    DELETE FROM public.geographical_divisions
    WHERE division_id = p_division_id;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;
    RETURN v_rowcount > 0;
END;
$$ LANGUAGE plpgsql;


