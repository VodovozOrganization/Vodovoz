using DateTimeHelpers;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Presentation.ViewModels.Reports;

namespace Vodovoz.Presentation.ViewModels.Store.Reports
{
	public partial class DefectiveItemsReport : IClosedXmlReport
	{
		private DefectiveItemsReport(
			DateTime startDate,
			DateTime endDate,
			int? driverId,
			IEnumerable<DefectiveItemsReportRow> defectiveItemsReportRows)
		{
			StartDate = startDate;
			EndDate = endDate.LatestDayTime();
			DriverId = driverId;

			CreatedAt = DateTime.Now;
			Rows = defectiveItemsReportRows;
		}

		public string TemplatePath => @".\Reports\Store\DefectiveItemsReport.xlsx";

		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public int? DriverId { get; }
		public DateTime CreatedAt { get; }

		public IEnumerable<DefectiveItemsReportRow> Rows { get; }

		public static DefectiveItemsReport Create(IUnitOfWork unitOfWork, DateTime startDate, DateTime endDate, int? driverId)
		{
			var rows = new List<DefectiveItemsReportRow>();

			return new DefectiveItemsReport(startDate, endDate, driverId, rows);
		}
	}
}
