using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Статус распределения платежа
	/// </summary>
	public enum AllocationStatus
	{
		/// <summary>
		/// Распределено
		/// </summary>
		[Display(Name = "Распределено")]
		Accepted,
		/// <summary>
		/// Распределение отменено
		/// </summary>
		[Display(Name = "Распределение отменено")]
		Cancelled
	}
}
