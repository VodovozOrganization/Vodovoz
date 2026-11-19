using System.Linq;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Нормализация телефонных номеров
	/// </summary>
	public static class PhoneNumberNormalizer
	{
		/// <summary>
		/// Приводит номер к 10 цифрам без кода страны
		/// </summary>
		public static string Normalize(string phone)
		{
			if(string.IsNullOrWhiteSpace(phone))
			{
				return null;
			}

			var digits = new string(phone.Where(char.IsDigit).ToArray());

			if(string.IsNullOrWhiteSpace(digits))
			{
				return null;
			}

			if(digits.Length > 10)
			{
				digits = digits.Substring(digits.Length - 10);
			}

			return digits;
		}

		/// <summary>
		/// Маскирует номер для логирования
		/// </summary>
		public static string MaskForLog(string phone)
		{
			if(string.IsNullOrWhiteSpace(phone))
			{
				return "***";
			}

			if(phone.Length >= 10)
			{
				return $"{phone.Substring(0, 3)}***{phone.Substring(phone.Length - 4)}";
			}

			if(phone.Length >= 7)
			{
				return $"{phone.Substring(0, 2)}***{phone.Substring(phone.Length - 2)}";
			}

			if(phone.Length >= 4)
			{
				return $"{phone.Substring(0, 1)}***{phone.Substring(phone.Length - 1)}";
			}

			return "***";
		}
	}
}
