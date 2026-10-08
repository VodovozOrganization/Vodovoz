using System.Collections.Generic;
using System.Text.Json;

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
		/// Ключи (названия) включенных районов
		/// </summary>
		public List<string> IncludedDistrictKeys { get; set; } = new List<string>();

		/// <summary>
		/// Ключи (названия) исключенных районов
		/// </summary>
		public List<string> ExcludedDistrictKeys { get; set; } = new List<string>();

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
				parsed.IncludedDistrictKeys = parsed.IncludedDistrictKeys ?? new List<string>();
				parsed.ExcludedDistrictKeys = parsed.ExcludedDistrictKeys ?? new List<string>();

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
