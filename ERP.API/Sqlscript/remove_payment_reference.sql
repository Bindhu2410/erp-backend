-- Migration: Remove payment_reference from payments table
ALTER TABLE payments DROP COLUMN IF EXISTS payment_reference;
