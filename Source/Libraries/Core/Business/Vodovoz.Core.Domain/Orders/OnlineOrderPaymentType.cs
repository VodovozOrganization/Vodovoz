using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Формы оплат онлайн заказа
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum OnlineOrderPaymentType
	{
		/// <summary>
		/// Наличная
		/// </summary>
		[Display(Name = "Наличная")]
		Cash,
		/// <summary>
		/// Терминал
		/// </summary>
		[Display(Name = "Терминал")]
		Terminal,
		/// <summary>
		/// Оплачено онлайн
		/// </summary>
		[Display(Name = "Оплачено онлайн")]
		PaidOnline
	}
}
