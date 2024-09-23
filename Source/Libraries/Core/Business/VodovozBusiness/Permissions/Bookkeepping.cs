using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	/// <summary>
	/// Бухгалтерия
	/// </summary>
	public static partial class Bookkeepping
	{
		/// <summary>
		/// Доступ к редактированию финансовых статей расхода для списаний платежей
		/// </summary>
		[Display(Name = "Доступ к редактированию финансовых статей расхода для списаний платежей")]
		public static string CanEditPaymentWriteOffAvailableFinancialExpenseCategories => nameof(CanEditPaymentWriteOffAvailableFinancialExpenseCategories);
	}
}
