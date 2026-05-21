using System;
using System.Threading.Tasks;
using CustomerOnlineOrdersRegistrar.V3.Factories;
using CustomerOrders.Contracts.Default.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using QS.DomainModel.UoW;
using QS.Utilities.Debug;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services.Logistics;
using Vodovoz.Services.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.OnlineOrders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Services.Orders;

namespace CustomerOnlineOrdersRegistrar.V3.Consumers
{
	/// <summary>
	/// Конкретный обработчик онлайн заказа
	/// </summary>
	public class OnlineOrderRegisteredConsumer : OnlineOrderConsumerV3, IConsumer<OnlineOrderInfoDto>
	{
		private readonly IBus _bus;

		public OnlineOrderRegisteredConsumer(
			ILogger<OnlineOrderRegisteredConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrderFactoryV3 onlineOrderFactory,
			IOrderService orderService,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDiscountReasonSettings discountReasonSettings,
			IOnlineOrderRepository onlineOrderRepository,
			IOnlineOrderCancellationReasonSettings onlineOrderCancellationReasonSettings,
			IRouteListService routeListService,
			IOrderFromOnlineOrderValidator onlineOrderValidator,
			IOnlineOrderTemplateFromOnlineOrderValidator onlineOrderTemplateValidator,
			IBus bus)
				: base(
					logger,
					unitOfWorkFactory,
					onlineOrderFactoryV4,
					onlineOrderFactoryV5,
					deliveryRulesSettings,
					discountReasonSettings,
					onlineOrderRepository,
					onlineOrderCancellationReasonSettings,
					orderService,
					routeListService,
					onlineOrderValidator,
					onlineOrderTemplateValidator
				)
		{
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}
		
		/// <summary>
		/// Обработка входящего сообщения с данными по заказу
		/// </summary>
		/// <param name="context">Контекст с данными по заказу</param>
		public async Task Consume(ConsumeContext<OnlineOrderInfoDto> context)
		{
			var message = context.Message;
			Logger.LogInformation("Пришел онлайн заказ {ExternalOrderId} от пользователя {ExternalCounterpartyId} клиента {ClientId} " +
				"с контактным номером {ContactPhone}, регистрируем...",
				message.ExternalOrderId,
				message.ExternalCounterpartyId,
				message.CounterpartyErpId,
				message.ContactPhone);
			
			try
			{
				await TryRegisterOnlineOrderV4Async(message, context.CancellationToken);
				return;
			}
			catch(Exception e)
			{
				if(e.FindExceptionTypeInInner<MySqlException>() is { ErrorCode: MySqlErrorCode.DuplicateKeyEntry })
				{
					Logger.LogInformation("Пришел дубль уже зарегистрированного заказа {ExternalOrderId}, пропускаем", message.ExternalOrderId);
					return;
				}
				
				Logger.LogError(e, "Ошибка при регистрации онлайн заказа {ExternalOrderId}", message.ExternalOrderId);
				message.FaultedMessage = true;
				await _bus.Publish(message, context.CancellationToken);
				return;
			}
		}
	}
}
