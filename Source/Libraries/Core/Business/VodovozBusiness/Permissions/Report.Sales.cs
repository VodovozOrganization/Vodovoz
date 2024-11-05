namespace Vodovoz.Permissions
{
	public static partial class Report
	{
		public static class Sales
		{
			/// <summary>
			/// Разрешено формировать отчеты по продажам с контактами
			/// </summary>
			public static string CanGetContactsInSalesReports => "contacts_in_sales_reports";
		}
	}
}
