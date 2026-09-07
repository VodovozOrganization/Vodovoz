-- 5674_AutoVoidAndResend: таблицы процесса корректировки чеков
-- Применить вручную на dev/stage перед тестированием журналов.
-- Выполнять скрипт целиком, сверху вниз.
-- Если уже были созданы receipt_baselines* — сначала выполнить 5674_rename_baseline_to_operation.sql.

CREATE TABLE IF NOT EXISTS receipt_operations (
	id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	order_id INT NOT NULL,
	source_edo_fiscal_document_id INT NULL,
	source_cash_receipt_id INT NULL,
	organization_id INT NULL,
	counterparty_id INT NULL,
	contract_id INT NULL,
	cashbox_id INT NULL,
	payment_type INT NULL,
	client_inn VARCHAR(20) NULL,
	contact VARCHAR(255) NULL,
	delivery_date DATETIME NULL,
	fiscal_document_number VARCHAR(64) NULL,
	fiscal_document_date DATETIME NULL,
	sum DECIMAL(14, 2) NOT NULL DEFAULT 0,
	created_date DATETIME NOT NULL,
	INDEX ix_receipt_operations_order_id (order_id)
);

CREATE TABLE IF NOT EXISTS receipt_operation_items (
	id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	receipt_operation_id INT NOT NULL,
	nomenclature_id INT NULL,
	name VARCHAR(512) NOT NULL,
	quantity DECIMAL(14, 3) NOT NULL,
	price DECIMAL(14, 2) NOT NULL,
	discount_sum DECIMAL(14, 2) NOT NULL DEFAULT 0,
	sum DECIMAL(14, 2) NOT NULL,
	vat INT NOT NULL,
	product_mark TEXT NULL,
	INDEX ix_receipt_operation_items_operation_id (receipt_operation_id)
);

CREATE TABLE IF NOT EXISTS receipt_correction_processes (
	id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	order_id INT NOT NULL,
	receipt_operation_id INT NOT NULL,
	scenario_type INT NOT NULL,
	status INT NOT NULL,
	change_fingerprint VARCHAR(128) NOT NULL,
	error_description TEXT NULL,
	created_date DATETIME NOT NULL,
	completed_date DATETIME NULL,
	organization_id INT NULL,
	INDEX ix_receipt_correction_processes_order_id (order_id),
	INDEX ix_receipt_correction_processes_operation_id (receipt_operation_id),
	INDEX ix_receipt_correction_processes_fingerprint (change_fingerprint)
);

CREATE TABLE IF NOT EXISTS receipt_correction_process_documents (
	id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	receipt_correction_process_id INT NOT NULL,
	planned_document_type INT NOT NULL,
	document_guid CHAR(36) NOT NULL,
	edo_fiscal_document_id INT NULL,
	status INT NOT NULL,
	error_description TEXT NULL,
	UNIQUE KEY ux_receipt_correction_process_documents_guid (document_guid),
	INDEX ix_receipt_correction_process_documents_process_id (receipt_correction_process_id)
);

CREATE TABLE IF NOT EXISTS receipt_correction_explanatory_notes (
	id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	receipt_correction_process_id INT NOT NULL,
	template_type INT NOT NULL,
	organization_id INT NULL,
	signer_title VARCHAR(255) NULL,
	signer_name VARCHAR(255) NULL,
	content TEXT NOT NULL,
	created_date DATETIME NOT NULL,
	INDEX ix_receipt_correction_explanatory_notes_process_id (receipt_correction_process_id)
);

-- Внешние ключи добавляются после создания всех таблиц.
-- Если constraint уже существует, соответствующий ALTER можно пропустить.

ALTER TABLE receipt_operation_items
	ADD CONSTRAINT fk_receipt_operation_items_operation
	FOREIGN KEY (receipt_operation_id) REFERENCES receipt_operations(id);

ALTER TABLE receipt_correction_processes
	ADD CONSTRAINT fk_receipt_correction_processes_operation
	FOREIGN KEY (receipt_operation_id) REFERENCES receipt_operations(id);

ALTER TABLE receipt_correction_process_documents
	ADD CONSTRAINT fk_receipt_correction_process_documents_process
	FOREIGN KEY (receipt_correction_process_id) REFERENCES receipt_correction_processes(id);

ALTER TABLE receipt_correction_explanatory_notes
	ADD CONSTRAINT fk_receipt_correction_explanatory_notes_process
	FOREIGN KEY (receipt_correction_process_id) REFERENCES receipt_correction_processes(id);
