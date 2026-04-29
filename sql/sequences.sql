-- Create sequences for lead, opportunity, and quotation IDs
CREATE SEQUENCE IF NOT EXISTS lead_id_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE IF NOT EXISTS opportunity_id_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE IF NOT EXISTS quotation_id_seq START WITH 1 INCREMENT BY 1;

-- Create functions to generate IDs with prefixes
CREATE OR REPLACE FUNCTION generate_lead_id() 
RETURNS varchar AS $$
BEGIN
    RETURN 'LD' || LPAD(nextval('lead_id_seq')::text, 5, '0');
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION generate_opportunity_id() 
RETURNS varchar AS $$
BEGIN
    RETURN 'OPP' || LPAD(nextval('opportunity_id_seq')::text, 5, '0');
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION generate_quotation_id() 
RETURNS varchar AS $$
BEGIN
    RETURN 'QT-' || LPAD(nextval('quotation_id_seq')::text, 4, '0');
END;
$$ LANGUAGE plpgsql;

-- Add lead_id column if not exists
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'sales_lead' AND column_name = 'lead_id'
    ) THEN
        ALTER TABLE public.sales_lead ADD COLUMN lead_id character varying(255);
        
        -- Update existing records with a generated lead_id (LD + 5-digit sequence)
        WITH numbered_rows AS (
            SELECT id, 'LD' || LPAD(ROW_NUMBER() OVER (ORDER BY id)::text, 5, '0') as new_lead_id
            FROM public.sales_lead
        )
        UPDATE public.sales_lead
        SET lead_id = numbered_rows.new_lead_id
        FROM numbered_rows
        WHERE sales_lead.id = numbered_rows.id;

        -- Add NOT NULL constraint after filling in values
        ALTER TABLE public.sales_lead ALTER COLUMN lead_id SET NOT NULL;
    END IF;
END $$;

-- Add opportunity_id column if not exists
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'sales_opportunities' AND column_name = 'opportunity_id'
    ) THEN
        ALTER TABLE public.sales_opportunities ADD COLUMN opportunity_id character varying(255);
        
        -- Update existing records with a generated opportunity_id (OPP + 5-digit sequence)
        WITH numbered_rows AS (
            SELECT id, 'OPP' || LPAD(ROW_NUMBER() OVER (ORDER BY id)::text, 5, '0') as new_opportunity_id
            FROM public.sales_opportunities
        )
        UPDATE public.sales_opportunities
        SET opportunity_id = numbered_rows.new_opportunity_id
        FROM numbered_rows
        WHERE sales_opportunities.id = numbered_rows.id;

        -- Add NOT NULL constraint after filling in values
        ALTER TABLE public.sales_opportunities ALTER COLUMN opportunity_id SET NOT NULL;
    END IF;
END $$;