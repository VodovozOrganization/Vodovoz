-- Приведение уже созданной таблицы состояния к типам проекта:
-- transaction_id nullable, числовые поля unsigned int(10).
-- Выполнять после 010, если таблица была создана предыдущей версией скрипта.
ALTER TABLE `mindbox_counterparty_state`
  DROP FOREIGN KEY `fk_mindbox_counterparty_state_counterparty`;

ALTER TABLE `mindbox_counterparty_state`
  MODIFY COLUMN `counterparty_id` int(10) unsigned NOT NULL,
  MODIFY COLUMN `transaction_id` char(36) DEFAULT NULL,
  MODIFY COLUMN `attempts` int(10) unsigned NOT NULL DEFAULT 0,
  MODIFY COLUMN `last_http_status_code` int(10) unsigned DEFAULT NULL,
  MODIFY COLUMN `last_attempt_duration_ms` int(10) unsigned DEFAULT NULL;

UPDATE `mindbox_counterparty_state`
SET `transaction_id` = NULL
WHERE `transaction_id` = '00000000-0000-0000-0000-000000000000';

ALTER TABLE `mindbox_counterparty_state`
  ADD CONSTRAINT `fk_mindbox_counterparty_state_counterparty`
    FOREIGN KEY (`counterparty_id`) REFERENCES `counterparty` (`id`) ON DELETE CASCADE;
