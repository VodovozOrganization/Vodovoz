using TrueMark.Contracts;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Errors;
using Vodovoz.Core.Domain.Interfaces.TrueMark;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	/// <summary>
	/// Парсер строки кода, полученного с маркировки «Честный знак» для воды.
	/// <para/> 
	/// Источник: "Описание структуры кодов датаматрикс систем ГИС МТ, МДЛП Версия 1.2 от 28/01/2022"<br/>
	/// https://xn--80ajghhoc2aj1c8b.xn--p1ai
	/// <para/> 
	/// Код должен иметь формат: <![CDATA[<FNC1>01<GTIN>21<SerialNumber><GS>93<CheckCode>]]> где:<br/>
	/// FNC1 - специальный символ, указывающий, что закодированная последовательность является штрихкодом GS1. Код 232 в таблице символов ASCII (E8 hex).<br/>
	/// Важно! На практике FNC1 имеет код hex 1D <br/>
	/// GTIN - Код товара (присваивается номенклатуре), 14 цифр <br/>
	/// SerialNumber - Серийный номер экземпляра, 13 символов <br/>
	/// CheckCode - Код проверки, 4 символа
	/// <para/> 
	/// Код идентификации формируется по шаблону: <![CDATA[01<GTIN>21<SerialNumber>]]>
	/// 
	/// КИГУ
	/// валидация совпадает с валидацией КИ
	/// 
	/// КИТУ
	/// от 18 до 74 символов включительно: цифры, буквы латинского алфавита,
	/// спецсимволы (A-Z a-z 0-9 % &amp; ' " ( ) * + , - _ . / : ; &lt; = &gt; ? !);
	/// идентификаторы применения AI (00, 01, 21) в составе КИТУ указываются без обрамляющих
	/// круглых скобок.
	/// <para/>
	/// Допустимые форматы входной строки: стандартный (разделитель FNC1 необязателен, признак FNC1 в начале необязателен,
	/// допускается экранированный текст "\u001d"/"\u00e8"), скобочный <![CDATA[(01)…(21)…(93)…]]>,
	/// выгрузка из 1С (<see cref="TryParseCodeFrom1c"/>), ручной ввод (<see cref="TryParseManual"/>),
	/// нечёткий разбор для поиска (<see cref="FuzzyParse"/>).
	/// </summary>
	public class TrueMarkWaterCodeParser : ITrueMarkProductCodeParser
	{
		private const string _leadGroupName = "Lead";
		private const string _gtinGroupName = "GTIN";
		private const string _serialGroupName = "SerialNumber";
		private const string _checkGroupName = "CheckCode";

		private static readonly TimeSpan _fuzzyTimeout = TimeSpan.FromSeconds(1);

		// Стандартный формат: [FNC1]01<GTIN 14 цифр>21<серийный 13>[GS]93<проверка 4>, GS необязателен.
		private static readonly Regex _standardPattern = BuildIdentificationPattern(@"\u001d?", _leadGroupName);

		// Формат из 1С: как стандартный, но перед 93 допускается ещё и пробел (так выгружает часть отчётов).
		private static readonly Regex _from1cPattern = BuildIdentificationPattern(@"[\u001d ]?", _leadGroupName);

		// Скобочный формат: (01)...(21)...(93)...
		private static readonly Regex _altPattern = new Regex(
			$@"^\(01\)(?<{_gtinGroupName}>\d{{14}})\(21\)(?<{_serialGroupName}>{TrueMarkCodeText.TextChar}{{13}})\(93\)(?<{_checkGroupName}>{TrueMarkCodeText.TextChar}{{4}})\z",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		// Ручной ввод без разделителей: допускает постороннее окружение вокруг кода
		// (например, скопированный из письма текст), поэтому намеренно не привязан к началу/концу строки.
		private static readonly Regex _manualPattern = new Regex(
			$@"01(?<{_gtinGroupName}>\d{{14}})21(?<{_serialGroupName}>{TrueMarkCodeText.TextChar}{{13}})93(?<{_checkGroupName}>{TrueMarkCodeText.TextChar}{{4}})",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		// Нечёткий разбор для поиска: все части кода, кроме серийного номера, необязательны.
		private static readonly Regex _fuzzyPattern = new Regex(
			$@"^(?<{_leadGroupName}>(\\u00e8)|(\\u001d)|([\u00e8\u001d]{{1}}))?(?<IdentificationCode>(01)?((?<{_gtinGroupName}>[^\u001d\u000a]{{14}}))?(^|(21)|(.{{14}}21.{{13}}))(?<{_serialGroupName}>[^\u001d]{{13}}))((((\\u001d)|([\u001d]{{1}}))?($|(93)|(93.{{4}}))(?<{_checkGroupName}>[^\u001d]{{4}})?)|$)$",
			RegexOptions.Compiled | RegexOptions.CultureInvariant,
			_fuzzyTimeout);

		private static Regex BuildIdentificationPattern(string separatorPattern, string leadGroupName)
		{
			return new Regex(
				$@"^(?<{leadGroupName}>{TrueMarkCodeText.LeadingFnc1})?01(?<{_gtinGroupName}>\d{{14}})21(?<{_serialGroupName}>{TrueMarkCodeText.TextChar}{{13}}){separatorPattern}93(?<{_checkGroupName}>{TrueMarkCodeText.TextChar}{{4}})\z",
				RegexOptions.Compiled | RegexOptions.CultureInvariant);
		}

		/// <summary>
		/// Разбор кода воды в стандартном (в т.ч. скобочном) формате.
		/// </summary>
		public bool TryParse(string rawCode, out ITrueMarkCode result)
		{
			result = null;

			if(string.IsNullOrWhiteSpace(rawCode))
			{
				return false;
			}

			var text = TrueMarkCodeText.PrepareStandard(rawCode);

			if(TryCreateCode(_standardPattern.Match(text), out var code)
				|| TryCreateCode(_altPattern.Match(text), out code))
			{
				result = code;
				return true;
			}

			return false;
		}

		/// <remarks>
		/// Метод оставлен без изменений ради совместимости. Он разбирает только код воды, поэтому всё нераспознанное
		/// (в том числе код кормов для животных) уходит в <paramref name="kituAction"/> как транспортный код.
		/// Ответственность оркестрации не относится к парсеру; при возможности перенести в прикладной сервис
		/// и определять тип кода через <see cref="TrueMarkCodeParser"/>.
		/// </remarks>
		public async Task<Result<IEnumerable<TrueMarkIdentificationCode>>> Match(
			string rawCode,
			Func<TrueMarkCode, Task<Result<IEnumerable<TrueMarkIdentificationCode>>>> kiOrKiguAction,
			Func<string, Task<Result<IEnumerable<TrueMarkIdentificationCode>>>> kituAction,
			Action<Exception> exceptionAction)
		{
			try
			{
				if(TryParse(rawCode, out var parsed) && parsed is TrueMarkCode code)
				{
					return await kiOrKiguAction(code);
				}

				return await kituAction(rawCode);
			}
			catch(Exception ex)
			{
				exceptionAction(ex);
				return TrueMarkCodeErrors.TrueMarkCodeParsingError;
			}
		}

		/// <summary>
		/// Попытка распарсить код честного знака
		/// с учётом того, что код может быть введён не в полном формате.<br/>
		/// Все части кода являются необязательными, кроме серийного номера.<br/>
		/// Пример: https://regex101.com
		/// </summary>
		public bool FuzzyParse(string input, out ITrueMarkCode result)
		{
			result = null;

			if(string.IsNullOrWhiteSpace(input))
			{
				return false;
			}

			Match match;

			try
			{
				match = _fuzzyPattern.Match(TrueMarkCodeText.Prepare(input));
			}
			catch(RegexMatchTimeoutException)
			{
				return false;
			}

			if(!match.Success)
			{
				return false;
			}

			var serialGroup = match.Groups[_serialGroupName];
			if(!serialGroup.Success || string.IsNullOrEmpty(serialGroup.Value))
			{
				return false;
			}

			result = new TrueMarkCode
			{
				Gtin = match.Groups[_gtinGroupName].Success ? match.Groups[_gtinGroupName].Value : null,
				SerialNumber = serialGroup.Value,
				CheckCode = match.Groups[_checkGroupName].Success ? match.Groups[_checkGroupName].Value : null,
				ProductGroup = TrueMarkProductGroup.Water
			};

			return true;
		}

		/// <summary>
		/// Разбор кода из 1С/CSV/Excel: снимаются внешние кавычки, раскрываются удвоенные,
		/// маркеры разделителя (_x001d_ в любом регистре, |ГС|, пробел перед 93) приводятся к GS.
		/// Строка должна целиком быть кодом (без постороннего текста).
		/// </summary>
		public bool TryParseCodeFrom1c(string code, out ITrueMarkCode result)
		{
			result = null;

			if(string.IsNullOrWhiteSpace(code))
			{
				return false;
			}

			if(TryCreateCode(_from1cPattern.Match(TrueMarkCodeText.PrepareFrom1c(code)), out var identificationCode))
			{
				result = identificationCode;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Разбор кода, введённого вручную без разделителей.
		/// Допускает постороннее окружение вокруг кода.
		/// </summary>
		public bool TryParseManual(string rawCode, out ITrueMarkCode result)
		{
			result = null;

			if(string.IsNullOrWhiteSpace(rawCode))
			{
				return false;
			}

			if(TryCreateCode(_manualPattern.Match(TrueMarkCodeText.UnescapeSeparators(rawCode)), out var identificationCode))
			{
				result = identificationCode;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Разбор кода, введённого вручную. Возвращает null, если разобрать не удалось.
		/// </summary>
		public TrueMarkCode ParseCodeFromManual(string code)
		{
			return TryParseManual(code, out var result) ? result as TrueMarkCode : null;
		}

		/// <summary>
		/// Собирает код из совпадения регулярного выражения. SourceCode формируется в каноническом виде:
		/// спецсимволы заменяются подстрокой с кодом спецсимвола, поскольку для дальнейшей работы
		/// с кодом идентификации необходимо строковое представление спецсимволов.
		/// </summary>
		private static bool TryCreateCode(Match match, out TrueMarkCode result)
		{
			result = null;

			if(!match.Success)
			{
				return false;
			}

			var gtin = match.Groups[_gtinGroupName].Value;
			var serialNumber = match.Groups[_serialGroupName].Value;
			var checkCode = match.Groups[_checkGroupName].Value;

			var leadGroup = match.Groups[_leadGroupName];
			var leadingSymbol = leadGroup.Success && leadGroup.Value == TrueMarkCodeText.Fnc1Symbol.ToString()
				? TrueMarkCodeText.Fnc1SymbolEscaped
				: TrueMarkCodeText.GroupSeparatorEscaped;

			result = new TrueMarkCode
			{
				SourceCode = $"{leadingSymbol}01{gtin}21{serialNumber}{TrueMarkCodeText.GroupSeparatorEscaped}93{checkCode}",
				Gtin = gtin,
				SerialNumber = serialNumber,
				CheckCode = checkCode,
				ProductGroup = TrueMarkProductGroup.Water
			};

			return true;
		}
	}
}
