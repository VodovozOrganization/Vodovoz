-- ВНИМАНИЕ: выполнять только на тестовой базе!
-- Скрипт выгружает идентификаторы, ранее отправленные в боевые карточки,
-- и сбрасывает статусы Mindbox для повторной отправки под тестовыми идентификаторами.

-- 1. Список ранее отправленных контрагентов и заказов.
SELECT 'counterparty' AS entity_type, counterparty_id AS erp_id, status, sent_at
FROM mindbox_counterparty_state
WHERE status IN ('Queued', 'Sent', 'DataError', 'Error');

SELECT 'online_order' AS entity_type, online_order_id AS erp_id, mindbox_status AS status
FROM online_order_mindbox_state
WHERE mindbox_status IN ('Queued', 'Sent', 'DataError', 'Error');

-- 2. Сброс статусов контрагентов.
UPDATE mindbox_counterparty_state
SET status = 'Unknown',
    transaction_id = NULL,
    attempts = 0,
    last_attempt_at = NULL,
    last_error = NULL,
    last_http_status_code = NULL,
    last_mindbox_status = NULL,
    last_error_id = NULL,
    last_attempt_duration_ms = NULL,
    sent_at = NULL,
    updated_at = UTC_TIMESTAMP(6);

-- 3. Сброс статусов онлайн-заказов.
UPDATE online_order_mindbox_state
SET mindbox_status = 'Unknown',
    mindbox_order_id = NULL,
    mindbox_transaction_id = NULL,
    mindbox_last_error = NULL,
    mindbox_last_attempt_at = NULL,
    mindbox_attempts = 0,
    mindbox_last_http_status_code = NULL,
    mindbox_last_status = NULL,
    mindbox_last_error_id = NULL,
    mindbox_snapshot = NULL,
    mindbox_snapshot_updated_at = NULL,
    mindbox_last_change_at = NULL,
    mindbox_sync_requested_at = NULL;
