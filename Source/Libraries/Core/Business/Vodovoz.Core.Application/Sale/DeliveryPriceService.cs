using System;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Orders.Delivery;
using Vodovoz.Core.Domain.Interfaces.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders.Delivery;
using VodovozBusiness.Domain.Service;

namespace Vodovoz.Core.Application.Sale
{
	public class DeliveryPriceService : IDeliveryPriceService
	{
		private readonly IDeliveryPriceGetter<OnlineOrderDeliveryPriceContext> _onlineOrderDeliveryPriceGetter;
		private readonly IDeliveryPriceGetter<OrderDeliveryPriceContext> _orderDeliveryPriceGetter;

		public DeliveryPriceService(
			IDeliveryPriceGetter<OnlineOrderDeliveryPriceContext> onlineOrderDeliveryPriceGetter,
			IDeliveryPriceGetter<OrderDeliveryPriceContext> orderDeliveryPriceGetter
			)
		{
			_onlineOrderDeliveryPriceGetter =
				onlineOrderDeliveryPriceGetter ?? throw new ArgumentNullException(nameof(onlineOrderDeliveryPriceGetter));
			_orderDeliveryPriceGetter = orderDeliveryPriceGetter ?? throw new ArgumentNullException(nameof(orderDeliveryPriceGetter));
		}
		
		public Result<decimal> GetDeliveryPrice(IUnitOfWork uow, IFreeDeliveryPrice saleSource)
		{
			switch(saleSource)
			{
				case OnlineOrder onlineOrder:
					var onlineOrderContext = DeliveryPriceGetterContext<OnlineOrderDeliveryPriceContext>
						.Create(
							OnlineOrderDeliveryPriceContext.Create(onlineOrder)
							);
					
					return _onlineOrderDeliveryPriceGetter.GetDeliveryPrice(onlineOrderContext);
				case Order order:
					var orderContext = DeliveryPriceGetterContext<OrderDeliveryPriceContext>
						.Create(
							OrderDeliveryPriceContext.Create(uow, order)
							);
					
					return _orderDeliveryPriceGetter.GetDeliveryPrice(orderContext);
			}
			
			throw new ArgumentOutOfRangeException(
				$"{nameof(saleSource)}",
				"Неизвестное значение источника продажи, невозможно подобрать сервис расчета платной доставки");
		}
	}
}
