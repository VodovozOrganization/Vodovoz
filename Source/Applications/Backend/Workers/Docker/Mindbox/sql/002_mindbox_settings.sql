-- Настройки интеграции с Mindbox
INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.IsEnabled', 'false', '00:05:00', 'Включить интеграцию с Mindbox'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.IsEnabled');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.BaseUrl', 'https://api.mindbox.ru', '00:05:00', 'Базовый адрес API Mindbox'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.BaseUrl');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.SecretKey', '', '00:05:00', 'Секретный ключ проекта Mindbox'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.SecretKey');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.EndpointId', '', '00:05:00', 'Идентификатор точки доступа Mindbox'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.EndpointId');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.CreateCounterpartyOperation', '', '00:05:00', 'Системное имя операции создания контрагента'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.CreateCounterpartyOperation');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.CreateCounterpartyOperationType', 'async', '00:05:00', 'Тип операции создания контрагента: sync или async'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.CreateCounterpartyOperationType');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.ImportOperation', 'DirectCrm.Customers.Import', '00:05:00', 'Операция массового импорта клиентов'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.ImportOperation');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.ImportSegment', '', '00:05:00', 'Сегмент для массового импорта'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.ImportSegment');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.ImportSourceActionTemplate', '', '00:05:00', 'Действие регистрации клиента для массового импорта'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.ImportSourceActionTemplate');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.ImportBatchSize', '20000', '00:05:00', 'Размер пакета строк CSV'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.ImportBatchSize');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.SendBatchSize', '50', '00:05:00', 'Сообщений очереди за цикл воркера'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.SendBatchSize');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.MaxSendAttempts', '10', '00:05:00', 'Максимум попыток отправки'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.MaxSendAttempts');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.RetryDelaySeconds', '60', '00:05:00', 'Базовая задержка повторной отправки, секунды'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.RetryDelaySeconds');
