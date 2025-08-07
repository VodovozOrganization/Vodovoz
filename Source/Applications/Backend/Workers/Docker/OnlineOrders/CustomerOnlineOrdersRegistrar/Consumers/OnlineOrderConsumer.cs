using System;
using CustomerOnlineOrdersRegistrar.Factories;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Settings.Delivery;
using VodovozBusiness.Services.Orders;
using Vodovoz.Settings.Orders;

namespace CustomerOnlineOrdersRegistrar.Consumers
{
	public abstract class OnlineOrderConsumer
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOnlineOrderFactory _onlineOrderFactory;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IDiscountReasonSettings _discountReasonSettings;
		private readonly IOrderService _orderService;
		
		protected ILogger<OnlineOrderConsumer> Logger { get; }

		protected OnlineOrderConsumer(
			ILogger<OnlineOrderConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrderFactory onlineOrderFactory,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDiscountReasonSettings discountReasonSettings,
			IOrderService orderService)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_onlineOrderFactory = onlineOrderFactory ?? throw new ArgumentNullException(nameof(onlineOrderFactory));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_discountReasonSettings = discountReasonSettings ?? throw new ArgumentNullException(nameof(discountReasonSettings));
			_orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
		}
		
		protected virtual int TryRegisterOnlineOrder(ICreatingOnlineOrder message)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot($"Создание онлайн заказа из ИПЗ {message.Source.GetEnumTitle()}");
			var onlineOrder = _onlineOrderFactory.CreateOnlineOrder(
				uow,
				message,
				_deliveryRulesSettings.FastDeliveryScheduleId,
				_discountReasonSettings.GetSelfDeliveryDiscountReasonId);

			uow.Save(onlineOrder);
			uow.Commit();
				
			var externalOrderId = message.ExternalOrderId;

			Logger.LogInformation("Проводим заказ на основе онлайн заказа {ExternalOrderId}", externalOrderId);
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
					externalOrderId);
			}
			finally
			{
				if(orderId == default)
				{
					Logger.LogInformation(
						"Не удалось оформить заказ на основе онлайн заказа {ExternalOrderId} отправляем на ручное...",
						externalOrderId);
				}
				else
				{
					Logger.LogInformation(
						"Онлайн заказ {ExternalOrderId} оформлен в заказ {OrderId}",
						externalOrderId,
						orderId);
				}
			}

			return onlineOrder.Id;
		}
	}
}
