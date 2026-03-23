using Vodovoz.Core.Data.Orders.V4;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	/// <summary>
	/// Информация об активном заказе
	/// </summary>
	public class ActiveOrderDto : OrderDto
	{
		/// <summary>
		/// Маршрут проложен водителем до точки доставки
		/// </summary>
		public bool EstablishedRoute { get; private set; }

		/// <summary>
		/// Текстовое сообщение о статусе заказа
		/// </summary>
		public string TextStatusMessage { get; private set; }

		public void UpdateDriverRoute(bool establishedRoute)
		{
			EstablishedRoute = establishedRoute;
		}

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
