using System;
using System.Collections.Generic;
using CustomerOrders.Contracts;
using CustomerOrders.Contracts.V5.Orders.FixedPrice;
using CustomerOrders.Contracts.V5.Orders.FixedPrices;
using CustomerOrders.Contracts.V5.Orders.OrderItem;
using CustomerOrdersApi.Library.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Handlers;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.V5.Services
{
	public class CustomerOrderFixedPriceServiceV5 : SignatureService, ICustomerOrderFixedPriceServiceV5
	{
		private readonly ILogger<CustomerOrderFixedPriceServiceV5> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISignatureManager _signatureManager;
		private readonly IOnlineOrderFixedPriceHandlerV5 _onlineOrderFixedPriceHandler;
		private readonly SignatureOptions _signatureOptions;

		public CustomerOrderFixedPriceServiceV5(
			ILogger<CustomerOrderFixedPriceServiceV5> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISignatureManager signatureManager,
			IOptions<SignatureOptions> signatureOptions,
			IOnlineOrderFixedPriceHandlerV5 onlineOrderFixedPriceHandler)
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
					OrderSumInKopecks = (int)(applyFixedPriceDto.OrderSum * 100),
					ShopId = (int)applyFixedPriceDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}
		
		public Result<IEnumerable<OnlineOrderItemWithFixedPriceV5>> ApplyFixedPriceToOnlineOrder(ApplyFixedPriceDto applyFixedPriceDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot($"Применение фиксы к онлайн заказу {applyFixedPriceDto.ExternalOrderId}");

			var node = new CanApplyOnlineOrderFixedPriceV5
			{
				IsSelfDelivery =	applyFixedPriceDto.IsSelfDelivery,
				DeliveryPointId = applyFixedPriceDto.ErpDeliveryPointId,
				CounterpartyId = applyFixedPriceDto.ErpCounterpartyId,
				OnlineOrderItems = applyFixedPriceDto.OnlineOrderItems
			};
			
			return _onlineOrderFixedPriceHandler.TryApplyFixedPrice(uow, node);
		}
	}
}
