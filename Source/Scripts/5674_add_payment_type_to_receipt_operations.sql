-- 5674: форма оплаты в фискальной операции (для SALE_CORRECTION при смене оплаты в рамках одной организации)
-- Применить вручную, если receipt_operations уже создана без payment_type.

ALTER TABLE receipt_operations
	ADD COLUMN payment_type INT NULL AFTER cashbox_id;
