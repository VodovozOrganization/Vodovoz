using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.V6.Dto.Orders.FixedPrice;
using CustomerOrdersApi.Library.V6.Dto.Orders.OrderItem;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Abstractions;
using Vodovoz.Core.Domain.Results;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.V6.Services
{
	public class CustomerOrderFixedPriceServiceV6 : SignatureService, ICustomerOrderFixedPriceServiceV6
	{
		private readonly ILogger<CustomerOrdersServiceV6> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISignatureManager _signatureManager;
		private readonly IOnlineOrderFixedPriceHandler _onlineOrderFixedPriceHandler;
		private readonly SignatureOptions _signatureOptions;

		public CustomerOrderFixedPriceServiceV6(
			ILogger<CustomerOrdersServiceV6> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISignatureManager signatureManager,
			IOptions<SignatureOptions> signatureOptions,
			IOnlineOrderFixedPriceHandler onlineOrderFixedPriceHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_onlineOrderFixedPriceHandler =
				onlineOrderFixedPriceHandler ?? throw new ArgumentNullException(nameof(onlineOrderFixedPriceHandler));
			_signatureOptions = (signatureOptions ?? throw new ArgumentNullException(nameof(signatureOptions))).Value;
		}
		
		public bool ValidateApplyingFixedPriceSignature(ApplyFixedPriceDto applyFixedPriceDto, out string generatedSignature)
		{
			var sourceSign = GetSourceSign(applyFixedPriceDto.Source, _signatureOptions);
			
			return _signatureManager.Validate(
				applyFixedPriceDto.Signature,
				new ApplyFixedPriceSignatureParams
				{
					OrderId = applyFixedPriceDto.Source == ExternalSource.MobileApp
						? applyFixedPriceDto.ExternalCounterpartyId.ToString()
						: applyFixedPriceDto.ExternalOrderId.ToString(),
					OrderSumInKopecks = (int)(GetOnlineOrderSum(applyFixedPriceDto.OnlineOrderItems) * 100),
					ShopId = (int)applyFixedPriceDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}
		
		public Result<IEnumerable<OnlineOrderItemWithFixedPriceDto>> ApplyFixedPriceToOnlineOrder(ApplyFixedPriceDto applyFixedPriceDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot($"Применение фиксы к онлайн заказу {applyFixedPriceDto.ExternalOrderId}");
			return _onlineOrderFixedPriceHandler.TryApplyFixedPrice(uow, applyFixedPriceDto);
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
