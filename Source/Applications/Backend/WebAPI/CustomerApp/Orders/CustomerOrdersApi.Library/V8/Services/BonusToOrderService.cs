using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V7.Dto.Orders.OrderItem;
using CustomerOrdersApi.Library.V8.Dto.Sale;
using Mindbox.Library.Contracts;
using Mindbox.Library.Mapping;
using Mindbox.Library.Services;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Mindbox;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Handlers;
using Vodovoz.Settings.Database.Mindbox;
using Vodovoz.Settings.Nomenclature;

namespace CustomerOrdersApi.Library.V8.Services
{
	/// <inheritdoc />
	internal class BonusToOrderService : IBonusToOrderService
	{
		private const string OrderProcessingStatus = "OrderProcessing";
		private const string BonusProgramModeAll = "All";
		private const string BonusProgramModeClients = "Clients";

		private readonly ILogger<BonusToOrderService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IMindboxSettings _mindboxSettings;
		private readonly IMindboxAuthorizedCartService _authorizedCartService;
		private readonly IMindboxCounterpartyEnqueuer _counterpartyEnqueuer;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IOnlineOrderDiscountHandler _onlineOrderDiscountHandler;

		public BonusToOrderService(
			ILogger<BonusToOrderService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IMindboxSettings mindboxSettings,
			IMindboxAuthorizedCartService authorizedCartService,
			IMindboxCounterpartyEnqueuer counterpartyEnqueuer,
			INomenclatureSettings nomenclatureSettings,
			IOnlineOrderDiscountHandler onlineOrderDiscountHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_mindboxSettings = mindboxSettings ?? throw new ArgumentNullException(nameof(mindboxSettings));
			_authorizedCartService = authorizedCartService ?? throw new ArgumentNullException(nameof(authorizedCartService));
			_counterpartyEnqueuer = counterpartyEnqueuer ?? throw new ArgumentNullException(nameof(counterpartyEnqueuer));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_onlineOrderDiscountHandler = onlineOrderDiscountHandler ?? throw new ArgumentNullException(nameof(onlineOrderDiscountHandler));
		}

		/// <inheritdoc />
		public async Task<ApplyBonusToOrderResponse> CalculateAsync(
			ApplyBonusToOrderRequest request,
			CancellationToken cancellationToken)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var requestItems = request.OnlineOrderItems?
				.Where(item => item != null)
				.ToList() ?? new List<OnlineOrderItemDto>();

			ValidateRequest(request, requestItems);

			var erpCounterpartyId = request.ErpCounterpartyId.Value;
			var bonusesToWriteOff = (int)request.BonusesToWriteOff.Value;

			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Расчёт бонусов при оформлении заказа");

			if(request.Source != Source.MobileApp || !IsBonusProgramEnabled(erpCounterpartyId))
			{
				_logger.LogInformation(
					"ApplyBonusToOrder: Mindbox не вызывается, клиент не участник программы. erpCounterpartyId={ErpCounterpartyId}, externalOrderId={ExternalOrderId}, source={Source}",
					erpCounterpartyId,
					request.ExternalOrderId,
					request.Source);

				return BuildResponse(
					requestItems,
					CreateCalculatedItems(uow, requestItems),
					null,
					BonusUnavailableReason.NotParticipant);
			}

			var state = uow.Session.Get<MindboxCounterpartyState>(erpCounterpartyId);

			if(state is null || state.Status != MindboxCounterpartyOutboxStatus.Sent)
			{
				if(state is null || state.Status == MindboxCounterpartyOutboxStatus.Unknown)
				{
					await _counterpartyEnqueuer.EnqueueCounterpartyAsync(erpCounterpartyId, cancellationToken);
				}

				_logger.LogInformation(
					"ApplyBonusToOrder: контрагент не передан в Mindbox. erpCounterpartyId={ErpCounterpartyId}, externalOrderId={ExternalOrderId}, status={Status}",
					erpCounterpartyId,
					request.ExternalOrderId,
					state?.Status);

				return BuildResponse(
					requestItems,
					CreateCalculatedItems(uow, requestItems),
					null,
					BonusUnavailableReason.CustomerNotRegistered);
			}

