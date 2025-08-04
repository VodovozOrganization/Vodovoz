using System;
using System.Threading.Tasks;
using CustomerOnlineOrdersRegistrar.Factories;
using CustomerOrdersApi.Library.V4.Dto.Orders;
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
		
		public async Task Consume(ConsumeContext<OnlineOrderInfoDto> context)
		{
			var message = context.Message;
			Logger.LogInformation("Пришел онлайн заказ {ExternalOrderId}, регистрируем...", message.ExternalOrderId);
			
			try
			{
				var createdOnlineOrderData = TryRegisterOnlineOrder(message);
				await context.RespondAsync(createdOnlineOrderData);
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
