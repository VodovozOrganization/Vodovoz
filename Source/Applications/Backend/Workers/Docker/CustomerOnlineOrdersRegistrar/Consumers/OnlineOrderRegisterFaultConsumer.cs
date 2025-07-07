using System;
using System.Threading.Tasks;
using CustomerOnlineOrdersRegistrar.Factories;
using CustomerOrdersApi.Library.Dto.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Settings.Delivery;
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
			IDiscountReasonSettings discountReasonSettings)
			: base(logger, unitOfWorkFactory, onlineOrderFactory, deliveryRulesSettings, discountReasonSettings, orderService)
		{
		}
		
		public Task Consume(ConsumeContext<OnlineOrderInfoDto> context)
		{
			var message = context.Message;
			Logger.LogInformation("Пробуем обработать онлайн заказ {ExternalOrderId}", message.ExternalOrderId);
			
			try
			{
				TryRegisterOnlineOrder(message);
				return Task.CompletedTask;
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
