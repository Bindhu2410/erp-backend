-- Update opportunity statuses to new values
UPDATE sales_opportunities 
SET status = 'Identified' 
WHERE status IN ('Prospecting', 'Qualified');

UPDATE sales_opportunities 
SET status = 'Solution Presentation' 
WHERE status = 'Initial Contact' 
   OR status = 'Presentation';

UPDATE sales_opportunities 
SET status = 'Proposal' 
WHERE status = 'Proposal Pending' 
   OR status = 'Proposal Sent';

UPDATE sales_opportunities 
SET status = 'Negotiation' 
WHERE status = 'In Negotiation';

UPDATE sales_opportunities 
SET status = 'Closed Won' 
WHERE status = 'Won' 
   OR status = 'Closed - Won';
