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
	public class CreatingOnlineOrderConsumer : OnlineOrderConsumer, IConsumer<CreatingOnlineOrder>
	{
		public CreatingOnlineOrderConsumer(
			ILogger<CreatingOnlineOrderConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrderFactory onlineOrderFactory,
			IOrderService orderService,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDiscountReasonSettings discountReasonSettings)
			: base(logger, unitOfWorkFactory, onlineOrderFactory, deliveryRulesSettings, discountReasonSettings, orderService)
		{
		}
		
		public async Task Consume(ConsumeContext<CreatingOnlineOrder> context)
		{
			var message = context.Message;
			Logger.LogInformation("Пришел онлайн заказ {ExternalOrderId}, регистрируем...", message.ExternalOrderId);
			
			try
			{
				var createdOnlineOrderId = TryRegisterOnlineOrder(message);
				await context.RespondAsync(CreatedOnlineOrder.Create(createdOnlineOrderId));
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
