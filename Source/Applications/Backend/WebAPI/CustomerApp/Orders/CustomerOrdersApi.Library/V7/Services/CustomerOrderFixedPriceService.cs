using System;
using CustomerApp.Contracts.Common;
using CustomerOrders.Abstractions;
using CustomerOrders.Abstractions.V7.Sale;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.V7.Dto.Orders.FixedPrice;
using CustomerOrdersApi.Library.V7.Factories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Interfaces.Sale;
using VodovozInfrastructure.Cryptography;

namespace CustomerOrdersApi.Library.V7.Services
{
	internal class CustomerOrderFixedPriceService : SignatureService, ICustomerOrderFixedPriceService
	{
		private readonly ILogger<CustomerOrdersService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISignatureManager _signatureManager;
		private readonly IOnlineOrderFixedPriceHandler _onlineOrderFixedPriceHandler;
		private readonly IInfoMessageFactory _infoMessageFactory;
		private readonly SignatureOptions _signatureOptions;

		public CustomerOrderFixedPriceService(
			ILogger<CustomerOrdersService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ISignatureManager signatureManager,
			IOptions<SignatureOptions> signatureOptions,
			IOnlineOrderFixedPriceHandler onlineOrderFixedPriceHandler,
			IInfoMessageFactory infoMessageFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_onlineOrderFixedPriceHandler =
				onlineOrderFixedPriceHandler ?? throw new ArgumentNullException(nameof(onlineOrderFixedPriceHandler));
			_infoMessageFactory = infoMessageFactory ?? throw new ArgumentNullException(nameof(infoMessageFactory));
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
		
		public ISaleItemPromotion ApplyFixedPriceToOnlineOrder(ApplyFixedPriceDto applyFixedPriceDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot($"Применение фиксы к онлайн заказу {applyFixedPriceDto.ExternalOrderId}");

			var (fixedPriceAppliedToAllOrder, saleItems) = _onlineOrderFixedPriceHandler.TryApplyFixedPrice(uow, applyFixedPriceDto);

			return AppliedFixedPriceDto.Create(
				saleItems,
				fixedPriceAppliedToAllOrder.HasValue && !fixedPriceAppliedToAllOrder.Value
					? _infoMessageFactory.CreateFixedPriceAppliedToNotAllItemsWarning()
					: null);
		}
	}
}
