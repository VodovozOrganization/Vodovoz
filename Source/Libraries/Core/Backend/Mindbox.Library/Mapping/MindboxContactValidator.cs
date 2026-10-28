using System;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.Core.Domain.Contacts;

namespace Mindbox.Library.Mapping
{
	/// <summary>
	/// Проверка и маскирование контактных данных для Mindbox
	/// </summary>
	public static class MindboxContactValidator
	{
		private static readonly Regex _phoneRegex = new Regex(
			"^[3489][0-9]{9}$",
			RegexOptions.Compiled,
			TimeSpan.FromSeconds(1));

		private static readonly Regex _emailRegex = new Regex(
			EmailEntity.EmailRegEx,
			RegexOptions.Compiled,
			TimeSpan.FromSeconds(1));

		/// <summary>
		/// Приводит телефон к формату +7XXXXXXXXXX.
		/// Возвращает null, если номер некорректен.
		/// </summary>
		public static string NormalizePhone(string source)
		{
			if(string.IsNullOrWhiteSpace(source))
			{
				return null;
			}

			var digits = new string(source.Where(char.IsDigit).ToArray());

			if(digits.Length == 11 && (digits[0] == '7' || digits[0] == '8'))
			{
				digits = digits.Substring(1);
			}

			if(digits.Length != 10 || !_phoneRegex.IsMatch(digits))
			{
				return null;
			}

			return $"+7{digits}";
		}

		/// <summary>
		/// Проверяет формат e-mail
		/// </summary>
		public static bool IsValidEmail(string email)
		{
			if(string.IsNullOrWhiteSpace(email))
			{
				return false;
			}

			try
			{
				return _emailRegex.IsMatch(email);
			}
			catch(RegexMatchTimeoutException)
			{
				return false;
			}
		}

		/// <summary>
		/// Маскирует телефон для логов
		/// </summary>
		public static string MaskPhone(string phone)
		{
			if(string.IsNullOrWhiteSpace(phone))
			{
				return string.Empty;
			}

			var digits = new string(phone.Where(char.IsDigit).ToArray());

			return digits.Length >= 4
				? $"+7***{digits.Substring(digits.Length - 4)}"
				: "+7***";
		}

		/// <summary>
		/// Маскирует e-mail для логов
		/// </summary>
		public static string MaskEmail(string email)
		{
			if(string.IsNullOrWhiteSpace(email))
			{
				return string.Empty;
			}

			var atIndex = email.IndexOf('@');

			if(atIndex <= 0 || atIndex == email.Length - 1)
			{
				return "***";
			}

			return $"{email[0]}***{email.Substring(atIndex)}";
		}

		/// <summary>
		/// Маскирует ФИО/название для логов
		/// </summary>
		public static string MaskName(string name)
		{
			if(string.IsNullOrWhiteSpace(name))
			{
				return string.Empty;
			}

			return $"{name[0]}***";
		}
	}
}
