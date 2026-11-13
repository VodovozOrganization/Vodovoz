-- Данные синхронизации онлайн-заказа с Mindbox.
ALTER TABLE `online_orders`
  ADD COLUMN `mindbox_snapshot` longtext NULL,
  ADD COLUMN `mindbox_snapshot_updated_at` datetime(6) NULL,
  ADD COLUMN `mindbox_last_change_at` datetime(6) NULL,
  ADD COLUMN `mindbox_sync_requested_at` datetime(6) NULL,
  ADD COLUMN `bonuses_available` tinyint(1) NOT NULL DEFAULT 0;
