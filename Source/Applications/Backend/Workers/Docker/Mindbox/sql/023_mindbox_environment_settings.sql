-- Настройки контура Mindbox.
INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.Environment', 'Prod', '00:05:00', 'Контур Mindbox: Test или Prod'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.Environment');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.TechnicalClientId', '11', '00:05:00', 'Технический клиент Mindbox для расчёта бонусов в каталоге'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.TechnicalClientId');
