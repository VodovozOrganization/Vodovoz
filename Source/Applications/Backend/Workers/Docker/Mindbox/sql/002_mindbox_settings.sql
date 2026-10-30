-- Настройки интеграции с Mindbox
INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.IsEnabled', 'false', '00:05:00', 'Включить отправку контрагентов в Mindbox'
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
SELECT 'Mindbox.CreateCustomerOperation', '', '00:05:00', 'Системное имя операции создания клиента'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.CreateCustomerOperation');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.CreateCustomerOperationType', 'sync', '00:05:00', 'Тип операции создания клиента: sync или async'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.CreateCustomerOperationType');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.TimeoutSeconds', '10', '00:05:00', 'Таймаут ответа Mindbox, секунды'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.TimeoutSeconds');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.SendBatchSize', '50', '00:05:00', 'Сообщений очереди за цикл воркера'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.SendBatchSize');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.EnqueueBatchSize', '1000', '00:05:00', 'Новых контрагентов в очередь за цикл'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.EnqueueBatchSize');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.RetryScheduleMinutes', '1,5,15,60', '00:05:00', 'Расписание повторов в минутах'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.RetryScheduleMinutes');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.RetryIntervalMinutes', '60', '00:05:00', 'Интервал повторов после расписания, минуты'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.RetryIntervalMinutes');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.RetryMaxHours', '24', '00:05:00', 'Максимальное время повторов, часы'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.RetryMaxHours');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.PauseMinutes', '5', '00:05:00', 'Пауза очереди при недоступности Mindbox, минуты'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.PauseMinutes');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.TransientErrorsBeforePause', '3', '00:05:00', 'Временных ошибок подряд до паузы очереди'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.TransientErrorsBeforePause');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.UnavailableAlertMinutes', '15', '00:05:00', 'Минут недоступности Mindbox до алерта'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.UnavailableAlertMinutes');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.LastEnqueuedCounterpartyId', '0', '00:01:00', 'Последний контрагент, добавленный в очередь Mindbox'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.LastEnqueuedCounterpartyId');
