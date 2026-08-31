using System;
using System.Threading.Tasks;
using CustomerOnlineOrdersRegistrar.V3.Factories;
using CustomerOrdersApi.Library.Default.Dto.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.OnlineOrders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Factories;
using VodovozBusiness.Services.Orders;

namespace CustomerOnlineOrdersRegistrar.V3.Consumers
{
	public class OnlineOrderRegisterFaultConsumer : OnlineOrderConsumer, IConsumer<OnlineOrderInfoDto>
	{
		public OnlineOrderRegisterFaultConsumer(
			ILogger<OnlineOrderRegisterFaultConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,			
			IOnlineOrderFactoryV3 onlineOrderFactory,			
			IOrderService orderService,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDiscountReasonSettings discountReasonSettings,
			IOnlineOrderRepository onlineOrderRepository,
			IOnlineOrderCancellationReasonSettings onlineOrderCancellationReasonSettings,
			IRouteListService routeListService,
			IOnlineOrderValidatorCreator onlineOrderValidatorCreator
			)
			: base(
				logger,
				unitOfWorkFactory,				
				onlineOrderFactory,
				deliveryRulesSettings,
				discountReasonSettings,
				onlineOrderRepository,
				onlineOrderCancellationReasonSettings,
				orderService,
				routeListService,
				onlineOrderValidatorCreator)
		{
		}
		
		public async Task Consume(ConsumeContext<OnlineOrderInfoDto> context)
		{
			var message = context.Message;
			Logger.LogInformation("Пробуем обработать онлайн заказ {ExternalOrderId}", message.ExternalOrderId);
			
			try
			{
				await TryRegisterOnlineOrderAsync(message, context.CancellationToken);
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
