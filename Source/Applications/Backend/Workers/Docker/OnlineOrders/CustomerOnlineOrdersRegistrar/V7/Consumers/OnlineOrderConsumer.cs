using System;
using System.Collections.Generic;
using CustomerOrdersApi.Library.V7.Dto.Orders;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;
using Mindbox.Library.Mapping;
using Mindbox.Library.Services;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Database.Mindbox;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.OnlineOrders;
using VodovozBusiness.Services.Orders;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using CustomerOnlineOrdersRegistrar.V7.Factories;
using MySqlConnector;
using QS.Utilities.Debug;
using Vodovoz.Services.Logistics;
using VodovozBusiness.Factories;

namespace CustomerOnlineOrdersRegistrar.V7.Consumers
{
	public abstract class OnlineOrderConsumer
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOnlineOrderFactory _onlineOrderFactory;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IOnlineOrderCancellationReasonSettings _onlineOrderCancellationReasonSettings;
		private readonly IOrderService _orderService;
		private readonly IRouteListService _routeListService;
		private readonly IOnlineOrderValidatorCreator _onlineOrderValidatorCreator;
		private readonly IMindboxAuthorizedLoyaltyOrderService _mindboxLoyaltyOrderService;
		private readonly IMindboxSettings _mindboxSettings;
		private readonly INomenclatureSettings _nomenclatureSettings;

		protected ILogger<OnlineOrderConsumer> Logger { get; }
		protected string LastErrorCode { get; private set; }
		protected int LastBonusesWrittenOff { get; private set; }
		protected int? LastBonusesToEarn { get; private set; }

