using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Models.Orders;

namespace VodovozBusiness.Extensions
{
	public static class OnlineOrderExtensions
	{
		public static OrderOrganizationChoice ToOrderOrganizationChoice(
			this OnlineOrder onlineOrder,
			IUnitOfWork uow,
			IOrderSettings orderSettings) =>
			OrderOrganizationChoice.Create(uow, orderSettings, onlineOrder);

		public static ExternalOrderStatus GetExternalOrderStatus(this OnlineOrder onlineOrder)
		{
			if(!onlineOrder.Orders.Any())
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

			switch(onlineOrder.Orders.First().OrderStatus)
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
