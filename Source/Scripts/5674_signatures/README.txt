-- Как применить факсимиле кассиров (5674)

-- 1) Колонки без сида (быстро):
--    Source/Scripts/5674_signature_cashier_columns_only.sql

-- 2) Полный сид JPEG из PDF Васильевой / Фахрутдиновой / Герасимовой (~480 KB):
--    Source/Scripts/5674_organization_version_signature_cashier.sql
--
-- Перед INSERT проверьте enum:
--   SELECT DISTINCT type, image_type FROM stored_resource LIMIT 20;
-- Если type/image_type строковые ('Image'/'Signature') —
-- в INSERT замените , 0, 0 на , 'Image', 'Signature'.

-- Исходные PDF лежат у постановки; JPEG извлечены в:
--   Source/Scripts/5674_signatures/*.jpg
