using System;
using System.Linq;
using CustomerOnlineOrdersRegistrar.Factories;
using CustomerOrdersApi.Library.Dto.Orders;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.OnlineOrders;
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
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IOnlineOrderCancellationReasonSettings _onlineOrderCancellationReasonSettings;
		private readonly IOrderService _orderService;
		
		protected ILogger<OnlineOrderConsumer> Logger { get; }

		protected OnlineOrderConsumer(
			ILogger<OnlineOrderConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrderFactory onlineOrderFactory,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDiscountReasonSettings discountReasonSettings,
			IOnlineOrderRepository onlineOrderRepository,
			IOnlineOrderCancellationReasonSettings onlineOrderCancellationReasonSettings,
			IOrderService orderService)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_onlineOrderFactory = onlineOrderFactory ?? throw new ArgumentNullException(nameof(onlineOrderFactory));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_discountReasonSettings = discountReasonSettings ?? throw new ArgumentNullException(nameof(discountReasonSettings));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_onlineOrderCancellationReasonSettings =
				onlineOrderCancellationReasonSettings ?? throw new ArgumentNullException(nameof(onlineOrderCancellationReasonSettings));
			_orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
		}
		
		protected virtual void TryRegisterOnlineOrder(OnlineOrderInfoDto message)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot($"Создание онлайн заказа из ИПЗ {message.Source.GetEnumTitle()}"))
			{
				var onlineOrder = _onlineOrderFactory.CreateOnlineOrder(
					uow,
					message,
					_deliveryRulesSettings.FastDeliveryScheduleId,
					_discountReasonSettings.GetSelfDeliveryDiscountReasonId);

				var clientOnlineOrdersDuplicates =
					_onlineOrderRepository.GetOnlineOrdersDuplicates(uow, onlineOrder, DateTime.Today);

				var externalOrderId = message.ExternalOrderId;
				
				if(clientOnlineOrdersDuplicates.Any())
				{
					Logger.LogInformation("Пришел возможный дубль {ExternalOrderId} отменяем", externalOrderId);
					onlineOrder.OnlineOrderStatus = OnlineOrderStatus.Canceled;
					onlineOrder.OnlineOrderCancellationReason =
						uow.GetById<OnlineOrderCancellationReason>(_onlineOrderCancellationReasonSettings.GetDuplicateOnlineOrderCancellationReasonId);
					
					var notification = OnlineOrderStatusUpdatedNotification.Create(onlineOrder);
					uow.Save(notification);
				}

				uow.Save(onlineOrder);
				uow.Commit();

				if(clientOnlineOrdersDuplicates.Any())
				{
					return;
				}

				Logger.LogInformation("Проводим заказ на основе онлайн заказа {ExternalOrderId} от пользователя {ExternalCounterpartyId}" +
					" клиента {ClientId} с контактным номером {ContactPhone}",
					externalOrderId,
					message.ExternalCounterpartyId,
					message.CounterpartyErpId,
					message.ContactPhone);
				
				var orderId = 0;
				
				try
				{
					orderId = _orderService.TryCreateOrderFromOnlineOrderAndAccept(uow, onlineOrder);
				}
				catch(Exception e)
				{
					Logger.LogError(
						e,
						"Возникла ошибка при подтверждении заказа на основе онлайн заказа {ExternalOrderId} от пользователя {ExternalCounterpartyId}" +
						" клиента {ClientId} с контактным номером {ContactPhone}",
						externalOrderId,
						message.ExternalCounterpartyId,
						message.CounterpartyErpId,
						message.ContactPhone);
				}
				finally
				{
					if(orderId == default)
					{
						Logger.LogInformation(
							"Не удалось оформить заказ на основе онлайн заказа {ExternalOrderId} от пользователя {ExternalCounterpartyId}" +
							" клиента {ClientId} с контактным номером {ContactPhone} отправляем на ручное...",
							externalOrderId,
							message.ExternalCounterpartyId,
							message.CounterpartyErpId,
							message.ContactPhone);
					}
					else
					{
						Logger.LogInformation(
							"Онлайн заказ {ExternalOrderId} от пользователя {ExternalCounterpartyId} клиента {ClientId}" +
							" с контактным номером {ContactPhone} оформлен в заказ {OrderId}",
							externalOrderId,
							message.ExternalCounterpartyId,
							message.CounterpartyErpId,
							message.ContactPhone,
							orderId);
					}
				}
			}
		}
	}
}
