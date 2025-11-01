using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Converters
{
	public class ExternalOrderStatusConverter : IExternalOrderStatusConverter
	{
		public ExternalOrderStatus ConvertOnlineOrderStatus(OnlineOrderStatus onlineOrderStatus)
		{
			switch(onlineOrderStatus)
			{
				case OnlineOrderStatus.New:
					return ExternalOrderStatus.OrderProcessing;
				case OnlineOrderStatus.OrderPerformed:
					return ExternalOrderStatus.OrderPerformed;
				default:
					return ExternalOrderStatus.Canceled;
			}
		}
		
		public ExternalOrderStatus ConvertOrderStatus(OrderStatus orderStatus)
		{
			switch(orderStatus)
			{
				case OrderStatus.Canceled:
				case OrderStatus.NotDelivered:
				case OrderStatus.DeliveryCanceled:
					return ExternalOrderStatus.Canceled;
				case OrderStatus.Accepted:
				case OrderStatus.InTravelList:
					return ExternalOrderStatus.OrderPerformed;
				case OrderStatus.Shipped:
				case OrderStatus.Closed:
				case OrderStatus.UnloadingOnStock:
					return ExternalOrderStatus.OrderCompleted;
				case OrderStatus.WaitForPayment:
					return ExternalOrderStatus.WaitingForPayment;
				case OrderStatus.OnTheWay:
					return ExternalOrderStatus.OrderDelivering;
				case OrderStatus.OnLoading:
					return ExternalOrderStatus.OrderCollecting;
				default:
					return ExternalOrderStatus.OrderProcessing;
			}
		}
	}
}
