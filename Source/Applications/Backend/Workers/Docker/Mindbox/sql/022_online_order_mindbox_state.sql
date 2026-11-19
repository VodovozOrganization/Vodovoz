-- Выделение состояния интеграции онлайн-заказа с Mindbox в отдельную таблицу.
CREATE TABLE IF NOT EXISTS `online_order_mindbox_state` (
  `online_order_id` int(10) unsigned NOT NULL,
  `mindbox_order_id` bigint(20) DEFAULT NULL,
  `mindbox_transaction_id` char(36) DEFAULT NULL,
  `bonuses_written_off` int(11) NOT NULL DEFAULT 0,
  `bonuses_to_earn` int(11) DEFAULT NULL,
  `mindbox_status` enum('Unknown','Queued','Sent','DataError','Error') NOT NULL DEFAULT 'Unknown',
  `mindbox_last_error` varchar(2000) DEFAULT NULL,
  `mindbox_last_attempt_at` datetime(6) DEFAULT NULL,
  `mindbox_attempts` int(11) NOT NULL DEFAULT 0,
  `mindbox_last_http_status_code` int(11) DEFAULT NULL,
  `mindbox_last_status` varchar(50) DEFAULT NULL,
  `mindbox_last_error_id` varchar(100) DEFAULT NULL,
  `mindbox_snapshot` longtext DEFAULT NULL,
  `mindbox_snapshot_updated_at` datetime(6) DEFAULT NULL,
  `mindbox_last_change_at` datetime(6) DEFAULT NULL,
  `mindbox_sync_requested_at` datetime(6) DEFAULT NULL,
  `bonuses_available` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`online_order_id`),
  CONSTRAINT `fk_online_order_mindbox_state_order`
    FOREIGN KEY (`online_order_id`) REFERENCES `online_orders` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

INSERT IGNORE INTO `online_order_mindbox_state` (
  `online_order_id`,
  `mindbox_order_id`,
  `mindbox_transaction_id`,
  `bonuses_written_off`,
  `bonuses_to_earn`,
  `mindbox_status`,
  `mindbox_last_error`,
  `mindbox_last_attempt_at`,
  `mindbox_attempts`,
  `mindbox_last_http_status_code`,
  `mindbox_last_status`,
  `mindbox_last_error_id`,
  `mindbox_snapshot`,
  `mindbox_snapshot_updated_at`,
  `mindbox_last_change_at`,
  `mindbox_sync_requested_at`,
  `bonuses_available`
)
SELECT
  `id`,
  `mindbox_order_id`,
  `mindbox_transaction_id`,
  `bonuses_written_off`,
  `bonuses_to_earn`,
  `mindbox_status`,
  `mindbox_last_error`,
  `mindbox_last_attempt_at`,
  `mindbox_attempts`,
  `mindbox_last_http_status_code`,
  `mindbox_last_status`,
  `mindbox_last_error_id`,
  `mindbox_snapshot`,
  `mindbox_snapshot_updated_at`,
  `mindbox_last_change_at`,
  `mindbox_sync_requested_at`,
  `bonuses_available`
FROM `online_orders`;

ALTER TABLE `online_orders`
  DROP COLUMN `mindbox_order_id`,
  DROP COLUMN `mindbox_transaction_id`,
  DROP COLUMN `bonuses_written_off`,
  DROP COLUMN `bonuses_to_earn`,
  DROP COLUMN `mindbox_status`,
  DROP COLUMN `mindbox_last_error`,
  DROP COLUMN `mindbox_last_attempt_at`,
  DROP COLUMN `mindbox_attempts`,
  DROP COLUMN `mindbox_last_http_status_code`,
  DROP COLUMN `mindbox_last_status`,
  DROP COLUMN `mindbox_last_error_id`,
  DROP COLUMN `mindbox_snapshot`,
  DROP COLUMN `mindbox_snapshot_updated_at`,
  DROP COLUMN `mindbox_last_change_at`,
  DROP COLUMN `mindbox_sync_requested_at`,
  DROP COLUMN `bonuses_available`;
