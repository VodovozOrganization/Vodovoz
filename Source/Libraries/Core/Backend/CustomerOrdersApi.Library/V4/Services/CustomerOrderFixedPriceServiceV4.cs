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
using VodovozBusiness.Domain.Orders.V4;
using VodovozBusiness.Nodes.V4;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.V4.Services
{
	public class CustomerOrderFixedPriceServiceV4 : SignatureService, ICustomerOrderFixedPriceServiceV4
	{
		private readonly ILogger<CustomerOrderFixedPriceServiceV4> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISignatureManager _signatureManager;
		private readonly IOnlineOrderFixedPriceHandlerV4 _onlineOrderFixedPriceHandler;
		private readonly SignatureOptions _signatureOptions;

		public CustomerOrderFixedPriceServiceV4(
			ILogger<CustomerOrderFixedPriceServiceV4> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISignatureManager signatureManager,
			IOptions<SignatureOptions> signatureOptions,
			IOnlineOrderFixedPriceHandlerV4 onlineOrderFixedPriceHandler)
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
		
		public Result<IEnumerable<IOnlineOrderedProductWithFixedPriceV4>> ApplyFixedPriceToOnlineOrder(ApplyFixedPriceDto applyFixedPriceDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot($"Применение фиксы к онлайн заказу {applyFixedPriceDto.ExternalOrderId}");

			var node = new CanApplyOnlineOrderFixedPriceV4
			{
				IsSelfDelivery =	applyFixedPriceDto.IsSelfDelivery,
				DeliveryPointId = applyFixedPriceDto.ErpDeliveryPointId,
				CounterpartyId = applyFixedPriceDto.ErpCounterpartyId,
				OnlineOrderItems = applyFixedPriceDto.OnlineOrderItems
			};
			
			return _onlineOrderFixedPriceHandler.TryApplyFixedPrice(uow, node);
		}
		
		private decimal GetOnlineOrderSum(IEnumerable<OnlineOrderItemDtoV4> orderItems)
		{
			return orderItems.Sum(x =>
				x.IsDiscountInMoney
					? x.Count * x.Price - x.Discount
					: x.Count * x.Price * (1 - x.Discount / 100));
		}
	}
}
