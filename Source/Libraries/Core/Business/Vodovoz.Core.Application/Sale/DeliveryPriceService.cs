using System;
using Microsoft.Extensions.DependencyInjection;
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
		private readonly IServiceScope _scope;

		public DeliveryPriceService(IServiceScope scope)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
		}
		
		public Result<decimal> GetDeliveryPrice(IUnitOfWork uow, IFreeDeliveryPrice saleSource)
		{
			switch(saleSource)
			{
				case OnlineOrder onlineOrder:
					var onlineOrderService = _scope.ServiceProvider.GetRequiredService<IDeliveryPriceGetter<OnlineOrderDeliveryPriceContext>>();
					var onlineOrderContext = DeliveryPriceGetterContext<OnlineOrderDeliveryPriceContext>
						.Create(
							OnlineOrderDeliveryPriceContext.Create(onlineOrder)
							);
					
					return onlineOrderService.GetDeliveryPrice(onlineOrderContext);
				case Order order:
					var orderService = _scope.ServiceProvider.GetRequiredService<IDeliveryPriceGetter<OrderDeliveryPriceContext>>();
					var orderContext = DeliveryPriceGetterContext<OrderDeliveryPriceContext>
						.Create(
							OrderDeliveryPriceContext.Create(uow, order)
							);
					
					return orderService.GetDeliveryPrice(orderContext);
			}
			
			throw new ArgumentOutOfRangeException(
				$"{nameof(saleSource)}",
				"Неизвестное значение источника продажи, невозможно подобрать сервис расчета платной доставки");
		}
	}
}
