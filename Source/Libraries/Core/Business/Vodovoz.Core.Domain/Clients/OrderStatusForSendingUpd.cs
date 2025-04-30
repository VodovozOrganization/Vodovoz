using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Статус заказа для отправки УПД
	/// </summary>
	public enum OrderStatusForSendingUpd
	{
		/// <summary>
		/// Доставлен
		/// </summary>
		[Display(Name = "Доставлен")]
		Delivered,
		/// <summary>
		/// В пути
		/// </summary>
		[Display(Name = "В пути")]
		EnRoute
	}

}