		protected OnlineOrderConsumer(
			ILogger<OnlineOrderConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrderFactory onlineOrderFactory,
			IDeliveryRulesSettings deliveryRulesSettings,
			IOnlineOrderRepository onlineOrderRepository,
			IOnlineOrderCancellationReasonSettings onlineOrderCancellationReasonSettings,
			IOrderService orderService,
			IRouteListService routeListService,
			IOnlineOrderValidatorCreator onlineOrderValidatorCreator,
			IMindboxAuthorizedLoyaltyOrderService mindboxLoyaltyOrderService,
			IMindboxSettings mindboxSettings,
			INomenclatureSettings nomenclatureSettings
			)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_onlineOrderFactory = onlineOrderFactory ?? throw new ArgumentNullException(nameof(onlineOrderFactory));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_onlineOrderCancellationReasonSettings =
				onlineOrderCancellationReasonSettings ?? throw new ArgumentNullException(nameof(onlineOrderCancellationReasonSettings));
			_orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_onlineOrderValidatorCreator = onlineOrderValidatorCreator ?? throw new ArgumentNullException(nameof(onlineOrderValidatorCreator));
			_mindboxLoyaltyOrderService = mindboxLoyaltyOrderService ?? throw new ArgumentNullException(nameof(mindboxLoyaltyOrderService));
			_mindboxSettings = mindboxSettings ?? throw new ArgumentNullException(nameof(mindboxSettings));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
		}		

		protected virtual async Task<(int OnlineOrderId, int Code)> TryRegisterOnlineOrderAsync(ICreatingOnlineOrder message, CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot($"Создание онлайн заказа из ИПЗ {message.Source.GetEnumTitle()}");
			// Необходимо сделать асинхронным
			var onlineOrder = _onlineOrderFactory.CreateOnlineOrder(
				uow,
				message,
				_deliveryRulesSettings.FastDeliveryScheduleId
			);

			var requestedBonuses = message.BonusesToWriteOff ?? 0;
			var itemsBonuses = message.OnlineOrderItems?.Sum(item => item.BonusesWrittenOff ?? 0) ?? 0;

			if(requestedBonuses < 0
				|| itemsBonuses != requestedBonuses
				|| (message.OnlineOrderItems?.Any(item => IsDeliveryItem(item.ErpId) && (item.BonusesWrittenOff ?? 0) != 0) ?? false))
			{
				LastErrorCode = "BonusesInvalid";
				return (0, 400);
			}

			if(requestedBonuses > 0)
			{
				onlineOrder.MindboxTransactionId = Guid.NewGuid();
			}

			var validationResult = _onlineOrderValidatorCreator
				.Create(onlineOrder)
				.Validate(uow);
			
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

			if(requestedBonuses > 0)
			{
				if(!CanApplyBonuses(uow, message))
				{
					LastErrorCode = "BonusesChanged";
					return (0, 409);
				}

				var mindboxResult = await CreateMindboxOrderAsync(uow, onlineOrder, message, cancellationToken);

				if(!mindboxResult.IsSuccess)
				{
					try
					{
						uow.Session.Delete(onlineOrder);
						await uow.CommitAsync(cancellationToken);
					}
					catch(Exception e)
					{
						Logger.LogError(e, "Не удалось удалить онлайн заказ {OnlineOrderId} после ошибки Mindbox", onlineOrder.Id);
					}

					LastErrorCode = mindboxResult.Outcome == MindboxLoyaltyOrderOutcome.BonusesChanged
						? "BonusesChanged"
						: "BonusServiceUnavailable";

					LastBonusesWrittenOff = 0;
					LastBonusesToEarn = null;

					return (0, mindboxResult.Outcome == MindboxLoyaltyOrderOutcome.BonusesChanged ? 409 : 503);
				}

				onlineOrder.MindboxOrderId = mindboxResult.MindboxOrderId;
				onlineOrder.BonusesWrittenOff = mindboxResult.BonusesWrittenOff;
				onlineOrder.BonusesToEarn = mindboxResult.BonusesToEarn;
				onlineOrder.MindboxStatus = Vodovoz.Core.Domain.Clients.Mindbox.MindboxCounterpartyOutboxStatus.Sent;
				onlineOrder.MindboxLastAttemptAt = DateTime.UtcNow;
				onlineOrder.MindboxAttempts++;
				onlineOrder.MindboxLastHttpStatusCode = mindboxResult.HttpStatusCode;
				onlineOrder.MindboxLastStatus = mindboxResult.Status;
				onlineOrder.MindboxLastErrorId = mindboxResult.ErrorId;

				LastBonusesWrittenOff = mindboxResult.BonusesWrittenOff;
				LastBonusesToEarn = mindboxResult.BonusesToEarn;

				await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);
			}

			if(validationResult.IsFailure)
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
				message.ErpCounterpartyId,
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
					message.ErpCounterpartyId,
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

		private async Task<MindboxLoyaltyOrderResult> CreateMindboxOrderAsync(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			ICreatingOnlineOrder message,
			CancellationToken cancellationToken)
		{
			var lines = new List<MindboxLoyaltyOrderLine>();
			var lineNumber = 0;

			foreach(var item in onlineOrder.OnlineOrderItems.Where(item => !IsDeliveryItem(item.NomenclatureId)).OrderBy(item => item.Id))
			{
				var product = item.PromoSet is not null
					? MindboxItemIdBuilder.Build(item.PromoSet)
					: item.Nomenclature is not null
						? MindboxItemIdBuilder.Build(item.Nomenclature)
						: null;

				if(product is null)
				{
					continue;
				}

				lineNumber++;

				lines.Add(new MindboxLoyaltyOrderLine
				{
					LineId = item.Id.ToString(),
					LineNumber = lineNumber,
					Product = new MindboxProduct
					{
						Ids = new Dictionary<string, string>(StringComparer.Ordinal)
						{
							[_mindboxSettings.BonusItemIdName] = product
						}
					},
					Quantity = item.Count,
					QuantityType = "int",
					BasePricePerItem = item.Count == 0
						? 0
						: Math.Round((item.CurrentSum + item.BonusesWrittenOff) / item.Count, 2),
					Status = new MindboxLoyaltyOrderLineStatus
					{
						Ids = new MindboxLoyaltyOrderLineStatusIds
						{
							ExternalId = onlineOrder.OnlineOrderPaymentType == OnlineOrderPaymentType.PaidOnline
								&& onlineOrder.OnlineOrderPaymentStatus == OnlineOrderPaymentStatus.UnPaid
									? "WaitingForPayment"
									: "Created"
						}
					}
				});
			}

			var deliveryCost = onlineOrder.OnlineOrderItems
				.Where(item => IsDeliveryItem(item.NomenclatureId))
				.Sum(item => item.CurrentSum);

			var balanceTypeIds = new Dictionary<string, string>(StringComparer.Ordinal);

			if(!string.IsNullOrWhiteSpace(_mindboxSettings.BonusBalanceSystemName))
			{
				balanceTypeIds["systemName"] = _mindboxSettings.BonusBalanceSystemName;
			}

			var request = new MindboxLoyaltyOrderRequest
			{
				Customer = new MindboxLoyaltyOrderCustomer
				{
					Ids = new Dictionary<string, string>(StringComparer.Ordinal)
					{
						["externalid"] = message.ErpCounterpartyId.Value.ToString()
					}
				},
				Order = new MindboxLoyaltyOrder
				{
					Ids = new Dictionary<string, string>(StringComparer.Ordinal)
					{
						[_mindboxSettings.MindboxOrderIdName] = onlineOrder.Id.ToString()
					},
					DeliveryCost = deliveryCost,
					TotalPrice = onlineOrder.OnlineOrderSum,
					BonusPoints = new List<MindboxAuthorizedCartBonusPoint>
					{
						new()
						{
							Amount = message.BonusesToWriteOff ?? 0,
							BalanceType = new MindboxAuthorizedCartBalanceType
							{
								Ids = balanceTypeIds
							}
						}
					},
					Lines = lines
				}
			};

			return await _mindboxLoyaltyOrderService.CreateAsync(
				request,
				onlineOrder.Id,
				onlineOrder.ExternalOrderId,
				message.ErpCounterpartyId.Value,
				onlineOrder.MindboxTransactionId.Value,
				cancellationToken);
		}

		private bool CanApplyBonuses(IUnitOfWork uow, ICreatingOnlineOrder message)
		{
			if(message.Source != Vodovoz.Core.Domain.Clients.Source.MobileApp
				|| !IsBonusProgramEnabled(message.ErpCounterpartyId ?? 0))
			{
				return false;
			}

			var state = uow.Session.Get<Vodovoz.Core.Domain.Clients.Mindbox.MindboxCounterpartyState>(
				message.ErpCounterpartyId ?? 0);

			return state?.Status == Vodovoz.Core.Domain.Clients.Mindbox.MindboxCounterpartyOutboxStatus.Sent;
		}

		private bool IsBonusProgramEnabled(int counterpartyId)
		{
			var mode = _mindboxSettings.BonusProgramMode?.Trim();

			if(string.Equals(mode, "All", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			if(!string.Equals(mode, "Clients", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return (_mindboxSettings.BonusProgramClientIds ?? string.Empty)
				.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(value => int.TryParse(value, out var id) ? id : (int?)null)
				.Where(id => id.HasValue)
				.Select(id => id.Value)
				.Contains(counterpartyId);
		}

		private bool IsDeliveryItem(int? nomenclatureId)
		{
			if(nomenclatureId is null)
			{
				return false;
			}

			return nomenclatureId == _nomenclatureSettings.PaidDeliveryNomenclatureId
				|| nomenclatureId == _nomenclatureSettings.FastDeliveryNomenclatureId
				|| _nomenclatureSettings.PaidDeliveriesNomenclaturesIds?.Contains(nomenclatureId.Value) == true;
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
