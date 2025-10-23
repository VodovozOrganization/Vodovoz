using System;
using System.Threading.Tasks;
using CustomerOnlineOrdersRegistrar.Factories;
using CustomerOrdersApi.Library.Dto.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Orders;
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
			IOnlineOrderFactory onlineOrderFactory,
			IOrderService orderService,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDiscountReasonSettings discountReasonSettings,
			IOnlineOrderRepository onlineOrderRepository,
			IOnlineOrderCancellationReasonSettings onlineOrderCancellationReasonSettings)
			: base(
				logger,
				unitOfWorkFactory,
				onlineOrderFactory,
				deliveryRulesSettings,
				discountReasonSettings,
				onlineOrderRepository,
				onlineOrderCancellationReasonSettings,
				orderService)
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
