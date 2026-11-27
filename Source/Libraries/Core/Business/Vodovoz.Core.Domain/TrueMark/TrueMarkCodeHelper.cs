using TrueMark.Contracts;
using System;

namespace Vodovoz.Core.Domain.TrueMark
{
	/// <summary>
	/// Вспомогательные методы для работы с кодами маркировки «Честный знак».
	/// </summary>
	public static class TrueMarkCodeHelper
	{
		/// <summary>
		/// Извлекает серийный номер из кода идентификации формата GS1: 01{GTIN}21{SerialNumber}.
		/// </summary>
		/// <param name="identificationCode">Код идентификации</param>
		/// <param name="gtin">GTIN товара (14 цифр)</param>
		/// <returns>Серийный номер товара</returns>
		public static string ExtractSerialNumber(string identificationCode, string gtin)
		{
			if(string.IsNullOrWhiteSpace(identificationCode))
			{
				return string.Empty;
			}

			// Стандартный формат GS1: 01 + 14 цифр GTIN + 21 + SerialNumber
			if(identificationCode.Length > 18
				&& identificationCode.StartsWith("01", StringComparison.Ordinal)
				&& identificationCode.Substring(16, 2) == "21")
			{
				return identificationCode.Substring(18);
			}

			if(!string.IsNullOrEmpty(gtin) && identificationCode.Contains(gtin))
			{
				var gtinIndex = identificationCode.IndexOf(gtin, StringComparison.Ordinal);
				var afterGtin = identificationCode.Substring(gtinIndex + gtin.Length);
				if(afterGtin.StartsWith("21", StringComparison.Ordinal))
				{
					return afterGtin.Substring(2);
				}
			}

			// Не GS1 и GTIN не найден — серийник извлечь нельзя.
			// Раньше здесь стоял Replace(gtin ?? "", ""), который при пустом GTIN бросал ArgumentException.
			return string.Empty;
		}

		/// <summary>
		/// Формирует код маркировки для фискального чека (CashReceiptCode, тег 1162 / 1260 ФФД 1.2).
		/// </summary>
		public static string FormatCashReceiptCode(TrueMarkProductGroup productGroup, string identificationCode, string checkCode)
		{
			if(string.IsNullOrEmpty(checkCode))
			{
				return identificationCode;
			}

			// Для каждой товарной группы нужен явный случай: у воды CheckCode хранится в БД без префикса AI 93
			// (добавляем его при форматировании), у кормов — уже с AI (93... или 91...92...).
			// Явный default заставляет добавить case для новой группы, а не молча «угадывать» формат.
			string prefix;
			switch(productGroup)
			{
				case TrueMarkProductGroup.Water:
					prefix = "93";
					break;
				case TrueMarkProductGroup.PetFood:
					prefix = string.Empty;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(productGroup), productGroup, "Неизвестная товарная группа маркировки");
			}

			return $"{identificationCode}\u001d{prefix}{checkCode}";
		}
	}
}
