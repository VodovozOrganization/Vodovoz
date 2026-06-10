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
		/// Возврат денежных средств
		/// </summary>
		[Display(Name = "Возврат денежных средств")]
		Refund
	}
}
