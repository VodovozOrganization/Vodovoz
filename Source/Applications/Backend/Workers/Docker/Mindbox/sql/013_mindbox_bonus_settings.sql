-- Настройки расчёта бонусов Mindbox для цен и остатков мобильного приложения.
-- Скрипт идемпотентен: повторный запуск не создаёт дубликаты настроек.
-- Секретный ключ и значения, которые выдаёт Mindbox, заполняются вручную.

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.BonusCalculationEnabled', 'false', '00:05:00', 'Включён ли расчёт бонусов Mindbox в ценах МП'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.BonusCalculationEnabled');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.CalculateProductListOperation', 'CalculateProductList', '00:05:00', 'Название операции Mindbox «Расчёт цен в каталоге и на карточке продукта»'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.CalculateProductListOperation');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.BonusItemIdName', 'itemErpId', '00:05:00', 'Имя идентификатора товара в запросе к Mindbox (выдаёт Mindbox)'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.BonusItemIdName');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.BonusBalanceSystemName', '', '00:05:00', 'Системное имя балльного счёта, по которому берутся бонусы (выдаёт Mindbox)'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.BonusBalanceSystemName');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.BonusBatchSize', '100', '00:05:00', 'Размер пачки позиций при расчёте бонусов'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.BonusBatchSize');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.BonusMaxParallelBatches', '5', '00:05:00', 'Максимальное количество одновременно отправляемых пачек бонусов (K)'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.BonusMaxParallelBatches');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.BonusBatchTimeoutSeconds', '2', '00:05:00', 'Таймаут одной пачки при расчёте бонусов, секунды'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.BonusBatchTimeoutSeconds');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.BonusTotalTimeoutMilliseconds', '2500', '00:05:00', 'Общий лимит ожидания Mindbox в методе цен, миллисекунды'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.BonusTotalTimeoutMilliseconds');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.BonusErrorsBeforePause', '5', '00:05:00', 'Количество подряд идущих ошибок 5xx/429/таймаутов до паузы (N)'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.BonusErrorsBeforePause');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.BonusPauseMinutes', '1', '00:05:00', 'Длительность паузы вызовов Mindbox при расчёте бонусов, минуты (M)'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.BonusPauseMinutes');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.BonusCacheSeconds', '0', '00:05:00', 'Время кеширования ответа Mindbox по ключу товара и базовой цене, секунды. 0 — выключен'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.BonusCacheSeconds');

-- Значения по умолчанию для уже существующих, но пустых настроек.
UPDATE `base_parameters`
SET `str_value` = 'CalculateProductList'
WHERE `name` = 'Mindbox.CalculateProductListOperation' AND (`str_value` IS NULL OR `str_value` = '');

UPDATE `base_parameters`
SET `str_value` = 'itemErpId'
WHERE `name` = 'Mindbox.BonusItemIdName' AND (`str_value` IS NULL OR `str_value` = '');

UPDATE `base_parameters`
SET `str_value` = '100'
WHERE `name` = 'Mindbox.BonusBatchSize' AND (`str_value` IS NULL OR `str_value` = '');

UPDATE `base_parameters`
SET `str_value` = '5'
WHERE `name` = 'Mindbox.BonusMaxParallelBatches' AND (`str_value` IS NULL OR `str_value` = '');

UPDATE `base_parameters`
SET `str_value` = '2'
WHERE `name` = 'Mindbox.BonusBatchTimeoutSeconds' AND (`str_value` IS NULL OR `str_value` = '');

UPDATE `base_parameters`
SET `str_value` = '2500'
WHERE `name` = 'Mindbox.BonusTotalTimeoutMilliseconds' AND (`str_value` IS NULL OR `str_value` = '');

UPDATE `base_parameters`
SET `str_value` = '5'
WHERE `name` = 'Mindbox.BonusErrorsBeforePause' AND (`str_value` IS NULL OR `str_value` = '');

UPDATE `base_parameters`
SET `str_value` = '1'
WHERE `name` = 'Mindbox.BonusPauseMinutes' AND (`str_value` IS NULL OR `str_value` = '');

UPDATE `base_parameters`
SET `str_value` = '0'
WHERE `name` = 'Mindbox.BonusCacheSeconds' AND (`str_value` IS NULL OR `str_value` = '');
