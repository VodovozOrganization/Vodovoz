-- 5674: переход на fiscal batch (без receipt_operations).
-- Для БД, где уже применён 5674_apply_on_production.sql с receipt_operations.

ALTER TABLE receipt_correction_processes
	ADD COLUMN IF NOT EXISTS source_edo_fiscal_document_id INT NULL AFTER order_id,
	ADD COLUMN IF NOT EXISTS receipt_edo_task_id INT NULL AFTER source_edo_fiscal_document_id,
	ADD COLUMN IF NOT EXISTS baseline_fiscal_document_number VARCHAR(64) NULL AFTER organization_id,
	ADD COLUMN IF NOT EXISTS baseline_fiscal_document_date DATETIME NULL AFTER baseline_fiscal_document_number,
	ADD COLUMN IF NOT EXISTS baseline_sum DECIMAL(14, 2) NOT NULL DEFAULT 0 AFTER baseline_fiscal_document_date;

-- Перенос baseline из receipt_operations, если таблица ещё есть.
UPDATE receipt_correction_processes p
INNER JOIN receipt_operations o ON o.id = p.receipt_operation_id
SET
	p.source_edo_fiscal_document_id = o.source_edo_fiscal_document_id,
	p.baseline_fiscal_document_number = o.fiscal_document_number,
	p.baseline_fiscal_document_date = o.fiscal_document_date,
	p.baseline_sum = o.sum
WHERE p.source_edo_fiscal_document_id IS NULL;

-- Для уже созданных процессов: task = task первого edo-документа пачки.
UPDATE receipt_correction_processes p
INNER JOIN (
	SELECT pd.receipt_correction_process_id AS process_id, MIN(efd.receipt_edo_task_id) AS task_id
	FROM receipt_correction_process_documents pd
	INNER JOIN edo_fiscal_documents efd ON efd.id = pd.edo_fiscal_document_id
	GROUP BY pd.receipt_correction_process_id
) src ON src.process_id = p.id
SET p.receipt_edo_task_id = src.task_id
WHERE p.receipt_edo_task_id IS NULL;

ALTER TABLE receipt_correction_processes
	DROP FOREIGN KEY IF EXISTS fk_receipt_correction_processes_operation;

ALTER TABLE receipt_correction_processes
	DROP COLUMN IF EXISTS receipt_operation_id;

DROP TABLE IF EXISTS receipt_operation_items;
DROP TABLE IF EXISTS receipt_operations;
