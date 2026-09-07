-- 5674: кассир в версиях организации
-- Применить вручную на dev/stage.
-- Перед апдейтом трёх орг убедиться, что сотрудники есть:
--   SELECT id, last_name, name, patronymic FROM employees
--   WHERE (last_name, name, patronymic) IN (
--     ('Васильева','Олеся','Владимировна'),
--     ('Фахрутдинова','Наталья','Владимировна'),
--     ('Герасимова','Наталья','Петровна')
--   );

ALTER TABLE organization_versions
	ADD COLUMN cashier_id INT NULL AFTER accountant_id;

-- По умолчанию кассир = руководитель (все версии, в т.ч. старые)
UPDATE organization_versions
SET cashier_id = leader_id
WHERE cashier_id IS NULL AND leader_id IS NOT NULL;

-- Текущие версии: кассиры по UPD (активные — без end_date или end_date в будущем)
UPDATE organization_versions ov
INNER JOIN organizations o ON o.id = ov.organization_id
INNER JOIN employees e ON e.last_name = 'Васильева'
	AND e.name = 'Олеся'
	AND e.patronymic = 'Владимировна'
SET ov.cashier_id = e.id
WHERE (o.name LIKE '%Веселый Водовоз Юг%' OR o.full_name LIKE '%Веселый Водовоз Юг%')
	AND (ov.end_date IS NULL OR ov.end_date >= NOW());

UPDATE organization_versions ov
INNER JOIN organizations o ON o.id = ov.organization_id
INNER JOIN employees e ON e.last_name = 'Фахрутдинова'
	AND e.name = 'Наталья'
	AND e.patronymic = 'Владимировна'
SET ov.cashier_id = e.id
WHERE (o.name LIKE '%Веселый Водовоз Восток%' OR o.full_name LIKE '%Веселый Водовоз Восток%')
	AND (ov.end_date IS NULL OR ov.end_date >= NOW());

UPDATE organization_versions ov
INNER JOIN organizations o ON o.id = ov.organization_id
INNER JOIN employees e ON e.last_name = 'Герасимова'
	AND e.name = 'Наталья'
	AND e.patronymic = 'Петровна'
SET ov.cashier_id = e.id
WHERE (o.name LIKE '%Губернаторов%' OR o.full_name LIKE '%Губернаторов%')
	AND (ov.end_date IS NULL OR ov.end_date >= NOW());

-- Проверка:
-- SELECT o.name, ov.id, ov.start_date, ov.end_date,
--   CONCAT_WS(' ', el.last_name, el.name, el.patronymic) AS leader,
--   CONCAT_WS(' ', ec.last_name, ec.name, ec.patronymic) AS cashier
-- FROM organization_versions ov
-- JOIN organizations o ON o.id = ov.organization_id
-- LEFT JOIN employees el ON el.id = ov.leader_id
-- LEFT JOIN employees ec ON ec.id = ov.cashier_id
-- WHERE ov.end_date IS NULL OR ov.end_date >= NOW()
-- ORDER BY o.name;
