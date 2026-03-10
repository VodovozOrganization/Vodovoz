using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Vodovoz.Core.Domain.Orders
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum OnlineOrderPaymentStatus
	{
		/// <summary>
		/// Не оплачен
		/// </summary>
		[Display(Name = "Не оплачен")]
		UnPaid,

		/// <summary>
		/// Оплачен
		/// </summary>
		[Display(Name = "Оплачен")]
		Paid,

		/// <summary>
		/// Возврат в процессе
		/// </summary>
		[Display(Name = "Возврат в процессе")]
		Refunding,

		/// <summary>
		/// Средства возвращены полностью
		/// </summary>
		[Display(Name = "Средства возвращены полностью")]
		Refunded
	}
}
