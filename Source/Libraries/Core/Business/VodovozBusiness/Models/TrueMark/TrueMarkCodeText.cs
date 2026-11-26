using System.Text.RegularExpressions;

namespace Vodovoz.Models.TrueMark
{
	/// <summary>
	/// Общие константы и нормализация текста кодов маркировки «Честный знак»,
	/// используемые всеми парсерами товарных групп.
	/// <para/>
	/// Разделитель групп FNC1 (ASCII 29, \u001d) и признак символики GS1 DataMatrix (ASCII 232, \u00e8)
	/// могут приходить как реальные управляющие символы либо как экранированный текст "\u001d" / "\u00e8".
	/// Нормализация приводит их к реальным символам, чтобы в регулярных выражениях
	/// не дублировать оба варианта.
	/// </summary>
	internal static class TrueMarkCodeText
	{
		/// <summary>Реальный разделитель групп FNC1 (ASCII 29).</summary>
		public const char GroupSeparator = '\u001d';

		/// <summary>Реальный признак символики GS1 DataMatrix FNC1 (ASCII 232).</summary>
		public const char Fnc1Symbol = '\u00e8';

		/// <summary>Разделитель групп в каноническом строковом виде (6 символов: \u001d), в котором он хранится в SourceCode.</summary>
		public const string GroupSeparatorEscaped = "\\u001d";

		/// <summary>Признак FNC1 в каноническом строковом виде (6 символов: \u00e8), в котором он хранится в SourceCode.</summary>
		public const string Fnc1SymbolEscaped = "\\u00e8";

		/// <summary>Фрагмент regex: один печатный символ (без управляющих, включая GS, CR и LF).</summary>
		public const string TextChar = @"[^\u0000-\u001F\u007F]";

		/// <summary>Фрагмент regex: необязательный признак FNC1 в начале кода.</summary>
		public const string LeadingFnc1 = @"[\u00e8\u001d]";

		private static readonly char[] _edgeChars = { ' ', '\t', '\r', '\n' };

		private static readonly Regex _excelSeparatorRegex = new Regex(
			"_x001d_",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		/// <summary>
		/// Убирает пробелы, табуляции и переводы строк по краям (сканеры и CSV часто добавляют CR/LF).
		/// Управляющие символы FNC1 не затрагиваются.
		/// </summary>
		public static string Prepare(string rawCode)
		{
			return rawCode == null ? null : rawCode.Trim(_edgeChars);
		}

		/// <summary>
		/// Заменяет экранированный текст "\u001d" и "\u00e8" на реальные символы.
		/// </summary>
		public static string UnescapeSeparators(string text)
		{
			return text
				.Replace("\\u001d", "\u001d")
				.Replace("\\u001D", "\u001d")
				.Replace("\\u00e8", "\u00e8")
				.Replace("\\u00E8", "\u00e8");
		}

		/// <summary>
		/// Подготовка строки для стандартного разбора: обрезка краёв и приведение разделителей к реальным символам.
		/// </summary>
		public static string PrepareStandard(string rawCode)
		{
			return UnescapeSeparators(Prepare(rawCode));
		}

		/// <summary>
		/// Подготовка строки, выгруженной из 1С/CSV/Excel: снимает внешние кавычки, раскрывает удвоенные кавычки,
		/// заменяет маркеры разделителя (_x001d_ в любом регистре, |ГС|) и экранированный текст на реальный GS.
		/// </summary>
		public static string PrepareFrom1c(string rawCode)
		{
			var text = Prepare(rawCode);

			if(text.Length >= 2 && text[0] == '"' && text[text.Length - 1] == '"')
			{
				text = text.Substring(1, text.Length - 2);
			}

			text = text.Replace("\"\"", "\"");
			text = _excelSeparatorRegex.Replace(text, "\u001d");
			text = text.Replace("|ГС|", "\u001d");

			return UnescapeSeparators(Prepare(text));
		}
	}
}
