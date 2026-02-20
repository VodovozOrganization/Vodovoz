using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.V4.Dto.Orders.FixedPrice;
using CustomerOrdersApi.Library.V4.Dto.Orders.OrderItem;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Handlers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Nodes;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.V4.Services
{
	public class CustomerOrderFixedPriceService : SignatureService, ICustomerOrderFixedPriceService
	{
		private readonly ILogger<CustomerOrdersService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISignatureManager _signatureManager;
		private readonly IOnlineOrderFixedPriceHandler _onlineOrderFixedPriceHandler;
		private readonly SignatureOptions _signatureOptions;

		public CustomerOrderFixedPriceService(
			ILogger<CustomerOrdersService> logger,
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
					OrderId = applyFixedPriceDto.Source == Source.MobileApp
						? applyFixedPriceDto.ExternalCounterpartyId.ToString()
						: applyFixedPriceDto.ExternalOrderId.ToString(),
					OrderSumInKopecks = (int)(GetOnlineOrderSum(applyFixedPriceDto.OnlineOrderItems) * 100),
					ShopId = (int)applyFixedPriceDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}
		
		public Result<IEnumerable<IOnlineOrderedProductWithFixedPrice>> ApplyFixedPriceToOnlineOrder(ApplyFixedPriceDto applyFixedPriceDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot($"Применение фиксы к онлайн заказу {applyFixedPriceDto.ExternalOrderId}");

			var node = new CanApplyOnlineOrderFixedPrice
			{
				IsSelfDelivery =	applyFixedPriceDto.IsSelfDelivery,
				DeliveryPointId = applyFixedPriceDto.ErpDeliveryPointId,
				CounterpartyId = applyFixedPriceDto.ErpCounterpartyId,
				OnlineOrderItems = applyFixedPriceDto.OnlineOrderItems
			};
			
			return _onlineOrderFixedPriceHandler.TryApplyFixedPrice(uow, node);
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
