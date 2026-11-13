-- Настройки синхронизации статусов и состава заказов Mindbox.
INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.UpdateOrderLinesStatusOperation', 'UpdateOrderLinesStatus', '00:05:00', 'Название операции Mindbox «Изменение статуса позиций заказа»'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.UpdateOrderLinesStatusOperation');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.OrderLineStatusMapping',
       '{"OrderPerformed":"Created","WaitingForPayment":"WaitingForPayment","OrderCollecting":"OrderCollecting","OrderDelivering":"OrderDelivering","OrderCompleted":"OrderCompleted","Canceled":"Canceled","Paid":"Paid"}',
       '00:05:00',
       'JSON-маппинг статусов онлайн-заказа ERP в статусы позиций Mindbox'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.OrderLineStatusMapping');
