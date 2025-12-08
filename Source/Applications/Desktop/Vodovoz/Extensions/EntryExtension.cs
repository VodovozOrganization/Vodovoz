using System;
using System.Linq;
using Gtk;

namespace Vodovoz.Extensions
{
	public static class EntryExtensions
	{
		public static void SetNumericValidation(this Entry entry, decimal maxValue)
		{
			// Обработчик отписки от событий
			FocusOutEventHandler focusOutHandler = null;

			// Основной обработчик Changed события (срабатывает при каждом символе)
			EventHandler changedHandler = (s, e) =>
			{
				var text = entry.Text;

				// Точку → запятую (русская локаль)
				var normalized = text.Replace('.', ',');

				// Только цифры + запятая (удаляем буквы и другие символы)
				var sanitized = new string(normalized.Where(c => char.IsDigit(c) || c == ',').ToArray());

				// Если начинается с запятой → добавляем "0"
				if(sanitized.Length > 0 && sanitized[0] == ',')
				{
					sanitized = "0" + sanitized;
				}

				// Максимум 2 цифры после первой запятой
				var parts = sanitized.Split(',');
				if(parts.Length > 1)
				{
					var integerPart = parts[0]; // Целая часть
					// Берем максимум 2 цифры из всех частей после запятой
					var decimalPart = new string(parts.Skip(1).SelectMany(p => p.Where(char.IsDigit)).Take(2).ToArray());
					sanitized = integerPart + "," + decimalPart;
				}

				// Убираем лидирующие нули только в целой части
				var commaIndex = sanitized.IndexOf(',');
				if(commaIndex > 0)
				{
					var integerPart = sanitized.Substring(0, commaIndex).TrimStart('0');
					
					if(string.IsNullOrEmpty(integerPart))
					{
						integerPart = "0";
					}

					sanitized = integerPart + sanitized.Substring(commaIndex);
				}
				else
				{
					// Для целых чисел
					sanitized = sanitized.TrimStart('0');
					
					if(string.IsNullOrEmpty(sanitized))
					{
						sanitized = "0";
					}
				}

				// Ограничение длины по maxValue
				var maxLength = maxValue.ToString("F2").Replace('.', ',').Length;
				
				if(sanitized.Length > maxLength)
				{
					sanitized = sanitized.Substring(0, maxLength);
				}

				// Проверка превышения максимального значения
				if(decimal.TryParse(sanitized.Replace(',', '.'), out var value) && value > maxValue)
				{
					sanitized = maxValue.ToString("F2").Replace('.', ',');
				}

				// Применяем изменения только если текст изменился
				if(sanitized != text)
				{
					entry.Text = sanitized;
				}
			};

			// Отписка от событий при потере фокуса (FocusOut)
			focusOutHandler = (o, ev) =>
			{
				entry.Changed -= changedHandler;
				entry.FocusOutEvent -= focusOutHandler;
				ev.RetVal = true; // Событие обработано
			};

			// Подписываемся на события
			entry.Changed += changedHandler;
			entry.FocusOutEvent += focusOutHandler;
		}
	}
}
