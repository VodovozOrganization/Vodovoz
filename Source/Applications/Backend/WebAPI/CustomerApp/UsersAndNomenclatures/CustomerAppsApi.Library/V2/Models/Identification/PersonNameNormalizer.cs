using System;
using System.Linq;
using System.Text.RegularExpressions;
using CustomerAppsApi.Library.V2.Dto.Counterparties;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Нормализация и разбор ФИО
	/// </summary>
	public static class PersonNameNormalizer
	{
		/// <summary>
		/// Разбирает запрос на фамилию, имя и отчество
		/// </summary>
		public static (string Surname, string FirstName, string Patronymic) Parse(CounterpartyNameSearchDto request)
		{
			if(request == null)
			{
				return (null, null, null);
			}

			var surname = NormalizePart(request.Surname);
			var firstName = NormalizePart(request.FirstName);
			var patronymic = NormalizePart(request.Patronymic);

			if(string.IsNullOrWhiteSpace(surname)
				&& string.IsNullOrWhiteSpace(firstName)
				&& string.IsNullOrWhiteSpace(patronymic)
				&& !string.IsNullOrWhiteSpace(request.FullName))
			{
				var normalizedFullName = NormalizePart(request.FullName);

				if(!string.IsNullOrWhiteSpace(normalizedFullName))
				{
					var fullNameParts = normalizedFullName
						.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
						.ToList();

					// Ассистент может передать ФИО с приставкой формы организации
					if(fullNameParts.Count > 2
						&& fullNameParts[0] == "индивидуальный"
						&& fullNameParts[1] == "предприниматель")
					{
						fullNameParts.RemoveRange(0, 2);
					}
					else if(fullNameParts.Count > 1
						&& (fullNameParts[0] == "ип" || fullNameParts[0] == "фл"))
					{
						fullNameParts.RemoveAt(0);
					}

					if(fullNameParts.Count > 0)
					{
						surname = fullNameParts[0];
					}

					if(fullNameParts.Count > 1)
					{
						firstName = fullNameParts[1];
					}

					if(fullNameParts.Count > 2)
					{
						patronymic = string.Join(" ", fullNameParts.Skip(2));
					}
				}
			}

			return (surname, firstName, patronymic);
		}

		/// <summary>
		/// Нормализует часть ФИО
		/// </summary>
		public static string NormalizePart(string value)
		{
			if(string.IsNullOrWhiteSpace(value))
			{
				return null;
			}

			var normalized = value.Trim().ToLowerInvariant().Replace('ё', 'е');
			normalized = Regex.Replace(normalized, @"[.,;:]+", " ");
			normalized = Regex.Replace(normalized, @"[%_]+", " ");
			normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

			return string.IsNullOrWhiteSpace(normalized)
				? null
				: normalized;
		}

		/// <summary>
		/// Собирает ФИО из частей
		/// </summary>
		public static string BuildFullName(string surname, string firstName, string patronymic)
		{
			return string.Join(
				" ",
				new[] { surname, firstName, patronymic }
					.Where(x => !string.IsNullOrWhiteSpace(x)));
		}
	}
}
