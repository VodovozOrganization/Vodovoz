﻿using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class Report
	{
		public static class Sales
		{
			/// <summary>
			/// Разрешено формировать отчеты по продажам с контактами
			/// </summary>
			[Display(Name = "Разрешено формировать отчеты по продажам с контактами")]
			public static string CanGetContactsInSalesReports => "contacts_in_sales_reports";

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
		}
	}
}
