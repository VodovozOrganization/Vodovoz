namespace Vodovoz.Permissions
{
	/// <summary>
	/// Права кассы
	/// </summary>
	public static partial class Cash
	{
		internal static string CanEditExpenseAndIncomeDate => "can_edit_cash_income_expense_date";

		/// <summary>
		/// Касса
		/// </summary>
		public static string RoleCashier => "role_сashier";

		/// <summary>
		/// Доступен отчет ДДС
		/// </summary>
		public static string CanGenerateCashFlowDdsReport => "can_generate_cash_flow_dds_report";

		public static string CanGenerateCashReportsForOrganizations => "can_create_cash_reports_for_organisations";
	}
}
