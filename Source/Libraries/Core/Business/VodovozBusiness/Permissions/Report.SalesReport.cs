namespace Vodovoz.Permissions
{
	public static partial class Report
	{
		public static class SalesReport
		{
			/// <summary>
			/// Разрешено формировать подробный отчет по продажам с телефонами
			/// </summary>
			public static string CanGenerateDetailedReportWithPhones => "phones_in_detailed_sales_report";
		}
	}
}