			var calculatedItems = CreateCalculatedItems(uow, requestItems);
			var mindboxRequest = BuildMindboxRequest(requestItems, erpCounterpartyId, bonusesToWriteOff);
			var result = await _authorizedCartService.CalculateAsync(
				mindboxRequest,
				erpCounterpartyId,
				request.ExternalOrderId,
				cancellationToken);

			if(!result.IsSuccess)
			{
				_logger.LogWarning(
					"ApplyBonusToOrder: Mindbox недоступен. erpCounterpartyId={ErpCounterpartyId}, externalOrderId={ExternalOrderId}, requested={Requested}, error={Error}",
					erpCounterpartyId,
					request.ExternalOrderId,
					bonusesToWriteOff,
					result.ErrorMessage);

				return BuildResponse(
					requestItems,
					calculatedItems,
					null,
					BonusUnavailableReason.ServiceUnavailable);
			}

			return BuildResponse(
				requestItems,
				calculatedItems,
				result,
				null);
		}

		private void ValidateRequest(ApplyBonusToOrderRequest request, IReadOnlyList<OnlineOrderItemDto> requestItems)
		{
			if(request.BonusesToWriteOff is null
				|| request.BonusesToWriteOff < 0
				|| decimal.Truncate(request.BonusesToWriteOff.Value) != request.BonusesToWriteOff.Value)
			{
				throw new BonusToOrderValidationException("Некорректная сумма списания");
			}

			if(requestItems.Count == 0 || requestItems.All(IsDeliveryItem))
			{
				throw new BonusToOrderValidationException("Корзина пуста");
			}

			if(request.ErpCounterpartyId is null || request.ErpCounterpartyId <= 0)
			{
				throw new BonusToOrderValidationException("Не передан клиент");
			}
		}

