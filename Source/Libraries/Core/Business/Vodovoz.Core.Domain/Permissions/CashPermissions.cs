namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// Права кассы
	/// </summary>
	public static partial class CashPermissions
	{
		internal static string CanEditExpenseAndIncomeDate => "can_edit_cash_income_expense_date";

		/// <summary>
		/// Доступен отчет ДДС
		/// </summary>
		public static string CanGenerateCashFlowDdsReport => "can_generate_cash_flow_dds_report";

		public static string CanGenerateCashReportsForOrganizations => "can_create_cash_reports_for_organisations";
	}
}
