using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

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
		private string gtinGroupName = "GTIN";
		private string serialGroupName = "SerialNumber";
		private string checkGroupName = "CheckCode";

		public TrueMarkWaterCodeParser()
		{
			//var pattern ="^((\\\\u00e8)|(\\\\u001d)|([\\u00e8,\\u001d]{1}))(?<IdentificationCode>01(?<GTIN>.{14})21(?<SerialNumber>.{13}))((\\\\u001d)|([\\u001d]{1}))93(?<CheckCode>.{4})$";
			var pattern = $"^((\\\\u00e8)|(\\\\u001d)|([\\u00e8,\\u001d]{{1}}))(?<IdentificationCode>01(?<{gtinGroupName}>.{{14}})21(?<{serialGroupName}>.{{13}}))((\\\\u001d)|([\\u001d]{{1}}))93(?<{checkGroupName}>.{{4}})$";
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

			Group gtinGroup = match.Groups[gtinGroupName];
			Group serialGroup = match.Groups[serialGroupName];
			Group checkGroup = match.Groups[checkGroupName];

			if(gtinGroup == null || serialGroup == null || checkGroup == null)
			{
				throw new InvalidOperationException($"Ошибка определения составных частей кода (GTIN, серийного номера и кода проверки). Возможно код имеет не известный формат. Код ({rawCode}).");
			}

			var result = new TrueMarkWaterCode
			{
				GTIN = match.Groups["GTIN"].Value,
				SerialNumber = match.Groups["SerialNumber"].Value,
				CheckCode = match.Groups["CheckCode"].Value,
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
	}
}
