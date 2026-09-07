-- 5674: переименование ReceiptBaseline → ReceiptOperation для уже созданных таблиц.
-- Выполнять один раз на БД, где уже применён старый 5674_receipt_correction_tables.sql.
-- На чистой БД этот скрипт не нужен — достаточно актуального 5674_receipt_correction_tables.sql.

-- 1. FK процессов на baseline
ALTER TABLE receipt_correction_processes
	DROP FOREIGN KEY fk_receipt_correction_processes_baseline;

-- 2. FK позиций baseline
ALTER TABLE receipt_baseline_positions
	DROP FOREIGN KEY fk_receipt_baseline_positions_baseline;

-- 3. Таблицы и колонки
RENAME TABLE receipt_baselines TO receipt_operations;
RENAME TABLE receipt_baseline_positions TO receipt_operation_items;

ALTER TABLE receipt_operation_items
	CHANGE COLUMN receipt_baseline_id receipt_operation_id INT NOT NULL;

ALTER TABLE receipt_correction_processes
	CHANGE COLUMN receipt_baseline_id receipt_operation_id INT NOT NULL;

-- 4. Индексы (имена могли отличаться — при ошибке «Duplicate key name» пропустить)
ALTER TABLE receipt_operations
	RENAME INDEX ix_receipt_baselines_order_id TO ix_receipt_operations_order_id;

ALTER TABLE receipt_operation_items
	RENAME INDEX ix_receipt_baseline_positions_baseline_id TO ix_receipt_operation_items_operation_id;

ALTER TABLE receipt_correction_processes
	RENAME INDEX ix_receipt_correction_processes_baseline_id TO ix_receipt_correction_processes_operation_id;

-- 5. FK заново
ALTER TABLE receipt_operation_items
	ADD CONSTRAINT fk_receipt_operation_items_operation
	FOREIGN KEY (receipt_operation_id) REFERENCES receipt_operations(id);

ALTER TABLE receipt_correction_processes
	ADD CONSTRAINT fk_receipt_correction_processes_operation
	FOREIGN KEY (receipt_operation_id) REFERENCES receipt_operations(id);
