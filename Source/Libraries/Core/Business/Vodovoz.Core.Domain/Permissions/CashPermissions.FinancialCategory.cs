namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class CashPermissions
	{
		public static class FinancialCategory
		{
			public static string HasAccessToHiddenFinancialCategories => "has_access_to_hidden_financial_categories";

			public static string CanChangeFinancialExpenseCategory => "can_change_financial_expense_category";
		}
	}
}
