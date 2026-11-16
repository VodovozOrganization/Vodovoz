-- Состояние отправки контрагентов в Mindbox (1 запись на контрагента).
CREATE TABLE IF NOT EXISTS `mindbox_counterparty_state` (
  `counterparty_id` int(10) unsigned NOT NULL,
  `status` enum('Unknown','Queued','Sent','DataError','Error') NOT NULL DEFAULT 'Unknown',
  `transaction_id` char(36) DEFAULT NULL,
  `attempts` int(10) unsigned NOT NULL DEFAULT 0,
  `last_attempt_at` datetime(6) DEFAULT NULL,
  `last_error` varchar(2000) DEFAULT NULL,
  `last_http_status_code` int(10) unsigned DEFAULT NULL,
  `last_mindbox_status` varchar(50) DEFAULT NULL,
  `last_error_id` varchar(100) DEFAULT NULL,
  `last_attempt_duration_ms` int(10) unsigned DEFAULT NULL,
  `sent_at` datetime(6) DEFAULT NULL,
  `created_at` datetime(6) DEFAULT NULL,
  `updated_at` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`counterparty_id`),
  KEY `ix_mindbox_counterparty_state_status` (`status`),
  CONSTRAINT `fk_mindbox_counterparty_state_counterparty`
    FOREIGN KEY (`counterparty_id`) REFERENCES `counterparty` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
