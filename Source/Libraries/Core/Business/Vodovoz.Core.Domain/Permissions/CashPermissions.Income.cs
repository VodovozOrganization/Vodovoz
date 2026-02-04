namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class CashPermissions
    {
		public static class Income
		{
			public static string CanEditDate => CanEditExpenseAndIncomeDate;
		}
	}
}
