using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Статус операции возврата (Надо ли?)
	/// </summary>
	public enum RefundStatus
	{
		/// <summary>
		/// Ожидание обработки
		/// </summary>
		[Display(Name = "В обработке")]
		PENDING,

		/// <summary>
		/// Успешно завершен
		/// </summary>
		[Display(Name = "Выполнен успешно")]
		SUCCEEDED,

		/// <summary>
		/// Отменен (для YooKassa)
		/// </summary>
		[Display(Name = "Отменен")]
		CANCELED,

		/// <summary>
		/// Ошибка при выполнении
		/// </summary>
		[Display(Name = "Ошибка")]
		FAIL
	}
}
