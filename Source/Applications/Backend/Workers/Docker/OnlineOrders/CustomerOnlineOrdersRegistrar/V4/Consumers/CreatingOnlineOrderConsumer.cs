using System;
using System.Threading.Tasks;
using CustomerOnlineOrdersRegistrar.V4.Factories;
using CustomerOrdersApi.Library.V4.Dto.Orders;
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

namespace CustomerOnlineOrdersRegistrar.V4.Consumers
{
	public class CreatingOnlineOrderConsumer : OnlineOrderConsumer, IConsumer<CreatingOnlineOrder>
	{
		public CreatingOnlineOrderConsumer(
			ILogger<CreatingOnlineOrderConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,			
			IOnlineOrderFactoryV4 onlineOrderFactoryV4,			
			IDeliveryRulesSettings deliveryRulesSettings,
			IDiscountReasonSettings discountReasonSettings,
			IOnlineOrderRepository onlineOrderRepository,
			IOnlineOrderCancellationReasonSettings onlineOrderCancellationReasonSettings,
			IOrderService orderService,
			IRouteListService routeListService,
			IOnlineOrderValidatorCreator onlineOrderValidatorCreator)
			: base(
				logger,
				unitOfWorkFactory,	
				onlineOrderFactoryV4,				
				deliveryRulesSettings,
				discountReasonSettings,
				onlineOrderRepository,
				onlineOrderCancellationReasonSettings,
				orderService,
				routeListService,
				onlineOrderValidatorCreator)
		{
		}
		
		public async Task Consume(ConsumeContext<CreatingOnlineOrder> context)
		{
			var message = context.Message;
			Logger.LogInformation("Пришел онлайн заказ {ExternalOrderId}, регистрируем...", message.ExternalOrderId);
			
			try
			{
				var onlineOrderIdWithCode = await TryRegisterOnlineOrderAsync(message, context.CancellationToken);
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
