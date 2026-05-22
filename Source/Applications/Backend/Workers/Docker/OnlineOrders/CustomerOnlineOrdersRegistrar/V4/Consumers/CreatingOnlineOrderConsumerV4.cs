using System;
using System.Threading.Tasks;
using CustomerOnlineOrdersRegistrar.V4.Factories;
using CustomerOrders.Contracts.V4.Orders;
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

namespace CustomerOnlineOrdersRegistrar.V4.Consumers
{
	public class CreatingOnlineOrderConsumerV4 : OnlineOrderConsumerV4, IConsumer<CreatingOnlineOrder>
	{
		public CreatingOnlineOrderConsumerV4(
			ILogger<CreatingOnlineOrderConsumerV4> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrderFactoryV4 onlineOrderFactory,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDiscountReasonSettings discountReasonSettings,
			IOnlineOrderRepository onlineOrderRepository,
			IOnlineOrderCancellationReasonSettings onlineOrderCancellationReasonSettings,
			IOrderService orderService,
			IRouteListService routeListService,
			IOrderFromOnlineOrderValidator onlineOrderValidator
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
				onlineOrderValidator
			)
		{
		}
		
		/// <summary>
		/// Обработка входящего сообщения с данными по заказу
		/// </summary>
		/// <param name="context">Контекст с данными по заказу</param>
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
				Logger.LogError(e, "Ошибка при регистрации онлайн заказа {ExternalOrderId}", message.ExternalOrderId);
				throw;
			}
		}
	}
}
