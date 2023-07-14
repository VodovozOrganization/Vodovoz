namespace Vodovoz.Permissions
{
	/// <summary>
	/// Права кассы
	/// </summary>
	public static partial class Cash
	{
		internal static string CanEditExpenseAndIncomeDate => "can_edit_cash_income_expense_date";

		/// <summary>
		/// Доступен отчет ДДС
		/// </summary>
		public static string CanGenerateCashFlowDdsReport => "can_generate_cash_flow_dds_report";
	}
}
