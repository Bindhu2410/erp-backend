-- Create sequences if they don't exist
CREATE SEQUENCE IF NOT EXISTS lead_id_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE IF NOT EXISTS opportunity_id_seq START WITH 1 INCREMENT BY 1;

-- Reset sequences
ALTER SEQUENCE lead_id_seq RESTART WITH 1;
ALTER SEQUENCE opportunity_id_seq RESTART WITH 1;

-- Update existing lead IDs to new format using CTE
WITH numbered_leads AS (
    SELECT id, 
           'LD' || LPAD(ROW_NUMBER() OVER (ORDER BY id)::text, 5, '0') as new_lead_id
    FROM public.sales_lead
    WHERE lead_id NOT LIKE 'LD%'
)
UPDATE public.sales_lead sl
SET lead_id = nl.new_lead_id
FROM numbered_leads nl
WHERE sl.id = nl.id;

-- Update existing opportunity IDs to new format using CTE
WITH numbered_opportunities AS (
    SELECT id, 
           'OPP' || LPAD(ROW_NUMBER() OVER (ORDER BY id)::text, 5, '0') as new_opportunity_id
    FROM public.sales_opportunities
    WHERE opportunity_id NOT LIKE 'OPP%'
)
UPDATE public.sales_opportunities o
SET opportunity_id = no.new_opportunity_id
FROM numbered_opportunities no
WHERE o.id = no.id;

-- Update the sequences to start after the last used number
SELECT setval('lead_id_seq', (
    SELECT COALESCE(MAX(CAST(SUBSTRING(lead_id FROM 3) AS INTEGER)), 0) 
    FROM public.sales_lead
));

SELECT setval('opportunity_id_seq', (
    SELECT COALESCE(MAX(CAST(SUBSTRING(opportunity_id FROM 4) AS INTEGER)), 0) 
    FROM public.sales_opportunities
));

-- Verify the changes
SELECT id, lead_id 
FROM public.sales_lead 
ORDER BY id;

SELECT id, opportunity_id 
FROM public.sales_opportunities 
ORDER BY id;

-- After verification, you can remove the backup columns if desired:
-- ALTER TABLE public.sales_lead DROP COLUMN old_lead_id;
-- ALTER TABLE public.sales_opportunities DROP COLUMN old_opportunity_id;
