-- Бонусные данные онлайн-заказа для Mindbox.
ALTER TABLE `online_orders`
  ADD COLUMN `mindbox_order_id` bigint NULL,
  ADD COLUMN `mindbox_transaction_id` char(36) NULL,
  ADD COLUMN `bonuses_written_off` int NOT NULL DEFAULT 0,
  ADD COLUMN `bonuses_to_earn` int NULL,
  ADD COLUMN `mindbox_status` int NOT NULL DEFAULT 0,
  ADD COLUMN `mindbox_last_error` varchar(2000) NULL,
  ADD COLUMN `mindbox_last_attempt_at` datetime(6) NULL,
  ADD COLUMN `mindbox_attempts` int NOT NULL DEFAULT 0,
  ADD COLUMN `mindbox_last_http_status_code` int NULL,
  ADD COLUMN `mindbox_last_status` varchar(50) NULL,
  ADD COLUMN `mindbox_last_error_id` varchar(100) NULL;

ALTER TABLE `online_order_items`
  ADD COLUMN `bonuses_written_off` int NOT NULL DEFAULT 0,
  ADD COLUMN `current_sum` decimal(10,2) NOT NULL DEFAULT 0;
