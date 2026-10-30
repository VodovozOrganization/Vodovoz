-- Представление для отчетов по статусам отправки в Mindbox
CREATE OR REPLACE VIEW `mindbox_counterparty_status` AS
SELECT
  c.`id` AS `counterparty_id`,
  c.`name` AS `counterparty_name`,
  c.`create_date` AS `create_date`,
  s.`id` AS `outbox_id`,
  s.`status` AS `status`,
  CASE
    WHEN s.`id` IS NULL THEN 'Не отправлялся'
    WHEN s.`status` = 0 THEN 'В очереди'
    WHEN s.`status` = 1 THEN 'Отправлен'
    WHEN s.`status` = 2 THEN 'Ошибка данных'
    WHEN s.`status` = 3 THEN 'Ошибка'
    ELSE 'Неизвестно'
  END AS `status_name`,
  s.`attempts` AS `attempts`,
  s.`last_attempt_at` AS `last_attempt_at`,
  s.`next_attempt_at` AS `next_attempt_at`,
  s.`retry_until` AS `retry_until`,
  s.`last_error` AS `last_error`,
  s.`last_http_status_code` AS `last_http_status_code`,
  s.`last_mindbox_status` AS `last_mindbox_status`,
  s.`last_error_id` AS `last_error_id`,
  s.`transaction_id` AS `transaction_id`
FROM `counterparty` c
LEFT JOIN `mindbox_counterparty_outbox` s ON s.`counterparty_id` = c.`id`;
