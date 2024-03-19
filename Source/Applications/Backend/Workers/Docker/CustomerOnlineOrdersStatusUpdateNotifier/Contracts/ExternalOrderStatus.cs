using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Contracts
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ExternalOrderStatus
	{
		[Display(Name = "Заказ оформляется")]
		OrderProcessing,
		[Display(Name = "Заказ оформлен")]
		OrderPerformed,
		[Display(Name = "Заказ доставляется")]
		OrderDelivering,
		[Display(Name = "Отменен")]
		Canceled
	}
}
