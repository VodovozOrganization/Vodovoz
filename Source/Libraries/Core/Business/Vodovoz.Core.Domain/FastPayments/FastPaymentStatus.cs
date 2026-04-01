using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.FastPayments
{
	public enum FastPaymentStatus
	{
		/// <summary>
		/// Обрабатывается
		/// </summary>
		[Display(Name = "Обрабатывается")]
		Processing = 1,

		/// <summary>
		/// Отбракован
		/// </summary>
		[Display(Name = "Отбракован")]
		Rejected,

		/// <summary>
		/// Возврат
		/// </summary>
		[Display(Name = "Возврат")]
		Refund,

		/// <summary>
		/// Исполнен
		/// </summary>
		[Display(Name = "Исполнен")]
		Performed
	}
}
