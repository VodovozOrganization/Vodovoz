-- Миграция таблицы очереди с версии 1 (если таблица уже создана предыдущей версией).
-- Выполнять один раз. Если колонки уже добавлены, скрипт завершится ошибкой — это ожидаемо.
ALTER TABLE `mindbox_counterparty_outbox`
  ADD COLUMN `last_attempt_at` datetime(6) DEFAULT NULL AFTER `attempts`,
  ADD COLUMN `last_http_status_code` int(11) DEFAULT NULL AFTER `last_error`,
  ADD COLUMN `last_mindbox_status` varchar(50) DEFAULT NULL AFTER `last_http_status_code`,
  ADD COLUMN `last_error_id` varchar(100) DEFAULT NULL AFTER `last_mindbox_status`,
  ADD COLUMN `last_attempt_duration_ms` bigint(20) DEFAULT NULL AFTER `last_error_id`,
  ADD COLUMN `retry_until` datetime(6) DEFAULT NULL AFTER `next_attempt_at`;

UPDATE `mindbox_counterparty_outbox`
SET `retry_until` = DATE_ADD(`created_at`, INTERVAL 24 HOUR)
WHERE `retry_until` IS NULL;

ALTER TABLE `mindbox_counterparty_outbox`
  MODIFY COLUMN `retry_until` datetime(6) NOT NULL;

-- В старой версии status=2 означал общую ошибку.
UPDATE `mindbox_counterparty_outbox`
SET `status` = 3
WHERE `status` = 2;

ALTER TABLE `mindbox_counterparty_outbox`
  ADD UNIQUE KEY `ux_mindbox_counterparty_outbox_counterparty` (`counterparty_id`);
