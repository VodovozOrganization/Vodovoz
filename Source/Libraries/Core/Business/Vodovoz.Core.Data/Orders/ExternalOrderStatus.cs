using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Vodovoz.Core.Data.Orders
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ExternalOrderStatus
	{
		[Display(Name = "Заказ оформляется")]
		OrderProcessing,
		[Display(Name = "Заказ оформлен")]
		OrderPerformed,
		[Display(Name = "Ожидание оплаты")]
		WaitingForPayment,
		[Display(Name = "Заказ собирается")]
		OrderCollecting,
		[Display(Name = "Заказ доставляется")]
		OrderDelivering,
		[Display(Name = "Заказ выполнен")]
		OrderCompleted,
		[Display(Name = "Отменен")]
		Canceled
	}
}
