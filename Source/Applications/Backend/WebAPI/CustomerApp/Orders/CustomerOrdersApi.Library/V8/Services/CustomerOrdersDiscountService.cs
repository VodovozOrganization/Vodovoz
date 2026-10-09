using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerApp.Contracts.Common;
using CustomerOrders.Abstractions;
using CustomerOrders.Abstractions.V8.Sale;
using CustomerOrders.Contracts.V8.Sale;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.V8.Dto.Orders;
using CustomerOrdersApi.Library.V8.Dto.Orders.Promotions.Discounts;
using CustomerOrdersApi.Library.V8.Extensions;
using CustomerOrdersApi.Library.V8.Factories;
using CustomerOrdersApi.Library.V8.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Settings.Orders;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.V8.Services
{
	internal class CustomerOrdersDiscountService : SignatureService, ICustomerOrdersDiscountService
	{
		private readonly ILogger<CustomerOrdersService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISignatureManager _signatureManager;
		private readonly IOnlineOrderDiscountHandler _onlineOrderDiscountHandler;
		private readonly IInfoMessageFactory _infoMessageFactory;
		private readonly ICustomerOrderRepository _customerOrderRepository;
		private readonly IDiscountReasonSettings _discountReasonSettings;
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private readonly SignatureOptions _signatureOptions;

		public CustomerOrdersDiscountService(
			ILogger<CustomerOrdersService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISignatureManager signatureManager,
			IOptions<SignatureOptions> signatureOptions,
			IOnlineOrderDiscountHandler onlineOrderDiscountHandler,
			IInfoMessageFactory infoMessageFactory,
			ICustomerOrderRepository customerOrderRepository,
			IDiscountReasonSettings discountReasonSettings,
			IDiscountReasonRepository discountReasonRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_onlineOrderDiscountHandler = onlineOrderDiscountHandler ?? throw new ArgumentNullException(nameof(onlineOrderDiscountHandler));
			_infoMessageFactory = infoMessageFactory ?? throw new ArgumentNullException(nameof(infoMessageFactory));
			_customerOrderRepository = customerOrderRepository ?? throw new ArgumentNullException(nameof(customerOrderRepository));
			_discountReasonSettings = discountReasonSettings ?? throw new ArgumentNullException(nameof(discountReasonSettings));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
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

		public ISalePromotion ApplyPromoCodeToOnlineOrder(ApplyPromoCodeDto applyPromoCodeDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Применение промокода к онлайн заказу");
			
			var result = _onlineOrderDiscountHandler.TryApplyPromoCode(uow, applyPromoCodeDto);

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
			ExternalSource source,
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
					source.ToSource(),
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

			var result = _onlineOrderDiscountHandler.TryApplyFirstOrderDiscount(uow, applyFirstOrderDiscountDto);
			
			return AppliedFirstOrderDiscountDto.Create(result);
		}

		public ISalePromotion ProcessAutoOrderDiscount(
			ApplyAutoOrderDiscountRequest applyAutoOrderDiscount,
			CancellationToken cancellationToken)
		{
			var action = applyAutoOrderDiscount.Apply ? "Применение" : "Снятие";
			using var uow = _unitOfWorkFactory.CreateWithoutRoot($"{action} скидки за автозаказ к онлайн заказу");
			
			var result = _onlineOrderDiscountHandler.ProcessAutoOrderDiscount(uow, applyAutoOrderDiscount);

			if(result.IsFailure)
			{
				return ApplyAutoOrderDiscountResponse.CreateError(result.Errors.First().Message);
			}
			
			return ApplyAutoOrderDiscountResponse.Create(result.Value);
		}

		public DiscountDto GetAutoOrderDiscount()
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot($"Получение данных скидки за автозаказ");
			var discount = _discountReasonRepository.GetDiscountReason(uow, _discountReasonSettings.AutoOrderDiscountReasonId);

			if(discount is null)
			{
				throw new InvalidOperationException(
					"Не найдена скидка за автозаказ. Проверьте корректность идентификатора в параметрах и саму скидку в БД");
			}

			return DiscountDto.Create(discount.ValueType == DiscountUnits.money, discount.Value);
		}
	}
}
