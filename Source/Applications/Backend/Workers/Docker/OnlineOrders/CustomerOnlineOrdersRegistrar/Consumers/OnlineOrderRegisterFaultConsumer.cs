using System;
using System.Threading.Tasks;
using CustomerOnlineOrdersRegistrar.Factories.V3;
using CustomerOnlineOrdersRegistrar.Factories.V4;
using CustomerOrdersApi.Library.Default.Dto.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services.Logistics;
using Vodovoz.Services.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.OnlineOrders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Services.Orders;

namespace CustomerOnlineOrdersRegistrar.Consumers
{
	public class OnlineOrderRegisterFaultConsumer : OnlineOrderConsumer, IConsumer<OnlineOrderInfoDto>
	{
		public OnlineOrderRegisterFaultConsumer(
			ILogger<OnlineOrderRegisterFaultConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrderFactoryV3 onlineOrderFactoryV3,
			IOnlineOrderFactoryV4 onlineOrderFactoryV4,
			IOrderService orderService,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDiscountReasonSettings discountReasonSettings,
			IOnlineOrderRepository onlineOrderRepository,
			IOnlineOrderCancellationReasonSettings onlineOrderCancellationReasonSettings,
			IRouteListService routeListService,
			IOrderFromOnlineOrderValidator onlineOrderValidator
			)
			: base(
				logger,
				unitOfWorkFactory,
				onlineOrderFactoryV3,
				onlineOrderFactoryV4,
				deliveryRulesSettings,
				discountReasonSettings,
				onlineOrderRepository,
				onlineOrderCancellationReasonSettings,
				orderService,
				routeListService,
				onlineOrderValidator)
		{
		}
		
		public async Task Consume(ConsumeContext<OnlineOrderInfoDto> context)
		{
			var message = context.Message;
			Logger.LogInformation("Пробуем обработать онлайн заказ {ExternalOrderId}", message.ExternalOrderId);
			
			try
			{
				await TryRegisterOnlineOrderV3Async(message, context.CancellationToken);
				return;
			}
			catch(Exception e)
			{
				Logger.LogError(e, "Ошибка при повторной обработке сообщения с онлайн заказом {ExternalOrderId}", message.ExternalOrderId);
				message.FaultedMessage = true;
				throw;
			}
		}
	}
}
