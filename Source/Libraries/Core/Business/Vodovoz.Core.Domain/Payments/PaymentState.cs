using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Состояние платежа
	/// </summary>
	public enum PaymentState
	{
		/// <summary>
		/// Не распределен
		/// </summary>
		[Display(Name = "Нераспределен")]
		undistributed,
		/// <summary>
		/// Распределен
		/// </summary>
		[Display(Name = "Распределен")]
		distributed,
		/// <summary>
		/// Завершен
		/// </summary>
		[Display(Name = "Завершен")]
		completed,
		/// <summary>
		/// Отменен
		/// </summary>
		[Display(Name = "Отменен")]
		Cancelled
	}
}