		private bool IsBonusProgramEnabled(int erpCounterpartyId)
		{
			var mode = _mindboxSettings.BonusProgramMode?.Trim();

			if(string.Equals(mode, BonusProgramModeAll, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			if(!string.Equals(mode, BonusProgramModeClients, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return ParseClientIds(_mindboxSettings.BonusProgramClientIds).Contains(erpCounterpartyId);
		}

		private static HashSet<int> ParseClientIds(string clientIds)
		{
			var result = new HashSet<int>();

			if(string.IsNullOrWhiteSpace(clientIds))
			{
				return result;
			}

			foreach(var value in clientIds.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if(int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
				{
					result.Add(id);
				}
			}

			return result;
		}

		private IReadOnlyList<IOrderedCartItemWithDiscountDetails> CreateCalculatedItems(
			IUnitOfWork uow,
			IReadOnlyList<OnlineOrderItemDto> requestItems)
		{
			return _onlineOrderDiscountHandler
				.CalculateDiscounts(uow, requestItems)
				.ToList();
		}

		private MindboxAuthorizedCartRequest BuildMindboxRequest(
			IReadOnlyList<OnlineOrderItemDto> requestItems,
			int erpCounterpartyId,
			int bonusesToWriteOff)
		{
			var deliveryCost = requestItems
				.Where(IsDeliveryItem)
				.Sum(item => item.CurrentSum);

			var lines = new List<MindboxAuthorizedCartLine>();
			var lineNumber = 0;
			var itemIdName = _mindboxSettings.BonusItemIdName;

			foreach(var item in requestItems.Where(item => !IsDeliveryItem(item)))
			{
				lineNumber++;

				lines.Add(new MindboxAuthorizedCartLine
				{
					LineNumber = lineNumber,
					Product = new MindboxProduct
					{
						Ids = new Dictionary<string, string>(StringComparer.Ordinal)
						{
							[itemIdName] = MindboxItemIdBuilder.Build(item.ItemType, item.ErpId)
						}
					},
					Quantity = item.Count,
					QuantityType = "int",
					BasePricePerItem = item.Count == 0
						? 0m
						: Math.Round(item.CurrentSum / item.Count, 2),
					Status = new MindboxAuthorizedCartLineStatus
					{
						Ids = new MindboxAuthorizedCartLineStatusIds
						{
							ExternalId = OrderProcessingStatus
						}
					}
				});
			}

			var balanceTypeIds = new Dictionary<string, string>(StringComparer.Ordinal);

			if(!string.IsNullOrWhiteSpace(_mindboxSettings.BonusBalanceSystemName))
			{
				balanceTypeIds["systemName"] = _mindboxSettings.BonusBalanceSystemName;
			}

			return new MindboxAuthorizedCartRequest
			{
				Customer = new MindboxAuthorizedCartCustomer
				{
					Ids = new Dictionary<string, string>(StringComparer.Ordinal)
					{
						["externalid"] = erpCounterpartyId.ToString(CultureInfo.InvariantCulture)
					}
				},
				Order = new MindboxAuthorizedCartOrder
				{
					DeliveryCost = deliveryCost,
					BonusPoints = new List<MindboxAuthorizedCartBonusPoint>
					{
						new MindboxAuthorizedCartBonusPoint
						{
							Amount = bonusesToWriteOff,
							BalanceType = new MindboxAuthorizedCartBalanceType
							{
								Ids = balanceTypeIds
							}
						}
					},
					Lines = lines
				}
			};
		}

		private ApplyBonusToOrderResponse BuildResponse(
			IReadOnlyList<OnlineOrderItemDto> requestItems,
			IReadOnlyList<IOrderedCartItemWithDiscountDetails> calculatedItems,
			MindboxAuthorizedCartResult mindboxResult,
			BonusUnavailableReason? unavailableReason)
		{
			var saleItems = new List<BonusSaleItemDto>(requestItems.Count);
			var lineNumber = 0;

			for(var index = 0; index < requestItems.Count; index++)
			{
				var requestItem = requestItems[index];
				var calculatedItem = index < calculatedItems.Count ? calculatedItems[index] : null;
				var isDelivery = IsDeliveryItem(requestItem);
				var bonusesWrittenOff = 0;

				if(!isDelivery)
				{
					lineNumber++;

					if(mindboxResult != null
						&& mindboxResult.BonusesByLineNumber.TryGetValue(lineNumber, out var appliedBonuses))
					{
						bonusesWrittenOff = appliedBonuses;
					}
				}

				var currentSum = Math.Round(requestItem.CurrentSum - bonusesWrittenOff, 2);
				var currentPrice = requestItem.Count == 0
					? 0m
					: Math.Round(currentSum / requestItem.Count, 2);

				saleItems.Add(new BonusSaleItemDto
				{
					ErpId = requestItem.ErpId,
					ItemType = requestItem.ItemType,
					Price = requestItem.Price,
					CurrentPrice = currentPrice,
					PriceWithoutDiscount = requestItem.PriceWithoutDiscount,
					CurrentSum = currentSum,
					Count = requestItem.Count,
					IsFixedPrice = requestItem.IsFixedPrice,
					GiftItem = requestItem.GiftItem,
					Discounts = calculatedItem?.Discounts ?? new List<IDiscountAmount>(),
					BonusesWrittenOff = bonusesWrittenOff
				});
			}

			var orderSumBeforeBonuses = Math.Round(requestItems.Sum(item => item.CurrentSum), 2);
			var orderSum = Math.Round(saleItems.Sum(item => item.CurrentSum), 2);

			return new ApplyBonusToOrderResponse
			{
				BonusInfo = mindboxResult != null
					? new BonusInfoDto
					{
						IsAvailable = true,
						UnavailableReason = null,
						Balance = mindboxResult.Balance,
						MaxBonusesToWriteOff = mindboxResult.MaxBonusesToWriteOff,
						BonusesWrittenOff = mindboxResult.BonusesWrittenOff,
						BonusesToEarn = mindboxResult.BonusesToEarn
					}
					: BonusInfoDto.CreateUnavailable(unavailableReason.Value),
				OrderSumBeforeBonuses = orderSumBeforeBonuses,
				OrderSum = orderSum,
				SaleItems = saleItems
			};
		}

		private bool IsDeliveryItem(OnlineOrderItemDto item)
		{
			if(item is null)
			{
				return false;
			}

			if(item.ErpId == _nomenclatureSettings.PaidDeliveryNomenclatureId
				|| item.ErpId == _nomenclatureSettings.FastDeliveryNomenclatureId)
			{
				return true;
			}

			return _nomenclatureSettings.PaidDeliveriesNomenclaturesIds?.Contains(item.ErpId) == true;
		}
	}
}
