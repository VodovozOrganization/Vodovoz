using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class ReportPermissions
	{
		public static class Sales
		{
			/// <summary>
			/// Разрешено формировать отчеты с контактами
			/// </summary>
			[Display(Name = "Разрешено формировать отчеты с контактами")]
			public static string CanGetContactsInReports => "contacts_in_reports";

			/// <summary>
			/// Доступ к финансовой отчетности компании
			/// </summary>
			[Display(Name = "Доступ к финансовой отчетности компании")]
			public static string CanAccessSalesReports => "can_access_sales_reports";

			/// <summary>
			/// Просмотр продаж с чеками в отчётах
			/// </summary>
			[Display(Name = "Просмотр продаж с чеками в отчётах")]
			public static string CanViewReportSalesWithCashReceipts => "CanViewReportSalesWithCashReceipts";

			/// <summary>
			/// Выбор автора заказа в отчете по мотивации КЦ
			/// </summary>
			[Display(
				Name = "Выбор автора заказа в отчете по мотивации КЦ",
				Description = "Пользователь может выбирать автора заказа в отчете по мотивации КЦ")]
			public static string CanSelectTheOrderAuthorInCallCenterMotivationReport => nameof(CanSelectTheOrderAuthorInCallCenterMotivationReport);
		}
	}
}
