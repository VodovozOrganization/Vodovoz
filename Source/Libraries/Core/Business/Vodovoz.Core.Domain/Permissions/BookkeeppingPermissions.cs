using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// Бухгалтерия
	/// </summary>
	public static partial class BookkeeppingPermissions
	{
		/// <summary>
		/// Доступ к редактированию финансовых статей расхода для списаний платежей
		/// </summary>
		[Display(Name = "Доступ к редактированию финансовых статей расхода для списаний платежей")]
		public static string CanEditPaymentWriteOffAvailableFinancialExpenseCategories => nameof(CanEditPaymentWriteOffAvailableFinancialExpenseCategories);
	}
}
