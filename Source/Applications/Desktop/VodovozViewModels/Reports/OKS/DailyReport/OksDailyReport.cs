using ClosedXML.Excel;
using DateTimeHelpers;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.EntityRepositories.Complaints;

namespace Vodovoz.ViewModels.Reports.OKS.DailyReport
{
	public partial class OksDailyReport
	{
		private const string _complaintsSummaryWorksheetName = "Сводка рекламаций";
		private const string _dateFormatString = "dd.MM.yyyy";

		private OksDailyReport() { }

		public DateTime Date { get; private set; }

		public void ExportReport(string path)
		{
			using(var workbook = new XLWorkbook())
			{
				var complaintsSummaryWorksheet = workbook.Worksheets.Add(_complaintsSummaryWorksheetName);
				FillComplaintsSummaryWorksheet(ref complaintsSummaryWorksheet);

				workbook.SaveAs(path);
			}
		}

		public static OksDailyReport Create(
			IUnitOfWork uow,
			DateTime date,
			IComplaintsRepository complaintsRepository)
		{
			var report = new OksDailyReport();

			report.Date = date;

			report.ComplaintsDataForDate =
				complaintsRepository.GetClientComplaintsForPeriod(uow, date, date.LatestDayTime()).ToList();

			report.ComplaintsDataFromMonthBeginningToDate =
				complaintsRepository.GetClientComplaintsForPeriod(uow, date.FirstDayOfMonth(), date.LatestDayTime()).ToList();

			return report;
		}
	}
}
