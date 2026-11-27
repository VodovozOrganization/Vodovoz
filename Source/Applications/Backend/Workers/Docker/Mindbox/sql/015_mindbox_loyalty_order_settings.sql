-- Настройки создания заказа с бонусами в Mindbox.
INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.CreateAuthorizedLoyaltyOrderOperation', 'CreateAuthorizedLoyaltyOrder', '00:05:00', 'Название операции Mindbox «Создание заказа с бонусами»'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.CreateAuthorizedLoyaltyOrderOperation');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.AuthorizedLoyaltyOrderTimeoutSeconds', '10', '00:05:00', 'Таймаут CreateAuthorizedLoyaltyOrder в секундах'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.AuthorizedLoyaltyOrderTimeoutSeconds');

INSERT INTO `base_parameters` (`name`, `str_value`, `cache_timeout`, `description`)
SELECT 'Mindbox.MindboxOrderIdName', 'externalID', '00:05:00', 'Имя идентификатора заказа в Mindbox: externalID на проде, erptestOrderID на тесте'
WHERE NOT EXISTS (SELECT 1 FROM `base_parameters` WHERE `name` = 'Mindbox.MindboxOrderIdName');

UPDATE `base_parameters`
SET `str_value` = 'CreateAuthorizedLoyaltyOrder'
WHERE `name` = 'Mindbox.CreateAuthorizedLoyaltyOrderOperation' AND (`str_value` IS NULL OR `str_value` = '');

UPDATE `base_parameters`
SET `str_value` = '10'
WHERE `name` = 'Mindbox.AuthorizedLoyaltyOrderTimeoutSeconds' AND (`str_value` IS NULL OR `str_value` = '');

UPDATE `base_parameters`
SET `str_value` = 'externalID'
WHERE `name` = 'Mindbox.MindboxOrderIdName' AND (`str_value` IS NULL OR `str_value` = '');
