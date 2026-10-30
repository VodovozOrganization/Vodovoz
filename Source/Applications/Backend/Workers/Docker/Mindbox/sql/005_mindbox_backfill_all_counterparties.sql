-- Принудительная постановка в очередь всех существующих контрагентов.
-- Выполнять один раз, если интеграция уже устанавливалась и указатель
-- Mindbox.LastEnqueuedCounterpartyId был заполнен максимальным id.
-- Повторный проход безопасен: контрагенты с записью в очереди пропускаются.
UPDATE `base_parameters`
SET `str_value` = '0'
WHERE `name` = 'Mindbox.LastEnqueuedCounterpartyId';
