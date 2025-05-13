using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors.Orders;
using VodovozBusiness.Services.TrueMark;

namespace WarehouseApi.Library.Services
{
	internal sealed class SelfDeliveryService : ISelfDeliveryService
	{
		private ILogger<SelfDeliveryService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly IGenericRepository<SelfDeliveryDocument> _selfDeliveryDocumentRepository;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;

		public SelfDeliveryService(
			ILogger<SelfDeliveryService> logger,
			IUnitOfWork unitOfWork,
			IGenericRepository<Order> orderRepository,
			IGenericRepository<SelfDeliveryDocument> selfDeliveryDocumentRepository,
			ITrueMarkWaterCodeService trueMarkWaterCodeService)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
			_orderRepository = orderRepository
				?? throw new ArgumentNullException(nameof(orderRepository));
			_selfDeliveryDocumentRepository = selfDeliveryDocumentRepository
				?? throw new ArgumentNullException(nameof(selfDeliveryDocumentRepository));
			_trueMarkWaterCodeService = trueMarkWaterCodeService
				?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
		}

		public async Task<Result<SelfDeliveryDocument>> GetSelfDeliveryDocumentByOrderId(int? orderId)
		{
			try
			{
				if(_selfDeliveryDocumentRepository.Get(_unitOfWork, x => x.Order.Id == orderId).FirstOrDefault() is SelfDeliveryDocument selfDeliveryDocument)
				{
					return await Task.FromResult(selfDeliveryDocument);
				}

				return VodovozBusiness.Errors.Warehouse.SelfDeliveryDocument.NotFound;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка получения документа самовывоза по id заказа {orderId}");
				return Result.Failure<SelfDeliveryDocument>(Vodovoz.Errors.Common.Repository.DataRetrievalError);
			}
		}

		public Task<Result<SelfDeliveryDocument>> GetSelfDeliveryDocumentById(int? selfDeliveryDocumentId)
		{
			throw new NotImplementedException();
		}

		public async Task<Result<SelfDeliveryDocument>> CreateDocument(int orderId, int warehouseId)
		{
			return await Task.FromResult(new SelfDeliveryDocument
			{
				Order = new Order { Id = orderId },

			});
		}

		public Task<Result<SelfDeliveryDocument>> AddCodes(SelfDeliveryDocument selfDeliveryDocument, IEnumerable<string> codesToAdd)
		{
			_trueMarkWaterCodeService.GetTrueMarkAnyCodesByScannedCodes(_unitOfWork, codesToAdd)
				.BindAsync(trueMarkAnyCodes =>
				{
					var identificationCodes = trueMarkAnyCodes
						.Where(x => x.IsTrueMarkWaterIdentificationCode)
						.Select(x => x.TrueMarkWaterIdentificationCode)
						.ToArray();

					var nomenclatureGroupedItems = selfDeliveryDocument.Items
						.Where(x => x.Nomenclature.IsAccountableInTrueMark)
						.GroupBy(x => x.Nomenclature);

					foreach(var code in identificationCodes)
					{
						nomenclatureGroupedItems
							.FirstOrDefault(x => x.Key.Gtins.Any(gtin => gtin.GtinNumber == code.GTIN))
							.Where(sddi => sddi.TrueMarkProductCodes);
					}

					return Task.FromResult(selfDeliveryDocument);
				});
		}

		public Task<Result<SelfDeliveryDocument>> ChangeCodes(SelfDeliveryDocument selfDeliveryDocument, IDictionary<string, string> codesToChange)
		{
			throw new NotImplementedException();
		}

		public Task<Result<SelfDeliveryDocument>> RemoveCodes(SelfDeliveryDocument selfDeliveryDocument, IEnumerable<string> codesToDelete)
		{
			throw new NotImplementedException();
		}

		public Task<Result<SelfDeliveryDocument>> EndLoad(SelfDeliveryDocument selfDeliveryDocument)
		{
			throw new NotImplementedException();
		}

		private Result ValidateSelfDeliveryOrderToProcess(Order order)
		{
			if(order is null)
			{
				_logger.LogWarning($"Заказ не найден.");
				return Vodovoz.Errors.Orders.Order.NotFound;
			}

			if(!order.SelfDelivery)
			{
				_logger.LogWarning($"Заказ с id {order.Id} не является самовывозом.");
				return Vodovoz.Errors.Orders.Order.IsNotSelfDelivery;
			}

			if(order.OrderStatus != OrderStatus.Accepted)
			{
				_logger.LogWarning($"Заказ с id {order.Id} не может быть обработан, так как его статус {order.OrderStatus}");
				return Vodovoz.Errors.Orders.Order.CantEdit;
			}

			return Result.Success();
		}

	}
}
