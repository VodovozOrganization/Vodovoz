-- Статус отправки контрагента в Mindbox (денормализация из mindbox_counterparty_outbox).
-- Хранится как MySQL ENUM, по умолчанию Unknown.
ALTER TABLE `counterparty`
  ADD COLUMN `mindbox_status` enum('Unknown','Queued','Sent','DataError','Error') NOT NULL DEFAULT 'Unknown';

-- Заполняем статусы для уже поставленных в очередь контрагентов.
UPDATE `counterparty` c
JOIN `mindbox_counterparty_outbox` o ON o.`counterparty_id` = c.`id`
SET c.`mindbox_status` = o.`status`;
