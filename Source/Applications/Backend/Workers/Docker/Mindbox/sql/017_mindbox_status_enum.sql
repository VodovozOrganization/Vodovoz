-- Перевод уже созданной колонки mindbox_status из int в ENUM.
-- Для новых установок колонка создаётся сразу как ENUM в 016_mindbox_online_order_bonus.sql.

ALTER TABLE `online_orders`
  MODIFY COLUMN `mindbox_status` varchar(20) NOT NULL DEFAULT 'Unknown';

UPDATE `online_orders`
SET `mindbox_status` = CASE `mindbox_status`
  WHEN '0' THEN 'Unknown'
  WHEN '1' THEN 'Queued'
  WHEN '2' THEN 'Sent'
  WHEN '3' THEN 'DataError'
  WHEN '4' THEN 'Error'
  ELSE 'Unknown'
END;

ALTER TABLE `online_orders`
  MODIFY COLUMN `mindbox_status` ENUM('Unknown', 'Queued', 'Sent', 'DataError', 'Error') NOT NULL DEFAULT 'Unknown';
