-- Очередь отправки контрагентов в Mindbox
CREATE TABLE IF NOT EXISTS `mindbox_counterparty_outbox` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `counterparty_id` int(11) NOT NULL,
  `transaction_id` char(36) NOT NULL,
  `status` int(11) NOT NULL DEFAULT 0,
  `attempts` int(11) NOT NULL DEFAULT 0,
  `last_error` varchar(2000) DEFAULT NULL,
  `created_at` datetime(6) NOT NULL,
  `next_attempt_at` datetime(6) DEFAULT NULL,
  `sent_at` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `ix_mindbox_counterparty_outbox_pending` (`status`, `next_attempt_at`),
  KEY `ix_mindbox_counterparty_outbox_counterparty` (`counterparty_id`),
  CONSTRAINT `fk_mindbox_counterparty_outbox_counterparty`
    FOREIGN KEY (`counterparty_id`) REFERENCES `counterparty` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
