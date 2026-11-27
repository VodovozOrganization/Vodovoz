using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.Core.Domain.Interfaces.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	/// <summary>
	/// Фасад парсинга кодов маркировки «Честный знак».
	/// Перебирает парсеры товарных групп и возвращает первый успешный результат.
	/// Новая товарная группа подключается регистрацией парсера, без изменения фасада.
	/// </summary>
	public class TrueMarkCodeParser
	{
		private static readonly Regex _transportCodeRegex = new Regex(
			@"^((00\d{26})|(\d{26}))$",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private readonly IReadOnlyList<ITrueMarkProductCodeParser> _parsers;

		/// <summary>
		/// Парсеры товарных групп.
		/// </summary>
		public TrueMarkCodeParser(IEnumerable<ITrueMarkProductCodeParser> parsers)
		{
			if(parsers == null)
			{
				throw new ArgumentNullException(nameof(parsers));
			}

			var parsersList = parsers.ToList();

			if(parsersList.Any(x => x == null))
			{
				throw new ArgumentException("Список парсеров не должен содержать null.", nameof(parsers));
			}

			_parsers = parsersList;
		}

		/// <summary>
		/// Пытается распарсить код: сначала в стандартном формате всеми парсерами,
		/// после чего пробует ручной ввод без разделителей.
		/// </summary>
		public bool TryParse(string rawCode, out ITrueMarkCode result)
		{
			result = null;

			if(string.IsNullOrWhiteSpace(rawCode))
			{
				return false;
			}

			foreach(var parser in _parsers)
			{
				if(parser.TryParse(rawCode, out result))
				{
					return true;
				}
			}

			foreach(var parser in _parsers)
			{
				if(parser.TryParseManual(rawCode, out result))
				{
					return true;
				}
			}

			result = null;
			return false;
		}

		/// <summary>
		/// Пытается распарсить код из 1С/CSV: сначала по парсерам товарных групп,
		/// с fallback на обычный и скобочный формат.
		/// </summary>
		public bool TryParseCodeFrom1c(string rawCode, out ITrueMarkCode result)
		{
			result = null;

			if(string.IsNullOrWhiteSpace(rawCode))
			{
				return false;
			}

			foreach(var parser in _parsers)
			{
				if(parser.TryParseCodeFrom1c(rawCode, out result))
				{
					return true;
				}
			}

			return TryParse(rawCode, out result);
		}

		/// <summary>
		/// Нечёткий разбор кода маркировки для поиска и фильтрации.
		/// </summary>
		public bool FuzzyParse(string input, out ITrueMarkCode result)
		{
			result = null;

			if(string.IsNullOrWhiteSpace(input))
			{
				return false;
			}

			foreach(var parser in _parsers)
			{
				if(parser.FuzzyParse(input, out result))
				{
					return true;
				}
			}

			result = null;
			return false;
		}

		/// <summary>
		/// Проверяет, является ли код групповым транспортным кодом (КИТУ, 26 цифр).
		/// </summary>
		public bool IsTransportCode(string rawCode)
		{
			return !string.IsNullOrWhiteSpace(rawCode) && _transportCodeRegex.IsMatch(rawCode);
		}
	}
}
