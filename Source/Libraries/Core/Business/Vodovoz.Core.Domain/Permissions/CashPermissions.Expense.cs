using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class CashPermissions
    {
		public static class Expense
		{
			public static string CanEditDate => CanEditExpenseAndIncomeDate;

			/// <summary>
			/// Возможность редактирвоать дату учета ДДР
			/// </summary>
			[Display(Name = "Возможность редактирвоать дату учета ДДР")]
			public static string CanEditDdrDate => nameof(CanEditDdrDate);
		}
	}
}
