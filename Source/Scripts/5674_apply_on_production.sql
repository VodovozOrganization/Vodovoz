-- =============================================================================
-- 5674_AutoVoidAndResend — ОДИН скрипт для рабочей / чистой БД (fiscal batch)
-- =============================================================================
-- Применить целиком, сверху вниз (MySQL).
--
-- Включает:
--   1) таблицы процесса корректировки чеков (receipt_*, без receipt_operations)
--   2) cashier_id в organization_versions (+ по умолчанию = leader_id)
--   3) signature_cashier_id в organization_versions (+ по умолчанию = signature_leader_id)
--   4) signer_signature_id в receipt_correction_explanatory_notes
--
-- НЕ включает:
--   - receipt_operations / receipt_operation_items (устаревший подход)
--   - сид JPEG факсимиле в stored_resource
--   - назначение конкретных кассиров по ФИО (Васильева / Фахрутдинова / …)
--
-- Если БД уже на старой схеме с receipt_operations — сначала:
--   Source/Scripts/5674_fiscal_batch_migration.sql
--
-- Кассира и «Подпись кассира» после скрипта задайте в ДВ:
--   Справочники → Наша организация → Организации → Версии
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 1. Таблицы процесса (пачка фискальных документов)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS receipt_correction_processes (
	id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	order_id INT NOT NULL,
	source_edo_fiscal_document_id INT NULL,
	receipt_edo_task_id INT NULL,
	scenario_type INT NOT NULL,
	status INT NOT NULL,
	change_fingerprint VARCHAR(128) NOT NULL,
	error_description TEXT NULL,
	created_date DATETIME NOT NULL,
	completed_date DATETIME NULL,
	organization_id INT NULL,
	baseline_fiscal_document_number VARCHAR(64) NULL,
	baseline_fiscal_document_date DATETIME NULL,
	baseline_sum DECIMAL(14, 2) NOT NULL DEFAULT 0,
	INDEX ix_receipt_correction_processes_order_id (order_id),
	INDEX ix_receipt_correction_processes_receipt_edo_task_id (receipt_edo_task_id),
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
	signer_signature_id INT NULL,
	content TEXT NOT NULL,
	created_date DATETIME NOT NULL,
	INDEX ix_receipt_correction_explanatory_notes_process_id (receipt_correction_process_id)
);

-- Внешние ключи (идемпотентно)

SET @schema_name = DATABASE();

