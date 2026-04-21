using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerOnlineOrdersRegistrar.V4.Factories;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using QS.DomainModel.UoW;
using QS.Utilities.Debug;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services.Logistics;
using Vodovoz.Services.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.OnlineOrders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Services.Orders;

namespace CustomerOnlineOrdersRegistrar.V4.Consumers
{
	/// <summary>
	/// Общий обработчик онлайн заказа
	/// </summary>
	public abstract class OnlineOrderConsumerV4
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOnlineOrderFactoryV4 _onlineOrderFactory;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IDiscountReasonSettings _discountReasonSettings;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IOnlineOrderCancellationReasonSettings _onlineOrderCancellationReasonSettings;
		private readonly IOrderService _orderService;
		private readonly IRouteListService _routeListService;
		private readonly IOrderFromOnlineOrderValidator _onlineOrderValidator;

		protected ILogger<OnlineOrderConsumerV4> Logger { get; }

		protected OnlineOrderConsumerV4(
			ILogger<OnlineOrderConsumerV4> logger,
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
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_onlineOrderValidator = onlineOrderValidator ?? throw new ArgumentNullException(nameof(onlineOrderValidator));
		}
		
		/// <summary>
		/// Регистрация нового онлайн заказа
		/// </summary>
		/// <param name="message">Данные заказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// (Номер онлайн заказа, код результата)
		/// код результата:
		/// 200 - успешное создание онлайн заказа
		/// 409 - дубль
		/// 500 - неизвестная ошибка
		/// </returns>
		protected virtual async Task<(int OnlineOrderId, int Code)> TryRegisterOnlineOrderAsync(
			ICreatingOnlineOrder message,
			CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot($"Создание онлайн заказа из ИПЗ {message.Source.GetEnumTitle()}");
			// Необходимо сделать асинхронным
			var onlineOrder = _onlineOrderFactory.CreateOnlineOrder(
				uow,
				message,
				_deliveryRulesSettings.FastDeliveryScheduleId,
				_discountReasonSettings.GetSelfDeliveryDiscountReasonId
			);

			var validationResult = _onlineOrderValidator.ValidateOnlineOrder(uow, onlineOrder);
			var externalOrderId = message.ExternalOrderId;
			var needSpecialProcessingDuplicate = NeedSpecialProcessingDuplicate(uow, onlineOrder);

			if(needSpecialProcessingDuplicate != null)
			{
				if(needSpecialProcessingDuplicate == OnlineOrderDuplicateProcess.NeedCancel)
				{
					Logger.LogInformation("Пришел возможный дубль {ExternalOrderId} отменяем", externalOrderId);
					onlineOrder.OnlineOrderStatus = OnlineOrderStatus.Canceled;
					var cancellationReasonId = _onlineOrderCancellationReasonSettings.GetDuplicateOnlineOrderCancellationReasonId;
					onlineOrder.OnlineOrderCancellationReason = await uow.Session
						.GetAsync<OnlineOrderCancellationReason>(cancellationReasonId, cancellationToken);
					
					var notification = OnlineOrderStatusUpdatedNotification.Create(onlineOrder);
					await uow.SaveAsync(notification, cancellationToken: cancellationToken);
				}
				else
				{
					Logger.LogInformation("Пришел возможный дубль {ExternalOrderId} отправляем на ручное", externalOrderId);
				}
			}

			try
			{
				await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);
			}
			catch(Exception e)
			{
				if(e.FindExceptionTypeInInner<MySqlException>() is { ErrorCode: MySqlErrorCode.DuplicateKeyEntry })
				{
					Logger.LogInformation("Пришел дубль уже зарегистрированного заказа {ExternalOrderId}, пропускаем", message.ExternalOrderId);
					return (0, 409);
				}
				
				return (0, 500);
			}

			if(needSpecialProcessingDuplicate != null)
			{
				return (onlineOrder.Id, 200);
			}

			if(onlineOrder.IsNeedConfirmationByCall || validationResult.IsFailure)
			{
				Logger.LogInformation("Отправляем онлайн заказ {ExternalOrderId} на ручное...", externalOrderId);
				return (onlineOrder.Id, 200);
			}
			
			if(onlineOrder.OnlineOrderStatus == OnlineOrderStatus.WaitingForPayment)
			{
				Logger.LogInformation("Пришел онлайн заказ {ExternalOrderId} в ожидании оплаты...", externalOrderId);
				return (onlineOrder.Id, 200);
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
				orderId = await _orderService.TryCreateOrderFromOnlineOrderAndAcceptAsync(
					uow,
					onlineOrder,
					_routeListService,
					cancellationToken
				);
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

			return (onlineOrder.Id, 200);
		}

		private OnlineOrderDuplicateProcess? NeedSpecialProcessingDuplicate(IUnitOfWork uow, OnlineOrder onlineOrder)
		{
			// Необходимо сделать асинхронным
			var clientOnlineOrdersDuplicates = _onlineOrderRepository.GetOnlineOrdersDuplicates(
				uow, 
				onlineOrder, 
				DateTime.Today
			);

			var needCancel = false;
			var toManualProcessing = false;

			foreach(var duplicateOrder in clientOnlineOrdersDuplicates)
			{
				if(duplicateOrder.OnlineOrderStatus == OnlineOrderStatus.Canceled)
				{
					continue;
				}

				if(duplicateOrder.OnlineOrderPaymentType == onlineOrder.OnlineOrderPaymentType
					&& duplicateOrder.OnlinePayment == onlineOrder.OnlinePayment)
				{
					needCancel = true;
				}

				if(duplicateOrder.OnlineOrderPaymentType != onlineOrder.OnlineOrderPaymentType
					|| duplicateOrder.OnlinePayment != onlineOrder.OnlinePayment)
				{
					toManualProcessing = true;
				}
			}

			if(needCancel)
			{
				return OnlineOrderDuplicateProcess.NeedCancel;
			}
			
			if(toManualProcessing)
			{
				return OnlineOrderDuplicateProcess.ToManualProcessing;
			}

			return null;
		}
	}
}
