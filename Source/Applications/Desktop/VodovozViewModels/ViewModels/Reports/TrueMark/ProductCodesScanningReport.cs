using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Tools;
using static Vodovoz.ViewModels.ViewModels.Reports.FastDelivery.FastDeliveryAdditionalLoadingReportViewModel;

namespace Vodovoz.ViewModels.ViewModels.Reports.TrueMark
{
	[Appellative(Nominative = "Отчет о сканировании водителями маркировки ЧЗ")]
	public class ProductCodesScanningReport
	{
		public ProductCodesScanningReport(DateTime createDateFrom, DateTime createDateTo, List<Row> rows)
		{
			CreateDateFrom = createDateFrom;
			CreateDateTo = createDateTo;
			Rows = rows;
			ReportCreatedAt = DateTime.Now;
		}

		public string Title => typeof(FastDeliveryRemainingBottlesReport).GetClassUserFriendlyName().Nominative;

		public DateTime CreateDateFrom { get; }

		public DateTime CreateDateTo { get; }

		public DateTime ReportCreatedAt { get; }

		public List<Row> Rows { get; set; }

		public static ProductCodesScanningReport Generate(IUnitOfWork unitOfWork, DateTime createDateFrom, DateTime createDateTo)
		{
			return new ProductCodesScanningReport(createDateFrom, createDateTo, new List<Row>());
		}

		public class Row
		{
			public int RowNumber { get; set; }
			public string DriversName { get; set; }
			public int TotalCodesCount { get; set; }
			public int SuccessfullyScannedCodesCount { get; set; }
			public decimal SuccessfullyScannedCodesPercent { get; set; }
			public int UnscannedCodesCount { get; set; }
			public decimal UnscannedCodesPercent { get; set; }
			public int SingleDuplicatedCodesCount { get; set; }
			public decimal SingleDuplicatedCodesPercent { get; set; }
			public int MultiplyDuplicatedCodesCount { get; set; }
			public decimal MultiplyDuplicatedCodesPercent { get; set; }
			public int InvalidCodesCount { get; set; }
			public decimal InvalidCodesPercent { get; set; }
		}
	}
}
