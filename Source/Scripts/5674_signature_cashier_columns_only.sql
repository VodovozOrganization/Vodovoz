-- 5674: только колонки (без сида факсимиле), если большой SQL не удобно
ALTER TABLE organization_versions
	ADD COLUMN signature_cashier_id INT NULL AFTER signature_accountant_id;

UPDATE organization_versions
SET signature_cashier_id = signature_leader_id
WHERE signature_cashier_id IS NULL AND signature_leader_id IS NOT NULL;

ALTER TABLE receipt_correction_explanatory_notes
	ADD COLUMN signer_signature_id INT NULL AFTER signer_name;