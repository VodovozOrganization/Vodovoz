using QS.DomainModel.Entity;
using QS.Utilities.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.CompletedAddressesReport
{
	/// <summary>
	/// Отчет по выполненным адресам и районам
	/// </summary>
	[Appellative(Nominative = "Отчет по выполненным адресам и районам")]
	public partial class CompletedAddressesReport
	{
		/// <summary>
		/// Ключ адресов без района (как COALESCE в запросе бизнеса)
		/// </summary>
		public const string WithoutDistrictKey = "(без района)";

		/// <summary>
		/// Заголовок колонки адресов без района
		/// </summary>
		public const string WithoutDistrictTitle = "Без района";

		private static readonly StringComparer _keyComparer = StringComparer.OrdinalIgnoreCase;

		private CompletedAddressesReport()
		{
		}

		/// <summary>
		/// Начало периода
		/// </summary>
		public DateTime StartDate { get; private set; }

		/// <summary>
		/// Конец периода
		/// </summary>
		public DateTime EndDate { get; private set; }

		/// <summary>
		/// Заголовок отчета
		/// </summary>
		public string Title => $"Отчет по выполненным адресам с {StartDate:dd.MM.yyyy} по {EndDate:dd.MM.yyyy}";

		/// <summary>
		/// Заголовки колонок районов по порядку
		/// </summary>
		public IList<string> DistrictTitles { get; private set; } = new List<string>();

		/// <summary>
		/// Строки отчета
		/// </summary>
		public IList<CompletedAddressesReportRow> Rows { get; private set; } = new List<CompletedAddressesReportRow>();

		/// <summary>
		/// Ключ района узла: название района или ключ адресов без района
		/// </summary>
		public static string GetDistrictKey(CompletedAddressesCountNode node)
		{
			if(node.DistrictId == null || string.IsNullOrWhiteSpace(node.DistrictName))
			{
				return WithoutDistrictKey;
			}

			return node.DistrictName.Trim();
		}

		/// <summary>
		/// Различные водители из узлов (ключ - id строкой, заголовок - ФИО), по алфавиту
		/// </summary>
		public static IList<(string Key, string Title)> GetDrivers(IEnumerable<CompletedAddressesCountNode> nodes)
		{
			return nodes
				.GroupBy(n => n.DriverId)
				.Select(g => (Key: g.Key.ToString(), Title: GetDriverFullName(g.First()), Id: g.Key))
				.OrderBy(d => d.Title, GetTitleComparer())
				.ThenBy(d => d.Id)
				.Select(d => (d.Key, d.Title))
				.ToList();
		}

		/// <summary>
		/// Различные районы из узлов (ключ - название, для адресов без района - константа), по алфавиту заголовка
		/// </summary>
		public static IList<(string Key, string Title)> GetDistricts(IEnumerable<CompletedAddressesCountNode> nodes)
		{
			return nodes
				.Select(GetDistrictKey)
				.Distinct(_keyComparer)
				.Select(k => (Key: k, Title: GetDistrictTitle(k)))
				.OrderBy(d => d.Title, GetTitleComparer())
				.ToList();
		}

		/// <summary>
		/// Построение отчета из узлов запроса с учетом выбора фильтров
		/// </summary>
		public static CompletedAddressesReport Create(
			DateTime startDate,
			DateTime endDate,
			IEnumerable<CompletedAddressesCountNode> nodes,
			CompletedAddressesReportFilterSelection selection)
		{
			selection = selection ?? new CompletedAddressesReportFilterSelection();

			var includedDrivers = new HashSet<int>(selection.IncludedDriverIds);
			var excludedDrivers = new HashSet<int>(selection.ExcludedDriverIds);
			var includedDistricts = new HashSet<string>(selection.IncludedDistrictKeys, _keyComparer);
			var excludedDistricts = new HashSet<string>(selection.ExcludedDistrictKeys, _keyComparer);

			var filtered = nodes
				.Where(n => includedDrivers.Count == 0 || includedDrivers.Contains(n.DriverId))
				.Where(n => !excludedDrivers.Contains(n.DriverId))
				.Select(n => new { Node = n, DistrictKey = GetDistrictKey(n) })
				.Where(x => includedDistricts.Count == 0 || includedDistricts.Contains(x.DistrictKey))
				.Where(x => !excludedDistricts.Contains(x.DistrictKey))
				.ToList();

			var sums = filtered
				.GroupBy(x => new { x.Node.DriverId, DistrictKey = x.DistrictKey.ToUpperInvariant() })
				.Select(g => new
				{
					g.Key.DriverId,
					g.Key.DistrictKey,
					Count = g.Sum(x => x.Node.CompletedAddressesCount)
				})
				.Where(s => s.Count != 0)
				.ToList();

			var titlesByKey = filtered
				.GroupBy(x => x.DistrictKey.ToUpperInvariant())
				.ToDictionary(g => g.Key, g => GetDistrictTitle(g.First().DistrictKey));

			var districtColumns = sums
				.Select(s => s.DistrictKey)
				.Distinct()
				.OrderBy(k => titlesByKey[k], GetTitleComparer())
				.ToList();

			var driverNames = filtered
				.GroupBy(x => x.Node.DriverId)
				.ToDictionary(g => g.Key, g => GetDriverFullName(g.First().Node));

			var rows = sums
				.GroupBy(s => s.DriverId)
				.Select(g =>
				{
					var counts = districtColumns
						.Select(k => g.Where(s => s.DistrictKey == k).Sum(s => s.Count))
						.ToArray();

					return new CompletedAddressesReportRow
					{
						DriverId = g.Key,
						DriverFullName = driverNames[g.Key],
						DistrictCounts = counts,
						Total = counts.Sum()
					};
				})
				.OrderBy(r => r.DriverFullName, GetTitleComparer())
				.ThenBy(r => r.DriverId)
				.ToList();

			return new CompletedAddressesReport
			{
				StartDate = startDate,
				EndDate = endDate,
				DistrictTitles = districtColumns.Select(k => titlesByKey[k]).ToList(),
				Rows = rows
			};
		}

		private static string GetDriverFullName(CompletedAddressesCountNode node)
		{
			return PersonHelper.PersonFullName(node.DriverLastName, node.DriverName, node.DriverPatronymic);
		}

		private static string GetDistrictTitle(string districtKey)
		{
			return _keyComparer.Equals(districtKey, WithoutDistrictKey) ? WithoutDistrictTitle : districtKey;
		}

		private static StringComparer GetTitleComparer()
		{
			return StringComparer.Create(CultureInfo.GetCultureInfo("ru-RU"), true);
		}
	}
}
