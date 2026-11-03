-- 008. Перевод статусов Mindbox на MySQL ENUM и статус Unknown.
-- Выполняется для БД, где колонки статусов уже существуют с типом int.

-- 1. Outbox: int -> varchar (для перекодировки) -> ENUM.
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

-- 2. Денормализованный статус контрагента: int -> varchar (для перекодировки) -> ENUM.
ALTER TABLE `counterparty`
  MODIFY COLUMN `mindbox_status` varchar(50) NULL DEFAULT NULL;

UPDATE `counterparty`
SET `mindbox_status` = 'Unknown'
WHERE `mindbox_status` IS NULL;

UPDATE `counterparty`
SET `mindbox_status` = CASE `mindbox_status`
  WHEN '0' THEN 'Queued'
  WHEN '1' THEN 'Sent'
  WHEN '2' THEN 'DataError'
  WHEN '3' THEN 'Error'
  ELSE `mindbox_status`
END
WHERE `mindbox_status` IN ('0', '1', '2', '3');

ALTER TABLE `counterparty`
  MODIFY COLUMN `mindbox_status` enum('Unknown','Queued','Sent','DataError','Error') NOT NULL DEFAULT 'Unknown';
