using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders.OnlineOrders
{
	/// <summary>
	/// Статус шаблона автозаказа
	/// </summary>
	public enum OnlineOrderTemplateStatus
	{
		/// <summary>
		/// Активен
		/// </summary>
		[Display(Name = "Активен")]
		Active,
		/// <summary>
		/// Приостановлен
		/// </summary>
		[Display(Name = "Приостановлен")]
		Inactive
	}
}
