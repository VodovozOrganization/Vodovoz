-- Настройки расчёта бонусов при оформлении заказа в мобильном приложении (CalculateAuthorizedCart).
-- Скрипт идемпотентен: повторный запуск не создаёт дубликаты настроек.
-- Секретный ключ и endpointId общие с остальной интеграцией Mindbox и задаются в 002_mindbox_settings.sql.

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.CalculateAuthorizedCartOperation', 'CalculateAuthorizedCart', '00:05:00', 'Название операции Mindbox «Расчёт баланса и списания бонусов при оформлении заказа»'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.CalculateAuthorizedCartOperation');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.AuthorizedCartTimeoutSeconds', '3', '00:05:00', 'Таймаут запроса CalculateAuthorizedCart в секундах'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.AuthorizedCartTimeoutSeconds');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.BonusProgramMode', 'Disabled', '00:05:00', 'Режим бонусной программы: Disabled - выключена, Clients - по списку клиентов, All - для всех'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.BonusProgramMode');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.BonusProgramClientIds', '', '00:05:00', 'Идентификаторы клиентов ERP, для которых включена бонусная программа, через запятую'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.BonusProgramClientIds');

-- Защита от пустых значений, если настройка уже была создана вручную.
UPDATE `base_parameters`
SET `str_value` = 'CalculateAuthorizedCart'
WHERE `name` = 'Mindbox.CalculateAuthorizedCartOperation' AND (`str_value` IS NULL OR `str_value` = '');

UPDATE `base_parameters`
SET `str_value` = '3'
WHERE `name` = 'Mindbox.AuthorizedCartTimeoutSeconds' AND (`str_value` IS NULL OR `str_value` = '');

UPDATE `base_parameters`
SET `str_value` = 'Disabled'
WHERE `name` = 'Mindbox.BonusProgramMode' AND (`str_value` IS NULL OR `str_value` = '');
