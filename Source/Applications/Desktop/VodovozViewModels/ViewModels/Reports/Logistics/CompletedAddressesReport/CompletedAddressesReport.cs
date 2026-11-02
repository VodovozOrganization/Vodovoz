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
		/// Заголовок колонки адресов без района
		/// </summary>
		public const string WithoutDistrictTitle = "Без района";

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
		/// Водители для фильтра (id и ФИО), по алфавиту
		/// </summary>
		public static IList<(int Id, string Title)> GetDrivers(IEnumerable<CompletedAddressesCountNode> nodes)
		{
			return nodes
				.GroupBy(n => n.DriverId)
				.Select(g => (Id: g.Key, Title: GetDriverFullName(g.First())))
				.OrderBy(d => d.Title, GetTitleComparer())
				.ThenBy(d => d.Id)
				.ToList();
		}

		/// <summary>
		/// Районы для фильтра (id района, для адресов без района - null), по алфавиту заголовка
		/// </summary>
		public static IList<(int? Id, string Title)> GetDistricts(IEnumerable<CompletedAddressesCountNode> nodes)
		{
			return nodes
				.GroupBy(n => n.DistrictId)
				.Select(g => (Id: g.Key, Title: GetDistrictTitle(g.First())))
				.OrderBy(d => d.Title, GetTitleComparer())
				.ThenBy(d => d.Id)
				.ToList();
		}

		/// <summary>
		/// Построение отчета из узлов запроса (узлы уже отфильтрованы запросом)
		/// </summary>
		public static CompletedAddressesReport Create(
			DateTime startDate,
			DateTime endDate,
			IEnumerable<CompletedAddressesCountNode> nodes)
		{
			var items = nodes.ToList();

			var sums = items
				.GroupBy(n => new { n.DriverId, n.DistrictId })
				.Select(g => new
				{
					g.Key.DriverId,
					g.Key.DistrictId,
					Count = g.Sum(n => n.CompletedAddressesCount)
				})
				.Where(s => s.Count != 0)
				.ToList();

			var districtColumns = sums
				.Select(s => s.DistrictId)
				.Distinct()
				.Select(id => new { Id = id, Node = items.First(n => n.DistrictId == id) })
				.OrderBy(d => GetDistrictTitle(d.Node), GetTitleComparer())
				.ThenBy(d => d.Id)
				.ToList();

			var driverNames = items
				.GroupBy(n => n.DriverId)
				.ToDictionary(g => g.Key, g => GetDriverFullName(g.First()));

			var rows = sums
				.GroupBy(s => s.DriverId)
				.Select(g =>
				{
					var counts = districtColumns
						.Select(c => g.Where(s => s.DistrictId == c.Id).Sum(s => s.Count))
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
				DistrictTitles = districtColumns.Select(c => GetDistrictTitle(c.Node)).ToList(),
				Rows = rows
			};
		}

		private static string GetDriverFullName(CompletedAddressesCountNode node)
		{
			return PersonHelper.PersonFullName(node.DriverLastName, node.DriverName, node.DriverPatronymic);
		}

		private static string GetDistrictTitle(CompletedAddressesCountNode node)
		{
			return node.DistrictId == null ? WithoutDistrictTitle : node.DistrictName;
		}

		private static StringComparer GetTitleComparer()
		{
			return StringComparer.Create(CultureInfo.GetCultureInfo("ru-RU"), true);
		}
	}
}
