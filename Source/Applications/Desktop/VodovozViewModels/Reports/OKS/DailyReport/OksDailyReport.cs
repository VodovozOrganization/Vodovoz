using System;

namespace Vodovoz.ViewModels.Reports.OKS.DailyReport
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
