using CustomerOrders.Contracts;
using CustomerOrders.Contracts.V5.Orders.Discounts;
using CustomerOrders.Contracts.V5.Orders.OrderItem;
using CustomerOrders.Contracts.V5.Orders.PromoCodes;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.Default.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Handlers;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Extensions;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.V5.Services
{
	public class CustomerOrdersDiscountServiceV5 : SignatureService, ICustomerOrdersDiscountServiceV5
	{
		private readonly ILogger<CustomerOrdersDiscountServiceV5> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISignatureManager _signatureManager;
		private readonly IOnlineOrderDiscountHandlerV5 _onlineOrderDiscountHandler;
		private readonly ICustomerOnlineOrderRepository _customerOnlineOrderRepository;
		private readonly IOrderSettings _orderSettings;
		private readonly SignatureOptions _signatureOptions;

		public CustomerOrdersDiscountServiceV5(
			ILogger<CustomerOrdersDiscountServiceV5> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISignatureManager signatureManager,
			IOptions<SignatureOptions> signatureOptions,
			IOnlineOrderDiscountHandlerV5 onlineOrderDiscountHandler,
			ICustomerOnlineOrderRepository customerOnlineOrderRepository,
			IOrderSettings orderSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_onlineOrderDiscountHandler = onlineOrderDiscountHandler ?? throw new ArgumentNullException(nameof(onlineOrderDiscountHandler));
			_customerOnlineOrderRepository = customerOnlineOrderRepository ?? throw new ArgumentNullException(nameof(customerOnlineOrderRepository));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_signatureOptions =
				(signatureOptions ?? throw new ArgumentNullException(nameof(signatureOptions)))
				.Value;
		}
		
		public bool ValidateApplyingPromoCodeSignature(ApplyPromoCodeDto applyPromoCodeDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(applyPromoCodeDto.Source, _signatureOptions);
			
			return _signatureManager.Validate(
				applyPromoCodeDto.Signature,
				new ApplyPromoCodeSignatureParams
				{
					OrderId = applyPromoCodeDto.Source == ExternalSource.MobileApp
						? applyPromoCodeDto.ExternalCounterpartyId.ToString()
						: applyPromoCodeDto.ExternalOrderId.ToString(),
					OrderSumInKopecks = (int)(applyPromoCodeDto.OrderSum * 100),
					ShopId = (int)applyPromoCodeDto.Source,
					PromoCode = applyPromoCodeDto.PromoCode,
					Sign = sourceSign
				},
				out generatedSignature);
		}
		
		public bool ValidatePromoCodeWarningSignature(PromoCodeWarningDto promoCodeWarningDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(promoCodeWarningDto.Source, _signatureOptions);
			
			return _signatureManager.Validate(
				promoCodeWarningDto.Signature,
				new PromoCodeWarningSignatureParams
				{
					OrderId = promoCodeWarningDto.ExternalOrderId.ToString(),
					ShopId = (int)promoCodeWarningDto.Source,
					PromoCode = promoCodeWarningDto.PromoCode,
					Sign = sourceSign
				},
				out generatedSignature);
		}

		public Result<IEnumerable<OnlineOrderItemDto>> ApplyPromoCodeToOnlineOrder(ApplyPromoCodeDto applyPromoCodeDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Применение промокода к онлайн заказу");

			var dto = new CanApplyOnlineOrderPromoCodeV5
			{
				Source = applyPromoCodeDto.Source,
				PromoCode =	applyPromoCodeDto.PromoCode,
				Time = applyPromoCodeDto.RequestTime.ToLocalTime(),
				CounterpartyId = applyPromoCodeDto.ErpCounterpartyId.Value,
				Products = applyPromoCodeDto.OnlineOrderItems,
				OrderSum = applyPromoCodeDto.OrderSum
			};
			
			return _onlineOrderDiscountHandler.TryApplyPromoCode(uow, dto);
		}

		public async Task<FirstOrderDiscountConditionsDto> GetFirstOrderDiscountConditions(
			ExternalSource source,
			Guid externalCounterpartyId,
			int? counterpartyErpId,
			CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Проверка доступности использования скидки на первый заказ для клиента");

			if(counterpartyErpId is null)
			{
				return CreateFirstOrderDiscountConditionsDto(uow, false);
			}

			var isClientHasNotCancelledOnlineOrdersFromSource =
				await _customerOnlineOrderRepository.IsClientHasNotCancelledOnlineOrdersFromSource(
					uow,
					externalCounterpartyId,
					counterpartyErpId.Value,
					source.ToSource(),
					cancellationToken);

			return CreateFirstOrderDiscountConditionsDto(uow, !isClientHasNotCancelledOnlineOrdersFromSource);
		}

		private FirstOrderDiscountConditionsDto CreateFirstOrderDiscountConditionsDto(IUnitOfWork uow, bool isDiscountAvailable)
		{
			return new FirstOrderDiscountConditionsDto
			{
				DiscountIsAvailable = isDiscountAvailable,
				Discount = GetFirstOrderDiscountData(uow)
			};
		}

		private DiscountDto GetFirstOrderDiscountData(IUnitOfWork uow)
		{
			var discountReason =
				uow.GetById<DiscountReason>(_orderSettings.FirstOnlineOrderDiscountReasonId);

			return new DiscountDto
			{
				IsDiscountInMoney = discountReason.ValueType == DiscountUnits.money,
				Discount = discountReason.Value,
				DiscountReasonId = discountReason.Id
			};
		}
	}
}
