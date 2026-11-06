-- Точка контакта для контрагентов, созданных в ERP.
-- Для МП и сайта используется значение CounterpartyFrom (54/55/61), для ERP — эта настройка.
INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.PointOfContactErp', '1', '00:05:00', 'Идентификатор точки контакта для контрагентов, созданных в ERP'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.PointOfContactErp');

UPDATE `base_parameters`
SET `str_value` = '1'
WHERE `name` = 'Mindbox.PointOfContactErp' AND (`str_value` IS NULL OR `str_value` = '');
