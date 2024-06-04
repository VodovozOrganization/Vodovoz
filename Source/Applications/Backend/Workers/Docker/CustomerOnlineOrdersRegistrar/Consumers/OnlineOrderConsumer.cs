using System;
using CustomerOnlineOrdersRegistrar.Factories;
using CustomerOrdersApi.Library.Dto.Orders;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Settings.Delivery;

namespace CustomerOnlineOrdersRegistrar.Consumers
{
	public abstract class OnlineOrderConsumer
	{
		protected readonly ILogger<OnlineOrderConsumer> Logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOnlineOrderFactory _onlineOrderFactory;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IOrderService _orderService;

		protected OnlineOrderConsumer(
			ILogger<OnlineOrderConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrderFactory onlineOrderFactory,
			IDeliveryRulesSettings deliveryRulesSettings,
			IOrderService orderService)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_onlineOrderFactory = onlineOrderFactory ?? throw new ArgumentNullException(nameof(onlineOrderFactory));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
		}
		
		protected virtual void TryRegisterOnlineOrder(OnlineOrderInfoDto message)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var onlineOrder = _onlineOrderFactory.CreateOnlineOrder(uow, message, _deliveryRulesSettings.FastDeliveryScheduleId);

				uow.Save(onlineOrder);
				uow.Commit();

				Logger.LogInformation("Проводим заказ на основе онлайн заказа {ExternalOrderId}", message.ExternalOrderId);
				var orderId = 0;
				
				try
				{
					orderId = _orderService.TryCreateOrderFromOnlineOrderAndAccept(uow, onlineOrder);
				}
				catch(Exception e)
				{
					Logger.LogError(
						e,
						"Возникла ошибка при подтверждении заказа на основе онлайн заказа {ExternalOrderId}",
						message.ExternalOrderId);
				}
				finally
				{
					if(orderId == default)
					{
						Logger.LogInformation(
							"Не удалось оформить заказ на основе онлайн заказа {ExternalOrderId} отправляем на ручное...",
							message.ExternalOrderId);
					}
					else
					{
						Logger.LogInformation(
							"Онлайн заказ {ExternalOrderId} оформлен в заказ {OrderId}",
							message.ExternalOrderId,
							orderId);
					}
				}
			}
		}
	}
}
