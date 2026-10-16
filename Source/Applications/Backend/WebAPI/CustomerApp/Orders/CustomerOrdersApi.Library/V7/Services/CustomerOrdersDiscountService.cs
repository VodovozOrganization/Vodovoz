using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.V7.Dto.Orders;
using CustomerOrdersApi.Library.V7.Dto.Orders.Promotions.Discounts;
using CustomerOrdersApi.Library.V7.Factories;
using CustomerOrdersApi.Library.V7.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Handlers;
using Vodovoz.Nodes;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Nodes;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.V7.Services
{
	public class CustomerOrdersDiscountService : SignatureService, ICustomerOrdersDiscountService
	{
		private readonly ILogger<CustomerOrdersService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISignatureManager _signatureManager;
		private readonly IOnlineOrderDiscountHandler _onlineOrderDiscountHandler;
		private readonly IInfoMessageFactory _infoMessageFactory;
		private readonly ICustomerOrderRepository _customerOrderRepository;
		private readonly IDiscountReasonSettings _discountReasonSettings;
		private readonly SignatureOptions _signatureOptions;

		public CustomerOrdersDiscountService(
			ILogger<CustomerOrdersService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISignatureManager signatureManager,
			IOptions<SignatureOptions> signatureOptions,
			IOnlineOrderDiscountHandler onlineOrderDiscountHandler,
			IInfoMessageFactory infoMessageFactory,
			ICustomerOrderRepository customerOrderRepository,
			IDiscountReasonSettings discountReasonSettings
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_onlineOrderDiscountHandler = onlineOrderDiscountHandler ?? throw new ArgumentNullException(nameof(onlineOrderDiscountHandler));
			_infoMessageFactory = infoMessageFactory ?? throw new ArgumentNullException(nameof(infoMessageFactory));
			_customerOrderRepository = customerOrderRepository ?? throw new ArgumentNullException(nameof(customerOrderRepository));
			_discountReasonSettings = discountReasonSettings ?? throw new ArgumentNullException(nameof(discountReasonSettings));
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
					OrderId = applyPromoCodeDto.Source == Source.MobileApp
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

		public ISaleItemPromotion ApplyPromoCodeToOnlineOrder(ApplyPromoCodeDto applyPromoCodeDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Применение промокода к онлайн заказу");

			var dto = new CanApplyOnlineOrderPromoCodeV7
			{
				Source = applyPromoCodeDto.Source,
				PromoCode =	applyPromoCodeDto.PromoCode,
				Time = applyPromoCodeDto.RequestTime.ToLocalTime(),
				CounterpartyId = applyPromoCodeDto.ErpCounterpartyId,
				Products = applyPromoCodeDto.OnlineOrderItems
			};
			
			var result = _onlineOrderDiscountHandler.TryApplyPromoCodeV7(uow, dto);

			if(result.IsFailure)
			{
				return AppliedPromoCodeDto.CreateError(result.Errors.First());
			}
			
			return AppliedPromoCodeDto.Create(
				result.Value.CartItems,
				result.Value.AppliedToAllItems
					? null
					: _infoMessageFactory.CreatePromoCodeAppliedToNotAllItemsWarning());
		}
		
		public async Task<FirstOrderDiscountConditionsDto> CanApplyFirstOrderDiscount(
			Source source,
			Guid? externalCounterpartyId,
			int? erpCounterpartyId,
			CancellationToken cancellationToken
			)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Проверка доступности использования скидки на первый заказ для клиента");

			if(erpCounterpartyId is null)
			{
				return FirstOrderDiscountConditionsDto.Create(false);
			}

			var isClientHasNotCancelledOnlineOrdersFromSource =
				await _customerOrderRepository.IsClientHasNotCancelledOnlineOrdersFromSource(
					uow,
					externalCounterpartyId,
					erpCounterpartyId.Value,
					source,
					cancellationToken);

			return FirstOrderDiscountConditionsDto.Create(!isClientHasNotCancelledOnlineOrdersFromSource);
		}

		public async Task<AppliedFirstOrderDiscountDto> ApplyFirstOrderDiscount(
			ApplyFirstOrderDiscountDto applyFirstOrderDiscountDto,
			CancellationToken cancellationToken)
		{
			var canApply = await CanApplyFirstOrderDiscount(
				applyFirstOrderDiscountDto.Source,
				applyFirstOrderDiscountDto.ExternalCounterpartyId,
				applyFirstOrderDiscountDto.ErpCounterpartyId,
				cancellationToken);

			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Применение скидки на первый заказ");
			
			if(!canApply.DiscountIsAvailable)
			{
				return AppliedFirstOrderDiscountDto.Create(
					_onlineOrderDiscountHandler.CalculateDiscounts(uow, applyFirstOrderDiscountDto.OnlineOrderItems));
			}

			var dto = CanApplyFirstOrderDiscountRequest.Create(
				applyFirstOrderDiscountDto.Source,
				applyFirstOrderDiscountDto.ErpCounterpartyId,
				applyFirstOrderDiscountDto.ExternalCounterpartyId,
				applyFirstOrderDiscountDto.OnlineOrderItems
			);
			
			var result = _onlineOrderDiscountHandler.TryApplyFirstOrderDiscount(uow, dto);
			
			return AppliedFirstOrderDiscountDto.Create(result);
		}
	}
}
