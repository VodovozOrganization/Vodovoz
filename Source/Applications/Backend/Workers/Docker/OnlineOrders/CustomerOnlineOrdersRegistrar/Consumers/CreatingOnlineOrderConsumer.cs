using System;
using System.Threading.Tasks;
using CustomerOnlineOrdersRegistrar.Factories.V3;
using CustomerOnlineOrdersRegistrar.Factories.V4;
using CustomerOrdersApi.Library.V4.Dto.Orders;
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
	public class CreatingOnlineOrderConsumer : OnlineOrderConsumer, IConsumer<CreatingOnlineOrder>
	{
		public CreatingOnlineOrderConsumer(
			ILogger<CreatingOnlineOrderConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrderFactoryV3 onlineOrderFactoryV3,
			IOnlineOrderFactoryV4 onlineOrderFactoryV4,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDiscountReasonSettings discountReasonSettings,
			IOnlineOrderRepository onlineOrderRepository,
			IOnlineOrderCancellationReasonSettings onlineOrderCancellationReasonSettings,
			IOrderService orderService,
			IRouteListService routeListService,
			IOrderFromOnlineOrderValidator onlineOrderValidator)
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
		
		public async Task Consume(ConsumeContext<CreatingOnlineOrder> context)
		{
			var message = context.Message;
			Logger.LogInformation("Пришел онлайн заказ {ExternalOrderId}, регистрируем...", message.ExternalOrderId);
			
			try
			{
				var onlineOrderIdWithCode = await TryRegisterOnlineOrderV4Async(message, context.CancellationToken);
				await context.RespondAsync(CreatedOnlineOrderResult.Create(onlineOrderIdWithCode));
			}
			catch(Exception e)
			{
				//TODO: проверить работу вброса ошибки
				Logger.LogError(e, "Ошибка при регистрации онлайн заказа {ExternalOrderId}", message.ExternalOrderId);
				throw;
			}
		}
	}
}
