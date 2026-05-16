using CustomerOrders.Contracts;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Converters
{
	public class ExternalOrderStatusConverter : IExternalOrderStatusConverter
	{
		public ExternalCustomerOrderStatus ConvertOnlineOrderStatus(OnlineOrderStatus onlineOrderStatus)
		{
			switch(onlineOrderStatus)
			{
				case OnlineOrderStatus.New:
					return ExternalCustomerOrderStatus.OrderProcessing;
				case OnlineOrderStatus.OrderPerformed:
					return ExternalCustomerOrderStatus.OrderPerformed;
				case OnlineOrderStatus.WaitingForPayment:
					return ExternalCustomerOrderStatus.WaitingForPayment;
				default:
					return ExternalCustomerOrderStatus.Canceled;
			}
		}
		
		public ExternalCustomerOrderStatus ConvertOrderStatus(OrderStatus orderStatus)
		{
			switch(orderStatus)
			{
				case OrderStatus.Canceled:
				case OrderStatus.NotDelivered:
				case OrderStatus.DeliveryCanceled:
					return ExternalCustomerOrderStatus.Canceled;
				case OrderStatus.Accepted:
				case OrderStatus.InTravelList:
					return ExternalCustomerOrderStatus.OrderPerformed;
				case OrderStatus.Shipped:
				case OrderStatus.Closed:
				case OrderStatus.UnloadingOnStock:
					return ExternalCustomerOrderStatus.OrderCompleted;
				case OrderStatus.WaitForPayment:
					return ExternalCustomerOrderStatus.WaitingForPayment;
				case OrderStatus.OnTheWay:
					return ExternalCustomerOrderStatus.OrderDelivering;
				case OrderStatus.OnLoading:
					return ExternalCustomerOrderStatus.OrderCollecting;
				default:
					return ExternalCustomerOrderStatus.OrderProcessing;
			}
		}
	}
}
