using System.Collections.Generic;
using System.Text.Json;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.CompletedAddressesReport
{
	/// <summary>
	/// Выбор фильтров "Водители" и "Районы" отчета по выполненным адресам и районам
	/// </summary>
	public class CompletedAddressesReportFilterSelection
	{
		/// <summary>
		/// Идентификаторы включенных водителей
		/// </summary>
		public List<int> IncludedDriverIds { get; set; } = new List<int>();

		/// <summary>
		/// Идентификаторы исключенных водителей
		/// </summary>
		public List<int> ExcludedDriverIds { get; set; } = new List<int>();

		/// <summary>
		/// Идентификаторы включенных районов
		/// </summary>
		public List<int> IncludedDistrictIds { get; set; } = new List<int>();

		/// <summary>
		/// Идентификаторы исключенных районов
		/// </summary>
		public List<int> ExcludedDistrictIds { get; set; } = new List<int>();

		/// <summary>
		/// Элемент "Без района" включен
		/// </summary>
		public bool IsWithoutDistrictIncluded { get; set; }

		/// <summary>
		/// Элемент "Без района" исключен
		/// </summary>
		public bool IsWithoutDistrictExcluded { get; set; }

		/// <summary>
		/// Условия запроса по выбору фильтров
		/// </summary>
		public CompletedAddressesCountsFilter ToCountsFilter()
		{
			return new CompletedAddressesCountsFilter
			{
				IncludedDriverIds = new List<int>(IncludedDriverIds),
				ExcludedDriverIds = new List<int>(ExcludedDriverIds),
				IncludedDistrictIds = new List<int>(IncludedDistrictIds),
				ExcludedDistrictIds = new List<int>(ExcludedDistrictIds),
				IsWithoutDistrictIncluded = IsWithoutDistrictIncluded,
				IsWithoutDistrictExcluded = IsWithoutDistrictExcluded
			};
		}

		/// <summary>
		/// Сериализация выбора в JSON
		/// </summary>
		public string ToJson()
		{
			return JsonSerializer.Serialize(this);
		}

		/// <summary>
		/// Разбор выбора из JSON. Пустая строка дает пустой выбор и true, некорректный JSON - пустой выбор и false
		/// </summary>
		public static bool TryParse(string json, out CompletedAddressesReportFilterSelection selection)
		{
			selection = new CompletedAddressesReportFilterSelection();

			if(string.IsNullOrWhiteSpace(json))
			{
				return true;
			}

			try
			{
				var parsed = JsonSerializer.Deserialize<CompletedAddressesReportFilterSelection>(json);

				if(parsed == null)
				{
					return false;
				}

				parsed.IncludedDriverIds = parsed.IncludedDriverIds ?? new List<int>();
				parsed.ExcludedDriverIds = parsed.ExcludedDriverIds ?? new List<int>();
				parsed.IncludedDistrictIds = parsed.IncludedDistrictIds ?? new List<int>();
				parsed.ExcludedDistrictIds = parsed.ExcludedDistrictIds ?? new List<int>();

				selection = parsed;
				return true;
			}
			catch(JsonException)
			{
				return false;
			}
		}
	}
}
