namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class Cash
    {
		public static class Income
		{
			public static string CanEditDate => CanEditExpenseAndIncomeDate;
		}
	}
}
