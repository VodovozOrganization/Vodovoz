-- Миграция таблицы очереди с версии 1 (если таблица уже создана предыдущей версией).
-- Выполнять один раз. Если колонки уже добавлены, скрипт завершится ошибкой — это ожидаемо.
ALTER TABLE `mindbox_counterparty_outbox`
  ADD COLUMN `last_attempt_at` datetime(6) DEFAULT NULL AFTER `attempts`,
  ADD COLUMN `last_http_status_code` int(10) unsigned DEFAULT NULL AFTER `last_error`,
  ADD COLUMN `last_mindbox_status` varchar(50) DEFAULT NULL AFTER `last_http_status_code`,
  ADD COLUMN `last_error_id` varchar(100) DEFAULT NULL AFTER `last_mindbox_status`,
  ADD COLUMN `last_attempt_duration_ms` bigint(20) unsigned DEFAULT NULL AFTER `last_error_id`,
  ADD COLUMN `retry_until` datetime(6) DEFAULT NULL AFTER `next_attempt_at`;

UPDATE `mindbox_counterparty_outbox`
SET `retry_until` = DATE_ADD(`created_at`, INTERVAL 24 HOUR)
WHERE `retry_until` IS NULL;

ALTER TABLE `mindbox_counterparty_outbox`
  MODIFY COLUMN `retry_until` datetime(6) NOT NULL;

-- Приведение целочисленных колонок к типу БД: int(10) unsigned.
ALTER TABLE `mindbox_counterparty_outbox`
  MODIFY COLUMN `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  MODIFY COLUMN `counterparty_id` int(10) unsigned NOT NULL,
  MODIFY COLUMN `attempts` int(10) unsigned NOT NULL DEFAULT 0;

-- Status хранится как MySQL ENUM. Сначала переводим старые числовые значения в имена.
ALTER TABLE `mindbox_counterparty_outbox`
  MODIFY COLUMN `status` varchar(50) NULL DEFAULT NULL;

UPDATE `mindbox_counterparty_outbox`
SET `status` = 'Unknown'
WHERE `status` IS NULL;

UPDATE `mindbox_counterparty_outbox`
SET `status` = CASE `status`
  WHEN '0' THEN 'Queued'
  WHEN '1' THEN 'Sent'
  WHEN '2' THEN 'DataError'
  WHEN '3' THEN 'Error'
  ELSE `status`
END
WHERE `status` IN ('0', '1', '2', '3');

ALTER TABLE `mindbox_counterparty_outbox`
  MODIFY COLUMN `status` enum('Unknown','Queued','Sent','DataError','Error') NOT NULL DEFAULT 'Unknown';

ALTER TABLE `mindbox_counterparty_outbox`
  ADD UNIQUE KEY `ux_mindbox_counterparty_outbox_counterparty` (`counterparty_id`);
