using System;
using System.Collections.Generic;
using static Vodovoz.ViewModels.ViewModels.Reports.Sales.SalesBySubdivisionsAnalitycsReport;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsReportViewModel
	{
		public class SalesBySubdivisionsAnalitycsWithDynamicsReport
		{
			private readonly List<DisplayRow> _displayRows = new List<DisplayRow>();

			private SalesBySubdivisionsAnalitycsWithDynamicsReport(
				DateTime firstPeriodStartDate,
				DateTime firstPeriodEndDate,
				DateTime? secondPeriodStartDate,
				DateTime? secondPeriodEndDate,
				bool splitByNomenclatures,
				bool splitBySubdivisions,
				IDictionary<int, string> nomenclatures,
				IDictionary<int, string> productGroups,
				IDictionary<int, string> subdivisions,
				IEnumerable<SalesDataNode> sales)
			{
				
			}

			public string Title => "Аналитика продаж КБ с динамикой";

			public DateTime FirstPeriodStartDate { get; }

			public DateTime FirstPeriodEndDate { get; }

			public DateTime? SecondPeriodStartDate { get; }

			public DateTime? SecondPeriodEndDate { get; }

			public bool SplitByNomenclatures { get; }

			public bool SplitBySubdivisions { get; }

			public IDictionary<int, string> Subdivisions { get; set; }

			public IDictionary<int, string> Nomenclatures { get; }

			public IDictionary<int, string> ProductGroups { get; }

			public DateTime CreatedAt { get; }

			public List<DisplayRow> DisplayRows => _displayRows;

			public static SalesBySubdivisionsAnalitycsWithDynamicsReport Create(
				DateTime firstPeriodStartDate,
				DateTime firstPeriodEndDate,
				DateTime? secondPeriodStartDate,
				DateTime? secondPeriodEndDate,
				bool splitByNomenclatures,
				bool splitBySubdivisions,
				IDictionary<int, string> nomenclatures,
				IDictionary<int, string> productGroups,
				IDictionary<int, string> subdivisions,
				IEnumerable<SalesDataNode> sales)
			{
				return new SalesBySubdivisionsAnalitycsWithDynamicsReport(
					firstPeriodStartDate,
					firstPeriodEndDate,
					secondPeriodStartDate,
					secondPeriodEndDate,
					splitByNomenclatures,
					splitBySubdivisions,
					nomenclatures,
					productGroups,
					subdivisions,
					sales);
			}
		}
	}
}
