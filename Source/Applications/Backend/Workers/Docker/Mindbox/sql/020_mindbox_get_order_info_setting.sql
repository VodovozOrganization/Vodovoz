-- Настройка операции получения информации о заказе Mindbox.
INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.GetOrderInfoOperation', 'GetOrderInfo', '00:05:00', 'Название операции Mindbox «Получение информации о заказе»'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.GetOrderInfoOperation');