SET @sql = (
	SELECT IF(
		EXISTS(
			SELECT 1 FROM information_schema.TABLE_CONSTRAINTS
			WHERE CONSTRAINT_SCHEMA = @schema_name
				AND TABLE_NAME = 'receipt_correction_process_documents'
				AND CONSTRAINT_NAME = 'fk_receipt_correction_process_documents_process'
		),
		'SELECT ''fk_receipt_correction_process_documents_process already exists'' AS info',
		'ALTER TABLE receipt_correction_process_documents ADD CONSTRAINT fk_receipt_correction_process_documents_process FOREIGN KEY (receipt_correction_process_id) REFERENCES receipt_correction_processes(id)'
	)
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = (
	SELECT IF(
		EXISTS(
			SELECT 1 FROM information_schema.TABLE_CONSTRAINTS
			WHERE CONSTRAINT_SCHEMA = @schema_name
				AND TABLE_NAME = 'receipt_correction_explanatory_notes'
				AND CONSTRAINT_NAME = 'fk_receipt_correction_explanatory_notes_process'
		),
		'SELECT ''fk_receipt_correction_explanatory_notes_process already exists'' AS info',
		'ALTER TABLE receipt_correction_explanatory_notes ADD CONSTRAINT fk_receipt_correction_explanatory_notes_process FOREIGN KEY (receipt_correction_process_id) REFERENCES receipt_correction_processes(id)'
	)
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Колонки, которых могло не быть на частично накатанных стендах
SET @sql = (
	SELECT IF(
		EXISTS(
			SELECT 1 FROM information_schema.COLUMNS
			WHERE TABLE_SCHEMA = @schema_name
				AND TABLE_NAME = 'receipt_correction_processes'
				AND COLUMN_NAME = 'source_edo_fiscal_document_id'
		),
		'SELECT ''source_edo_fiscal_document_id already exists'' AS info',
		'ALTER TABLE receipt_correction_processes ADD COLUMN source_edo_fiscal_document_id INT NULL AFTER order_id'
	)
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = (
	SELECT IF(
		EXISTS(
			SELECT 1 FROM information_schema.COLUMNS
			WHERE TABLE_SCHEMA = @schema_name
				AND TABLE_NAME = 'receipt_correction_processes'
				AND COLUMN_NAME = 'receipt_edo_task_id'
		),
		'SELECT ''receipt_edo_task_id already exists'' AS info',
		'ALTER TABLE receipt_correction_processes ADD COLUMN receipt_edo_task_id INT NULL AFTER source_edo_fiscal_document_id'
	)
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = (
	SELECT IF(
		EXISTS(
			SELECT 1 FROM information_schema.COLUMNS
			WHERE TABLE_SCHEMA = @schema_name
				AND TABLE_NAME = 'receipt_correction_processes'
				AND COLUMN_NAME = 'baseline_fiscal_document_number'
		),
		'SELECT ''baseline_fiscal_document_number already exists'' AS info',
		'ALTER TABLE receipt_correction_processes ADD COLUMN baseline_fiscal_document_number VARCHAR(64) NULL AFTER organization_id'
	)
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = (
	SELECT IF(
		EXISTS(
			SELECT 1 FROM information_schema.COLUMNS
			WHERE TABLE_SCHEMA = @schema_name
				AND TABLE_NAME = 'receipt_correction_processes'
				AND COLUMN_NAME = 'baseline_fiscal_document_date'
		),
		'SELECT ''baseline_fiscal_document_date already exists'' AS info',
		'ALTER TABLE receipt_correction_processes ADD COLUMN baseline_fiscal_document_date DATETIME NULL AFTER baseline_fiscal_document_number'
	)
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = (
	SELECT IF(
		EXISTS(
			SELECT 1 FROM information_schema.COLUMNS
			WHERE TABLE_SCHEMA = @schema_name
				AND TABLE_NAME = 'receipt_correction_processes'
				AND COLUMN_NAME = 'baseline_sum'
		),
		'SELECT ''baseline_sum already exists'' AS info',
		'ALTER TABLE receipt_correction_processes ADD COLUMN baseline_sum DECIMAL(14, 2) NOT NULL DEFAULT 0 AFTER baseline_fiscal_document_date'
	)
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = (
	SELECT IF(
		EXISTS(
			SELECT 1 FROM information_schema.COLUMNS
			WHERE TABLE_SCHEMA = @schema_name
				AND TABLE_NAME = 'receipt_correction_explanatory_notes'
				AND COLUMN_NAME = 'signer_signature_id'
		),
		'SELECT ''signer_signature_id already exists'' AS info',
		'ALTER TABLE receipt_correction_explanatory_notes ADD COLUMN signer_signature_id INT NULL AFTER signer_name'
	)
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------------------------------
-- 2. Кассир в версиях организации
-- -----------------------------------------------------------------------------

SET @sql = (
	SELECT IF(
		EXISTS(
			SELECT 1 FROM information_schema.COLUMNS
			WHERE TABLE_SCHEMA = @schema_name
				AND TABLE_NAME = 'organization_versions'
				AND COLUMN_NAME = 'cashier_id'
		),
		'SELECT ''cashier_id already exists'' AS info',
		'ALTER TABLE organization_versions ADD COLUMN cashier_id INT NULL AFTER accountant_id'
	)
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

UPDATE organization_versions
SET cashier_id = leader_id
WHERE cashier_id IS NULL AND leader_id IS NOT NULL;

-- -----------------------------------------------------------------------------
-- 3. Подпись кассира (факсимиле) в версиях организации
-- -----------------------------------------------------------------------------

SET @sql = (
	SELECT IF(
		EXISTS(
			SELECT 1 FROM information_schema.COLUMNS
			WHERE TABLE_SCHEMA = @schema_name
				AND TABLE_NAME = 'organization_versions'
				AND COLUMN_NAME = 'signature_cashier_id'
		),
		'SELECT ''signature_cashier_id already exists'' AS info',
		'ALTER TABLE organization_versions ADD COLUMN signature_cashier_id INT NULL AFTER signature_accountant_id'
	)
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

UPDATE organization_versions
SET signature_cashier_id = signature_leader_id
WHERE signature_cashier_id IS NULL AND signature_leader_id IS NOT NULL;

-- -----------------------------------------------------------------------------
-- 4. Проверка
-- -----------------------------------------------------------------------------
-- SHOW TABLES LIKE 'receipt_%';
-- SHOW COLUMNS FROM receipt_correction_processes;
-- SHOW COLUMNS FROM organization_versions LIKE '%cashier%';
-- SHOW COLUMNS FROM receipt_correction_explanatory_notes LIKE 'signer_signature_id';
--
-- Дальше в ДВ: для нужных орг выставить Кассир и Подпись кассира
-- (ресурсы подписи — как у руководителя/бухгалтера, через stored_resource).
