-- =============================================================================
-- 5674: откат тестового process корректировки на dev (ручной запуск).
-- По умолчанию: process_id = 8, edo_fiscal_document_id = 288359, order_id = 5181293.
-- Перед DELETE проверь SELECT-ами внизу.
-- =============================================================================

SET @process_id = 8;
SET @edo_doc_id = 288359;
SET @order_id = 5181293;

-- --- Проверка перед удалением ---
-- SELECT * FROM receipt_correction_processes WHERE id = @process_id;
-- SELECT * FROM receipt_correction_process_documents WHERE receipt_correction_process_id = @process_id;
-- SELECT id, document_type, stage, receipt_edo_task_id FROM edo_fiscal_documents WHERE id = @edo_doc_id;

START TRANSACTION;

DELETE FROM edo_fiscal_invent_positions_order_items
WHERE edo_fiscal_invent_position_id IN (
	SELECT id FROM edo_fiscal_invent_positions WHERE edo_fiscal_document_id = @edo_doc_id
);

DELETE FROM edo_fiscal_money_positions
WHERE edo_fiscal_document_id = @edo_doc_id;

DELETE FROM edo_fiscal_invent_positions
WHERE edo_fiscal_document_id = @edo_doc_id;

DELETE FROM receipt_correction_explanatory_notes
WHERE receipt_correction_process_id = @process_id;

UPDATE receipt_correction_process_documents
SET edo_fiscal_document_id = NULL
WHERE receipt_correction_process_id = @process_id;

DELETE FROM edo_fiscal_documents
WHERE id = @edo_doc_id;

DELETE FROM receipt_correction_process_documents
WHERE receipt_correction_process_id = @process_id;

DELETE FROM receipt_correction_processes
WHERE id = @process_id;

-- Заказ 5181293 после полной отмены в статусе «Отменён».
-- Для повторного теста полной отмены на ЭТОМ же заказе — верни статус «Доставлен» (8):
-- UPDATE orders SET order_status = 8 WHERE id = @order_id AND order_status = 0;

COMMIT;

-- --- После отката ---
-- SELECT * FROM receipt_correction_processes WHERE order_id = @order_id;
-- SELECT id, document_type, stage FROM edo_fiscal_documents
-- WHERE receipt_edo_task_id = (SELECT receipt_edo_task_id FROM edo_fiscal_documents WHERE id = 288315);
