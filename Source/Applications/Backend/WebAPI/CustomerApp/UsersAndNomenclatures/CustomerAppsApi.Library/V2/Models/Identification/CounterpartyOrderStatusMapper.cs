using Vodovoz.Domain.Orders;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Преобразование статусов заказа в текстовый вид
	/// </summary>
	public static class CounterpartyOrderStatusMapper
	{
		/// <summary>
		/// Возвращает текстовый статус заказа
		/// </summary>
		public static string GetStatusName(
			OrderStatus orderStatus,
			OrderPaymentStatus paymentStatus)
		{
			if(orderStatus == OrderStatus.Canceled)
			{
				return "Отменён";
			}

			if(IsDelivered(orderStatus))
			{
				return "Доставлен";
			}

			if(paymentStatus == OrderPaymentStatus.Paid)
			{
				return "Оплачен";
			}

			return "Оформлен";
		}

		/// <summary>
		/// Признак доставленного заказа
		/// </summary>
		public static bool IsDelivered(OrderStatus orderStatus)
		{
			return orderStatus == OrderStatus.Shipped
				|| orderStatus == OrderStatus.UnloadingOnStock
				|| orderStatus == OrderStatus.Closed;
		}

		/// <summary>
		/// Признак отменённого заказа
		/// </summary>
		public static bool IsCanceled(OrderStatus orderStatus)
		{
			return orderStatus == OrderStatus.Canceled;
		}

		/// <summary>
		/// Признак оплаченного заказа
		/// </summary>
		public static bool IsPaid(OrderPaymentStatus paymentStatus)
		{
			return paymentStatus == OrderPaymentStatus.Paid;
		}
	}
}
