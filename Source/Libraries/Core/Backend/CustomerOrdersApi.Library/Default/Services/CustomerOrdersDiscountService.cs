using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.Default.Dto.Orders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders.V4;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Handlers;
using VodovozBusiness.Nodes.V4;
using VodovozInfrastructure.Cryptography;
using OnlineOrderItemDto = CustomerOrdersApi.Library.Default.Dto.Orders.OrderItem.OnlineOrderItemDto;

namespace CustomerOrdersApi.Library.Default.Services
{
	public class CustomerOrdersDiscountService : SignatureService, ICustomerOrdersDiscountService
	{
		private readonly ILogger<CustomerOrdersDiscountService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISignatureManager _signatureManager;
		private readonly IOnlineOrderDiscountHandlerV4 _onlineOrderDiscountHandler;
		private readonly SignatureOptions _signatureOptions;

		public CustomerOrdersDiscountService(
			ILogger<CustomerOrdersDiscountService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISignatureManager signatureManager,
			IOptions<SignatureOptions> signatureOptions,
			IOnlineOrderDiscountHandlerV4 onlineOrderDiscountHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_onlineOrderDiscountHandler = onlineOrderDiscountHandler ?? throw new ArgumentNullException(nameof(onlineOrderDiscountHandler));
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
					OrderSumInKopecks = (int)(GetOnlineOrderSum(applyPromoCodeDto.OnlineOrderItems) * 100),
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

		public Result<IEnumerable<IOnlineOrderedProductV4>> ApplyPromoCodeToOnlineOrder(ApplyPromoCodeDto applyPromoCodeDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Применение промокода к онлайн заказу");

			var dto = new CanApplyOnlineOrderPromoCodeV4
			{
				PromoCode =	applyPromoCodeDto.PromoCode,
				Time = applyPromoCodeDto.RequestTime.ToLocalTime(),
				CounterpartyId = applyPromoCodeDto.ErpCounterpartyId.Value,
				Products = applyPromoCodeDto.OnlineOrderItems
			};
			
			return _onlineOrderDiscountHandler.TryApplyPromoCode(uow, dto);
		}

		private decimal GetOnlineOrderSum(IEnumerable<OnlineOrderItemDto> orderItems)
		{
			return orderItems.Sum(x =>
				x.IsDiscountInMoney
					? x.Count * x.Price - x.Discount
					: x.Count * x.Price * (1 - x.Discount / 100));
		}
	}
}
