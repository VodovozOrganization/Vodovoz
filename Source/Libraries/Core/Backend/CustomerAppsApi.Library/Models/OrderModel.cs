using System;
using CustomerAppsApi.Library.Dto.Counterparties;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using VodovozBusiness.Services.Orders;

namespace CustomerAppsApi.Library.Models
{
	public class OrderModel : IOrderModel
	{
		private readonly ILogger<OrderModel> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IFreeLoaderChecker _freeLoaderChecker;

		public OrderModel(
			ILogger<OrderModel> logger,
			IUnitOfWork unitOfWork,
			IFreeLoaderChecker freeLoaderChecker)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_freeLoaderChecker = freeLoaderChecker ?? throw new ArgumentNullException(nameof(freeLoaderChecker));
		}

		public bool CanCounterpartyOrderPromoSetForNewClients(FreeLoaderCheckingDto freeLoaderCheckingDto)
		{
			return _freeLoaderChecker.CanOrderPromoSetForNewClientsFromOnline(
				_unitOfWork,
				freeLoaderCheckingDto.IsSelfDelivery,
				freeLoaderCheckingDto.ErpCounterpartyId,
				freeLoaderCheckingDto.ErpDeliveryPointId)
				.IsSuccess;
		}
	}
}
