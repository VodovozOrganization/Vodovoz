-- 009. Переход на общий outbox MassTransit (online_orders_outbox_*).
-- Отдельная таблица очереди mindbox_counterparty_outbox и её view больше не используются.
ALTER TABLE `counterparty`
  ADD COLUMN `mindbox_transaction_id` char(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

DROP VIEW IF EXISTS `mindbox_counterparty_status`;
DROP TABLE IF EXISTS `mindbox_counterparty_outbox`;
