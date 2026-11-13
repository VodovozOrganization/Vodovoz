-- 008. Переводит статус Mindbox в MySQL ENUM со значением Unknown по умолчанию.
-- Выполнять один раз, если колонка уже создана как int.
-- Шаг 1: int -> varchar (чтобы преобразовать числовые значения в метки).
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

-- Шаг 2: varchar -> ENUM.
ALTER TABLE `counterparty`
  MODIFY COLUMN `mindbox_status` enum('Unknown','Queued','Sent','DataError','Error') NOT NULL DEFAULT 'Unknown';
