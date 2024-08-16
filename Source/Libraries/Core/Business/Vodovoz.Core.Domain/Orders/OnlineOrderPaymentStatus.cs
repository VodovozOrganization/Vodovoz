using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Vodovoz.Core.Domain.Orders
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum OnlineOrderPaymentStatus
	{
		[Display(Name = "Не оплачен")]
		UnPaid,
		[Display(Name = "Оплачен")]
		Paid
	}
}
