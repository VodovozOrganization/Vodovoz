-- Статус отправки контрагента в Mindbox.
-- Значения хранятся в MySQL ENUM, по умолчанию Unknown.
ALTER TABLE `counterparty`
  ADD COLUMN `mindbox_status` enum('Unknown','Queued','Sent','DataError','Error') NOT NULL DEFAULT 'Unknown';
