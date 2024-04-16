﻿using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOnlineOrdersStatusUpdateNotifier.Converters
{
	public class ExternalOrderStatusConverter : IExternalOrderStatusConverter
	{
		public ExternalOrderStatus GetExternalOrderStatus(OnlineOrder onlineOrder)
		{
			if(onlineOrder.Order is null)
			{
				switch(onlineOrder.OnlineOrderStatus)
				{
					case OnlineOrderStatus.New:
						return ExternalOrderStatus.OrderProcessing;
					case OnlineOrderStatus.OrderPerformed:
						return ExternalOrderStatus.OrderPerformed;
					case OnlineOrderStatus.Canceled:
						return ExternalOrderStatus.Canceled;
				}
			}
			
			switch(onlineOrder.Order.OrderStatus)
			{
				case OrderStatus.DeliveryCanceled:
				case OrderStatus.NotDelivered:
				case OrderStatus.Canceled:
					return ExternalOrderStatus.Canceled;
				case OrderStatus.OnTheWay:
					return ExternalOrderStatus.OrderDelivering;
				default:
					return ExternalOrderStatus.OrderPerformed;
			}
		}
	}
}
