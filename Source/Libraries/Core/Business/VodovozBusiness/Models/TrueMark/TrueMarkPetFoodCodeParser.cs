using TrueMark.Contracts;
using System.Text.RegularExpressions;
using Vodovoz.Core.Domain.Interfaces.TrueMark;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	/// <summary>
	/// Парсер кода маркировки «Честный знак» для кормов для животных.
	/// Код идентификации формируется по шаблону: <![CDATA[01<GTIN>21<SerialNumber>]]>, где:
	/// <list type="bullet">
	/// <item>GTIN — код товара, 14 цифр (AI 01);</item>
	/// <item>SerialNumber — серийный номер, 6 символов: 1 цифра кода страны-члена ЕАЭС + 5 символов (AI 21).</item>
	/// </list>
	/// <para/>
	/// Чек-код (код проверки) может иметь одну из двух структур:
	/// <list type="bullet">
	/// <item>короткая (30 символов): <![CDATA[93<4 символа>]]>;</item>
	/// <item>длинная (76 символов): <![CDATA[91<4 символа ключа проверки>92<44 символа значения проверки>]]>.</item>
	/// </list>
	/// Между группами данных может присутствовать разделитель FNC1 (ASCII 29, \u001d).
	/// </summary>
	public class TrueMarkPetFoodCodeParser : ITrueMarkProductCodeParser
	{
		private const string _gtinGroupName = "GTIN";
		private const string _serialGroupName = "SerialNumber";
		private const string _keyCheckIdGroupName = "KeyCheckId";
		private const string _checkValueGroupName = "CheckValue";
		private const string _checkCodeGroupName = "CheckCode";

		// Все шаблоны работают со строкой после нормализации (см. TrueMarkCodeText): экранированный текст
		// "\u001d"/"\u00e8" к этому моменту уже заменён на реальные символы.
		private const string _text = TrueMarkCodeText.TextChar;

		// Необязательный признак FNC1 в начале и необязательный разделитель групп.
		private const string _leadingFnc1 = TrueMarkCodeText.LeadingFnc1 + "?";
		private const string _groupSeparator = @"\u001d?";

		// Серийный номер корма ровно 6 печатных символов GS1 (1 цифра страны ЕАЭС + 5 знаков).
		private const string _serialPattern = _text + "{6}";

		// Ключ проверки 4 символа, значение проверки 44 символа.
		private const string _keyCheckPattern = _text + "{4}";
		private const string _checkValuePattern = _text + "{44}";
		private const string _shortCheckPattern = _text + "{4}";

		// Длинная структура: 01 + GTIN(14) + 21 + серийник(6) + 91 + ключ проверки(4) + 92 + значение проверки(44)
		private static readonly Regex _longPattern = new Regex(
			$@"^{_leadingFnc1}01(?<{_gtinGroupName}>\d{{14}})21(?<{_serialGroupName}>{_serialPattern}){_groupSeparator}91(?<{_keyCheckIdGroupName}>{_keyCheckPattern}){_groupSeparator}92(?<{_checkValueGroupName}>{_checkValuePattern})\z",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		// Короткая структура: 01 + GTIN(14) + 21 + серийник(6) + 93 + код проверки(4)
		private static readonly Regex _shortPattern = new Regex(
			$@"^{_leadingFnc1}01(?<{_gtinGroupName}>\d{{14}})21(?<{_serialGroupName}>{_serialPattern}){_groupSeparator}93(?<{_checkCodeGroupName}>{_shortCheckPattern})\z",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		// Скобочный длинный формат: (01)...(21)...(91)...(92)...
		private static readonly Regex _altLongPattern = new Regex(
			$@"^\(01\)(?<{_gtinGroupName}>\d{{14}})\(21\)(?<{_serialGroupName}>[^\(\)]{{6}})\(91\)(?<{_keyCheckIdGroupName}>[^\(\)]{{4}})\(92\)(?<{_checkValueGroupName}>[^\(\)]{{44}})\z",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		// Скобочный короткий формат: (01)...(21)...(93)...
		private static readonly Regex _altShortPattern = new Regex(
			$@"^\(01\)(?<{_gtinGroupName}>\d{{14}})\(21\)(?<{_serialGroupName}>[^\(\)]{{6}})\(93\)(?<{_checkCodeGroupName}>[^\(\)]{{4}})\z",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex _fuzzyIdentificationPattern = new Regex(
			$@"^{_leadingFnc1}01(?<{_gtinGroupName}>\d{{14}})21(?<{_serialGroupName}>{_serialPattern})(?:{_groupSeparator}(?:91(?<{_keyCheckIdGroupName}>{_keyCheckPattern}){_groupSeparator}92(?<{_checkValueGroupName}>{_checkValuePattern})|93(?<{_checkCodeGroupName}>{_shortCheckPattern})))?\z",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		// Скобочный формат со скобкой у каждого AI: (01)…(21)…(91)…(92)… или (01)…(21)…(93)…
		private static readonly Regex _fuzzyAltIdentificationPattern = new Regex(
			$@"^\(01\)(?<{_gtinGroupName}>\d{{14}})\(21\)(?<{_serialGroupName}>[^\(\)]{{6}})(?:\(91\)(?<{_keyCheckIdGroupName}>[^\(\)]{{4}})\(92\)(?<{_checkValueGroupName}>[^\(\)]{{44}})|\(93\)(?<{_checkCodeGroupName}>[^\(\)]{{4}}))?\z",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		// «Голый» серийный номер корма: 6 печатных символов, первый — цифра кода страны-члена ЕАЭС.
		// Ограничение первой цифрой отсекает произвольные 6-символьные строки (номера заказов, артикулы).
		private static readonly Regex _pureSerialPattern = new Regex(
			@"^[0-9][\x21-\x7E]{5}\z",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		/// <summary>
		/// Безопасный разбор кода маркировки кормов для животных из 1С/CSV.
		/// Разделители (_x001d_ в любом регистре, |ГС|) приводятся к реальному GS.
		/// </summary>
		public bool TryParseCodeFrom1c(string code, out ITrueMarkCode result)
		{
			result = null;

			if(string.IsNullOrWhiteSpace(code))
			{
				return false;
			}

			return TryParse(TrueMarkCodeText.PrepareFrom1c(code), out result);
		}

		/// <summary>
		/// Безопасный разбор кода маркировки кормов для животных без выбрасывания исключений.
		/// </summary>
		public bool TryParse(string rawCode, out ITrueMarkCode result)
		{
			result = null;

			if(string.IsNullOrWhiteSpace(rawCode))
			{
				return false;
			}

			var text = TrueMarkCodeText.PrepareStandard(rawCode);

			var match = _longPattern.Match(text);
			if(match.Success)
			{
				result = CreateCode(match, isLongStructure: true);
				return true;
			}

			match = _shortPattern.Match(text);
			if(match.Success)
			{
				result = CreateCode(match, isLongStructure: false);
				return true;
			}

			match = _altLongPattern.Match(text);
			if(match.Success)
			{
				result = CreateCode(match, isLongStructure: true);
				return true;
			}

			match = _altShortPattern.Match(text);
			if(match.Success)
			{
				result = CreateCode(match, isLongStructure: false);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Разбор кода, введённого вручную. Для кормов не поддерживается.
		/// </summary>
		public bool TryParseManual(string rawCode, out ITrueMarkCode result)
		{
			result = null;
			return false;
		}

		/// <summary>
		/// Нечёткий разбор кода маркировки кормов для животных для поиска и фильтрации.
		/// Поддерживает полный код, КИ без криптохвоста и чистый 6-символьный серийный номер.
		/// </summary>
		public bool FuzzyParse(string input, out ITrueMarkCode result)
		{
			result = null;

			if(string.IsNullOrWhiteSpace(input))
			{
				return false;
			}

			var trimmed = TrueMarkCodeText.Prepare(input);

			if(TryParse(trimmed, out result))
			{
				return true;
			}

			var normalized = TrueMarkCodeText.UnescapeSeparators(trimmed);

			var match = _fuzzyIdentificationPattern.Match(normalized);
			if(!match.Success)
			{
				match = _fuzzyAltIdentificationPattern.Match(normalized);
			}

			if(match.Success)
			{
				var gtin = match.Groups[_gtinGroupName].Value;
				var serialNumber = match.Groups[_serialGroupName].Value;
				string checkCode = null;

				if(match.Groups[_keyCheckIdGroupName].Success && match.Groups[_checkValueGroupName].Success)
				{
					checkCode = $"91{match.Groups[_keyCheckIdGroupName].Value}\u001d92{match.Groups[_checkValueGroupName].Value}";
				}
				else if(match.Groups[_checkCodeGroupName].Success)
				{
					checkCode = $"93{match.Groups[_checkCodeGroupName].Value}";
				}

				result = new TrueMarkCode
				{
					SourceCode = trimmed,
					Gtin = gtin,
					SerialNumber = serialNumber,
					CheckCode = checkCode,
					ProductGroup = TrueMarkProductGroup.PetFood
				};
				return true;
			}

			if(_pureSerialPattern.IsMatch(trimmed))
			{
				result = new TrueMarkCode
				{
					SourceCode = trimmed,
					SerialNumber = trimmed,
					ProductGroup = TrueMarkProductGroup.PetFood
				};
				return true;
			}

			return false;
		}

		private static TrueMarkCode CreateCode(Match match, bool isLongStructure)
		{
			var gtin = match.Groups[_gtinGroupName].Value;
			var serialNumber = match.Groups[_serialGroupName].Value;

			string checkCode;
			string sourceCode;

			if(isLongStructure)
			{
				var keyCheckId = match.Groups[_keyCheckIdGroupName].Value;
				var checkValue = match.Groups[_checkValueGroupName].Value;

				// Разделитель FNC1 между 91 и 92 в checkCode хранится реальным управляющим символом \u001d,
				// так как он подставляется в CashReceiptCode для кассового чека (ФФД 1.2).
				checkCode = $"91{keyCheckId}\u001d92{checkValue}";

				// В SourceCode все разделители хранятся в каноническом строковом виде "\\u001d" единообразно
				sourceCode = $"{TrueMarkCodeText.GroupSeparatorEscaped}01{gtin}21{serialNumber}"
					+ $"{TrueMarkCodeText.GroupSeparatorEscaped}91{keyCheckId}"
					+ $"{TrueMarkCodeText.GroupSeparatorEscaped}92{checkValue}";
			}
			else
			{
				var shortCheck = match.Groups[_checkCodeGroupName].Value;
				checkCode = $"93{shortCheck}";
				sourceCode = $"{TrueMarkCodeText.GroupSeparatorEscaped}01{gtin}21{serialNumber}"
					+ $"{TrueMarkCodeText.GroupSeparatorEscaped}93{shortCheck}";
			}

			return new TrueMarkCode
			{
				SourceCode = sourceCode,
				Gtin = gtin,
				SerialNumber = serialNumber,
				CheckCode = checkCode,
				ProductGroup = TrueMarkProductGroup.PetFood
			};
		}
	}
}
