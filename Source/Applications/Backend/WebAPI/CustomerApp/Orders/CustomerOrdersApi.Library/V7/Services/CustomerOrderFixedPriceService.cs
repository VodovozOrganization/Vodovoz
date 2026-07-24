using System;
using System.Collections.Generic;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.V7.Dto.Orders.FixedPrice;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Handlers;
using VodovozBusiness.Nodes;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.V7.Services
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
					OrderSumInKopecks = (int)(applyFixedPriceDto.OrderSum * 100),
					ShopId = (int)applyFixedPriceDto.Source,
					Sign = sourceSign
				},
				out generatedSignature);
		}
		
		public Result<IEnumerable<IOrderedCartItem>> ApplyFixedPriceToOnlineOrder(ApplyFixedPriceDto applyFixedPriceDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot($"Применение фиксы к онлайн заказу {applyFixedPriceDto.ExternalOrderId}");

			var node = new CanApplyOnlineOrderFixedPriceV7
			{
				IsSelfDelivery =	applyFixedPriceDto.IsSelfDelivery,
				DeliveryPointId = applyFixedPriceDto.ErpDeliveryPointId,
				CounterpartyId = applyFixedPriceDto.ErpCounterpartyId,
				OnlineOrderItems = applyFixedPriceDto.OnlineOrderItems
			};
			
			return _onlineOrderFixedPriceHandler.TryApplyFixedPriceV7(uow, node);
		}
	}
}
