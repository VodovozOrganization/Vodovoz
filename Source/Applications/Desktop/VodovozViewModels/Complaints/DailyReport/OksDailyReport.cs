using System;

namespace Vodovoz.ViewModels.Complaints.DailyReport
{
	public class OksDailyReport
	{
		private OksDailyReport()
		{

		}

		public static OksDailyReport Create()
		{
			return new OksDailyReport();
		}

		public static string GetReportTitle(DateTime reportDate) =>
			$"Отчет по рекламациям ОКС {reportDate.ToString("dd.MM.yyyy")}";
	}
}
