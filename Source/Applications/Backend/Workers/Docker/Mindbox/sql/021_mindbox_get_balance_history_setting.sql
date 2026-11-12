-- Настройка операции получения истории баланса Mindbox.
INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.GetBalanceHistoryOperation', 'GetBalanceHistory', '00:05:00', 'Название операции Mindbox «Получение истории баланса»'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.GetBalanceHistoryOperation');
