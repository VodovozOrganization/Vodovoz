using iTextSharp.text.pdf;
using System;
using System.Text.RegularExpressions;

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
	/// </summary>
	public class TrueMarkWaterCodeParser
	{
		private Regex _regex;
		private string _specialCodeName = "SpecialCodeOne";
		private string _gtinGroupName = "GTIN";
		private string _serialGroupName = "SerialNumber";
		private string _checkGroupName = "CheckCode";

		public TrueMarkWaterCodeParser()
		{
			var pattern = $"^(?<{_specialCodeName}>(\\\\u00e8)|(\\\\u001d)|([\\u00e8,\\u001d]{{1}}))(?<IdentificationCode>01(?<{_gtinGroupName}>.{{14}})21(?<{_serialGroupName}>.{{13}}))((\\\\u001d)|([\\u001d]{{1}}))93(?<{_checkGroupName}>.{{4}})$";
			_regex = new Regex(pattern);
		}
		
		/// <inheritdoc cref="TrueMarkWaterCodeParser"/>
		public TrueMarkWaterCode Parse(string rawCode)
		{
			var match = _regex.Match(rawCode);
			if(!match.Success)
			{
				throw new TrueMarkException($"Невозможно распарсить код честного знака для воды. Код ({rawCode}).");
			}

			Group specialCodeGroup = match.Groups[_specialCodeName];
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
				GTIN = gtin,
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

		/// <summary>
		/// Формирует код идентификации из составных частей полного кода.
		/// </summary>
		public string GetWaterIdentificationCode(ITrueMarkWaterCode trueMarkWaterCode)
		{
			return $"01{trueMarkWaterCode.GTIN}21{trueMarkWaterCode.SerialNumber}";
		}

		public string GetProductCodeForCashReceipt(ITrueMarkWaterCode trueMarkWaterCode)
		{
			return $"\u001d01{trueMarkWaterCode.GTIN}21{trueMarkWaterCode.SerialNumber}\u001d93{trueMarkWaterCode.CheckCode}";
		}

		public TrueMarkWaterCode ParseCodeFrom1c(string code)
		{
			var cleanCode = code.Replace("\"\"", "\"");

			var pattern = $@"(?<IdentificationCode>01(?<{_gtinGroupName}>.{{14}})21(?<{_serialGroupName}>.{{13}}))((\|ГС\|)|(\\u001d)|()|())93(?<{_checkGroupName}>.{{4}})";
			_regex = new Regex(pattern);

			var match = _regex.Match(cleanCode);
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
				GTIN = gtin,
				SerialNumber = serialNumber,
				CheckCode = checkCode
			};

			return result;
		}
	}
}
