-- Переход со старой схемы, где статус и transactionId хранились в counterparty.
-- Выполнять только если ранее применялись 007 и 009 и в counterparty есть колонки
-- mindbox_status и mindbox_transaction_id. Для чистой установки этот скрипт не нужен.
INSERT INTO `mindbox_counterparty_state`
  (`counterparty_id`, `status`, `transaction_id`, `attempts`, `created_at`, `updated_at`)
SELECT
  `id`,
  `mindbox_status`,
  CASE
    WHEN `mindbox_transaction_id` IS NULL
      OR `mindbox_transaction_id` = '00000000-0000-0000-0000-000000000000'
      THEN NULL
    ELSE `mindbox_transaction_id`
  END,
  0,
  NOW(6),
  NOW(6)
FROM `counterparty`
WHERE `mindbox_status` <> 'Unknown'
   OR `mindbox_transaction_id` <> '00000000-0000-0000-0000-000000000000'
ON DUPLICATE KEY UPDATE
  `status` = VALUES(`status`),
  `transaction_id` = VALUES(`transaction_id`),
  `updated_at` = VALUES(`updated_at`);

ALTER TABLE `counterparty`
  DROP COLUMN `mindbox_status`,
  DROP COLUMN `mindbox_transaction_id`;
