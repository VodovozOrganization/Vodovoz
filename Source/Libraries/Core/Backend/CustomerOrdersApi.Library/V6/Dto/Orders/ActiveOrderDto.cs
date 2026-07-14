using Vodovoz.Core.Data.Orders.V6;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V6.Dto.Orders
{
	/// <summary>
	/// Информация об активном заказе
	/// </summary>
	public class ActiveOrderDto : OrderDto
	{
		/// <summary>
		/// Текстовое сообщение о статусе заказа
		/// </summary>
		public string TextStatusMessage { get; private set; }

		public void UpdateTextStatusMessage(bool establishedRoute, bool isOrderWasSelectedAsNext)
		{
			switch(OrderStatus)
			{
				case ExternalOrderStatus.OrderProcessing:
					TextStatusMessage = "Заказ оформлен";
					break;
				case ExternalOrderStatus.OrderDelivering:
					TextStatusMessage =
						establishedRoute ? "Курьер направляется к Вам"
						: isOrderWasSelectedAsNext
							? "Курьер задерживается"
							: "Заказ в пути";
					break;
				default:
					TextStatusMessage = string.Empty;
					break;
			}
		}
	}
}
