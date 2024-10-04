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
	public class OnlineOrderRegisteredConsumer : OnlineOrderConsumer, IConsumer<OnlineOrderInfoDto>
	{
		private readonly IBus _bus;

		public OnlineOrderRegisteredConsumer(
			ILogger<OnlineOrderRegisteredConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrderFactory onlineOrderFactory,
			IOrderService orderService,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDiscountReasonSettings discountReasonSettings,
			IBus bus) : base(logger, unitOfWorkFactory, onlineOrderFactory, deliveryRulesSettings, discountReasonSettings, orderService)
		{
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}
		
		public Task Consume(ConsumeContext<OnlineOrderInfoDto> context)
		{
			var message = context.Message;
			_logger.LogInformation("Пришел онлайн заказ {ExternalOrderId}, регистрируем...", message.ExternalOrderId);
			
			try
			{
				TryRegisterOnlineOrder(message);
				return Task.CompletedTask;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при регистрации онлайн заказа {ExternalOrderId}", message.ExternalOrderId);
				message.FaultedMessage = true;
				_bus.Publish<OnlineOrderInfoDto>(message);
				return Task.CompletedTask;
			}
		}
	}
}
