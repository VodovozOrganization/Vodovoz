using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Interfaces.TrueMark;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	/// <summary>
	/// Парсер строки кода полученного с маркировки честного знака для воды.
	/// <para/> 
	/// Источник: "Описание структуры кодов датаматрикс систем ГИС МТ, МДЛП Версия 1.2 от 28/01/2022"<br/>
	/// https://xn--80ajghhoc2aj1c8b.xn--p1ai/upload/%D0%A1%D1%82%D1%80%D1%83%D0%BA%D1%82%D1%83%D1%80%D0%B0%20DataMatrix.pdf
	/// <para/> 
	/// Код должен иметь формат: <![CDATA[<FNC1>01<GTIN>21<SerialNumber><GS>93<CheckCode>]]> где:<br/>
	/// FNC1 - специальный символ указывающий, что закодированная последовательность является штрихкодом GS1. Код 232 в таблице символов ASCII (E8 hex).<br/>
	/// Важно! На практике FNC1 имеет код hex 1D <br/>
	/// GTIN - Код товара (присваивается номенклатуре) <br/>
	/// SerialNumber - Серийный номер экземпляра <br/>
	/// CheckCode - Код проверки
	/// <para/> 
	/// Код идентификации формируется по шаблону: <![CDATA[01<GTIN>21<SerialNumber>]]>
	/// 
	/// КИГУ
	/// валидация совпадает с валидацией КИ
	/// 
	/// КИТУ
	/// от 18 до 74 символов включительно: цифры, буквы латинского алфавита,
	/// спецсимволы (A-Z a-z 0-9 % & ' " ( ) * + , - _ . / : ; < = > ? !);
	/// идентификаторы применения AI (00, 01, 21) в составе КИТУ указываются без обрамляющих
	/// круглых скобок.
	/// </summary>
	public class TrueMarkWaterCodeParser
	{
		private string _specialCodeName = "SpecialCodeOne";
		private string _gtinGroupName = "GTIN";
		private string _serialGroupName = "SerialNumber";
		private string _checkGroupName = "CheckCode";
		private static readonly string _restrictedChar = "\\u001d";

		/// <inheritdoc cref="TrueMarkWaterCodeParser"/>
		public TrueMarkWaterCode Parse(string rawCode)
		{
			var pattern = $"^(?<{_specialCodeName}>(\\\\u00e8)|(\\\\u001d)|([\\u00e8,\\u001d]{{1}}))(?<IdentificationCode>01(?<{_gtinGroupName}>[^{_restrictedChar}]{{14}})21(?<{_serialGroupName}>[^{_restrictedChar}]{{13}}))((\\\\u001d)|([\\u001d]{{1}}))93(?<{_checkGroupName}>[^{_restrictedChar}]{{4}})$";
			var regex = new Regex(pattern);
			var match = regex.Match(rawCode);
			
			if(!match.Success)
			{
				var altPattern = "^(?<IdentificationCode>\\(01\\)(?<GTIN>.{14})\\(21\\)(?<SerialNumber>.{13}))\\(93\\)(?<CheckCode>.{4})$";
				var altRegex = new Regex(altPattern);
				match = altRegex.Match(rawCode);
				
				if(!match.Success)
				{
					throw new TrueMarkException($"Невозможно распарсить код честного знака для воды. Код ({rawCode}).");
				}
			}

			Group specialCodeGroup = null;
			try
			{
				specialCodeGroup = match.Groups[_specialCodeName];
			}
			catch(Exception ex)
			{
			}
			
			Group gtinGroup = match.Groups[_gtinGroupName];
			Group serialGroup = match.Groups[_serialGroupName];
			Group checkGroup = match.Groups[_checkGroupName];

			if(gtinGroup == null || serialGroup == null || checkGroup == null)
			{
				throw new InvalidOperationException($"Ошибка определения составных частей кода (GTIN, серийного номера и кода проверки). Возможно код имеет не известный формат. Код ({rawCode}).");
			}

			string specialCodeOne = "\\u001d";
			string specialCodeTwo = "\\u001d";

			if(specialCodeGroup != null)
			{
				if(specialCodeGroup.Value == "\\u00e8" || specialCodeGroup.Value == "\u00e8")
				{
					specialCodeOne = "\\u00e8";
				}
			}

			string gtin = gtinGroup.Value;
			string serialNumber = serialGroup.Value;
			string checkCode = checkGroup.Value;

			//Формируем корректный полный код идентификации, заменяя спецсимволы на подстроку с кодом спецсимвола.
			//Для дальнейшей работы с кодом идентификации необходимо строкое представление спецсимволов.
			string sourceCode = $"{specialCodeOne}01{gtin}21{serialNumber}{specialCodeTwo}93{checkCode}";

			var result = new TrueMarkWaterCode
			{
				SourceCode = sourceCode,
				Gtin = gtin,
				SerialNumber = serialNumber,
				CheckCode = checkCode
			};

			return result;
		}

		/// <inheritdoc cref="TrueMarkWaterCodeParser"/>
		public bool TryParse(string rawCode, out TrueMarkWaterCode result)
		{
			result = null;
			try
			{
				result = Parse(rawCode);
				return true;
			}
			catch(Exception)
			{
				return false;
			}
		}

		public async Task<Result<IEnumerable<TrueMarkWaterIdentificationCode>>> Match(
			string rawCode,
			Func<TrueMarkWaterCode, Task<Result<IEnumerable<TrueMarkWaterIdentificationCode>>>> kiOrKiguAction,
			Func<string, Task<Result<IEnumerable<TrueMarkWaterIdentificationCode>>>> kituAction,
			Action<Exception> exceptionAction)
		{
			try
			{
				if(TryParse(rawCode, out var code))
				{
					return await kiOrKiguAction(code);
				}
				else
				{
					return await kituAction(rawCode);
				}
			}
			catch(Exception ex)
			{
				exceptionAction(ex);
				return await Task.FromResult(Vodovoz.Errors.TrueMark.TrueMarkCodeErrors.TrueMarkCodeParsingError);
			}
		}

		/// <summary>
		/// Попытка распарсить код честного знака
		/// с учетом того что код может быть введен не в полном формате.<br/>
		/// Все части кода являются не обязательными, кроме серийного номера <br/>
		/// Пример: https://regex101.com/r/sFR6jq/1
		/// </summary>
		public bool FuzzyParse(string input, out ITrueMarkWaterCode code)
		{
			var pattern = "^(?<SpecialCodeOne>(\\u00e8)|(\\u001d)|([\u00e8,\u001d]{1}))?(?<IdentificationCode>(01)?((?<GTIN>[^\u001d,^\u000a]{14}))?(^|(21)|(.{14}21.{13}))(?<SerialNumber>[^\u001d]{13}))((((\\u001d)|([\u001d]{1}))?($|(93)|(93.{4}))(?<CheckCode>[^\u001d]{4})?)|$)$";

			var regex = new Regex(pattern);
			var match = regex.Match(input);
			if(!match.Success)
			{
				code = null;
				return false;
			}

			var result = new TrueMarkWaterCode();

			var gtinGroup = match.Groups[_gtinGroupName];
			if(gtinGroup != null)
			{
				result.Gtin = gtinGroup.Value;
			}

			var serialGroup = match.Groups[_serialGroupName];
			if(serialGroup != null)
			{
				result.SerialNumber = serialGroup.Value;
			}

			var checkGroup = match.Groups[_checkGroupName];
			if(checkGroup != null)
			{
				result.CheckCode = checkGroup.Value;
			}

			code = result;
			return true;
		}

		public bool IsTransportCode(string rawCode)
		{
			var pattern = "((^00)(\\d{26}$))|(^\\d{26}$)";
			var regex = new Regex(pattern);
			var match = regex.Match(rawCode);
			return match.Success;
		}

		/// <summary>
		/// Формирует код идентификации из составных частей полного кода.
		/// </summary>
		[Obsolete("Используйте свойство IdentificationCode в коде")]
		public string GetWaterIdentificationCode(ITrueMarkWaterCode trueMarkWaterCode)
		{
			return $"01{trueMarkWaterCode.Gtin}21{trueMarkWaterCode.SerialNumber}";
		}

		[Obsolete("Используйте свойство CashReceiptCode в коде")]
		public string GetProductCodeForCashReceipt(ITrueMarkWaterCode trueMarkWaterCode)
		{
			return $"01{trueMarkWaterCode.Gtin}21{trueMarkWaterCode.SerialNumber}\u001d93{trueMarkWaterCode.CheckCode}";
		}

		[Obsolete("Используйте свойство Tag1260Code в коде")]
		public string GetProductCodeForTag1260(ITrueMarkWaterCode trueMarkWaterCode)
		{			
			return $"01{trueMarkWaterCode.Gtin}21{trueMarkWaterCode.SerialNumber}\u001d93{trueMarkWaterCode.CheckCode}";
		}

		public TrueMarkWaterCode ParseCodeFrom1c(string code)
		{
			var cleanCode = code
			.Replace("\"\"", "\"")
			.Replace("_x001d_", _restrictedChar);

			var pattern = $@"(?<IdentificationCode>01(?<{_gtinGroupName}>[^{_restrictedChar}]{{14}})21(?<{_serialGroupName}>[^{_restrictedChar}]{{13}}))((\|ГС\|)|(\\u001d)|()|())93(?<{_checkGroupName}>[^{_restrictedChar}]{{4}})";
			var regex = new Regex(pattern);

			var match = regex.Match(cleanCode);
			if(!match.Success)
			{
				throw new TrueMarkException($"Невозможно распарсить код честного знака для воды. Код ({cleanCode}).");
			}

			Group gtinGroup = match.Groups[_gtinGroupName];
			Group serialGroup = match.Groups[_serialGroupName];
			Group checkGroup = match.Groups[_checkGroupName];

			if(gtinGroup == null || serialGroup == null || checkGroup == null)
			{
				throw new InvalidOperationException($"Ошибка определения составных частей кода (GTIN, серийного номера и кода проверки). Возможно код имеет не известный формат. Код ({cleanCode}).");
			}

			string gtin = gtinGroup.Value;
			string serialNumber = serialGroup.Value;
			string checkCode = checkGroup.Value;
			string sourceCode = $"\\u001d01{gtin}21{serialNumber}\\u001d93{checkCode}";

			var result = new TrueMarkWaterCode
			{
				SourceCode = sourceCode,
				Gtin = gtin,
				SerialNumber = serialNumber,
				CheckCode = checkCode
			};

			return result;
		}
		
		public TrueMarkWaterCode ParseCodeFromSelfDelivery(string code)
		{
			var pattern = $@"(?<IdentificationCode>01(?<{_gtinGroupName}>.{{14}})21(?<{_serialGroupName}>.{{13}}))93(?<{_checkGroupName}>.{{4}})";
			var regex = new Regex(pattern);

			var match = regex.Match(code);
			if(!match.Success)
			{
				return null;
			}

			Group gtinGroup = match.Groups[_gtinGroupName];
			Group serialGroup = match.Groups[_serialGroupName];
			Group checkGroup = match.Groups[_checkGroupName];

			if(gtinGroup == null || serialGroup == null || checkGroup == null)
			{
				return null;
			}

			string gtin = gtinGroup.Value;
			string serialNumber = serialGroup.Value;
			string checkCode = checkGroup.Value;
			string sourceCode = $"\\u001d01{gtin}21{serialNumber}\\u001d93{checkCode}";

			var result = new TrueMarkWaterCode
			{
				SourceCode = sourceCode,
				Gtin = gtin,
				SerialNumber = serialNumber,
				CheckCode = checkCode
			};

			return result;
		}
	}
}
