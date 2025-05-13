using FluentNHibernate.Data;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Stock;
using VodovozBusiness.Domain.Goods;
using VodovozBusiness.Services.TrueMark;

namespace WarehouseApi.Library.Services
{
	internal sealed class SelfDeliveryService : ISelfDeliveryService
	{
		private ILogger<SelfDeliveryService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly IGenericRepository<Warehouse> _warehouseRepository;
		private readonly IGenericRepository<SelfDeliveryDocument> _selfDeliveryDocumentRepository;
		private readonly IGenericRepository<GroupGtin> _groupGtinrepository;
		private readonly IGenericRepository<Gtin> _gtinRepository;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IStockRepository _stockRepository;
		private readonly IBottlesRepository _bottlesRepository;

		public SelfDeliveryService(
			ILogger<SelfDeliveryService> logger,
			IUnitOfWork unitOfWork,
			IGenericRepository<Order> orderRepository,
			IGenericRepository<Warehouse> warehouseRepository,
			IGenericRepository<SelfDeliveryDocument> selfDeliveryDocumentRepository,
			IGenericRepository<GroupGtin> groupGtinrepository,
			IGenericRepository<Gtin> gtinRepository,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			INomenclatureRepository nomenclatureRepository,
			IStockRepository stockRepository,
			IBottlesRepository bottlesRepository)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
			_orderRepository = orderRepository
				?? throw new ArgumentNullException(nameof(orderRepository));
			_warehouseRepository = warehouseRepository
				?? throw new ArgumentNullException(nameof(warehouseRepository));
			_selfDeliveryDocumentRepository = selfDeliveryDocumentRepository
				?? throw new ArgumentNullException(nameof(selfDeliveryDocumentRepository));
			_groupGtinrepository = groupGtinrepository
				?? throw new ArgumentNullException(nameof(groupGtinrepository));
			_gtinRepository = gtinRepository
				?? throw new ArgumentNullException(nameof(gtinRepository));
			_trueMarkWaterCodeService = trueMarkWaterCodeService
				?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_nomenclatureRepository = nomenclatureRepository
				?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_stockRepository = stockRepository
				?? throw new ArgumentNullException(nameof(stockRepository));
			_bottlesRepository = bottlesRepository
				?? throw new ArgumentNullException(nameof(bottlesRepository));
		}

		public async Task<Result<SelfDeliveryDocument>> GetSelfDeliveryDocumentByOrderId(int orderId, CancellationToken cancellationToken)
		{
			try
			{
				if(_selfDeliveryDocumentRepository
					.Get(_unitOfWork, x => x.Order.Id == orderId)
					.FirstOrDefault() is SelfDeliveryDocument selfDeliveryDocument)
				{
					return await Task.FromResult(selfDeliveryDocument);
				}

				return VodovozBusiness.Errors.Warehouses.Documents.SelfDeliveryDocument.NotFound;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка получения документа самовывоза по id заказа {orderId}");
				return Result.Failure<SelfDeliveryDocument>(Vodovoz.Errors.Common.Repository.DataRetrievalError);
			}
		}

		public async Task<Result<SelfDeliveryDocument>> GetSelfDeliveryDocumentById(int selfDeliveryDocumentId, CancellationToken cancellationToken)
		{
			try
			{
				if(_selfDeliveryDocumentRepository
					.Get(_unitOfWork, x => x.Id == selfDeliveryDocumentId)
					.FirstOrDefault() is SelfDeliveryDocument selfDeliveryDocument)
				{
					return await Task.FromResult(selfDeliveryDocument);
				}

				return VodovozBusiness.Errors.Warehouses.Documents.SelfDeliveryDocument.NotFound;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка получения документа самовывоза по id {selfDeliveryDocumentId}");
				return Result.Failure<SelfDeliveryDocument>(Vodovoz.Errors.Common.Repository.DataRetrievalError);
			}
		}

		public async Task<Result<SelfDeliveryDocument>> CreateDocument(Employee author, int orderId, int warehouseId, CancellationToken cancellationToken)
		{
			var order = _orderRepository
				.Get(_unitOfWork, x => x.Id == orderId, 1)
				.FirstOrDefault();

			if(order is null)
			{
				_logger.LogWarning($"Заказ с id {orderId} не найден.");
				return Vodovoz.Errors.Orders.Order.NotFound;
			}

			var warehouse = _warehouseRepository
				.Get(_unitOfWork, x => x.Id == warehouseId, 1)
				.FirstOrDefault();

			if(warehouse is null)
			{
				_logger.LogWarning($"Склад с id {warehouseId} не найден.");
				return VodovozBusiness.Errors.Warehouses.Warehouse.NotFound;
			}

			var selfDeliveryDocument = new SelfDeliveryDocument
			{
				Author = author,
				Order = order,
				Warehouse = warehouse,
			};

			selfDeliveryDocument.InitializeDefaultValues(_unitOfWork, _nomenclatureRepository);

			selfDeliveryDocument.FillByOrder();
			selfDeliveryDocument.UpdateStockAmount(_unitOfWork, _stockRepository);
			selfDeliveryDocument.UpdateAlreadyUnloaded(_unitOfWork, _nomenclatureRepository, _bottlesRepository);

			UpdateAmounts(selfDeliveryDocument);

			return await Task.FromResult(selfDeliveryDocument);
		}

		public Task<Result<SelfDeliveryDocument>> AddCodes(SelfDeliveryDocument selfDeliveryDocument, IEnumerable<string> codesToAdd, CancellationToken cancellationToken)
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

		public Task<Result<SelfDeliveryDocument>> ChangeCodes(SelfDeliveryDocument selfDeliveryDocument, IDictionary<string, string> codesToChange, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<Result<SelfDeliveryDocument>> RemoveCodes(SelfDeliveryDocument selfDeliveryDocument, IEnumerable<string> codesToDelete, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<Result<SelfDeliveryDocument>> EndLoad(SelfDeliveryDocument selfDeliveryDocument, CancellationToken cancellationToken)
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

		private async Task AddCodeToSelfDeliveryDocumentItemAsync(
			SelfDeliveryDocumentItem selfDeliveryDocumentItem,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			CancellationToken cancellationToken)
		{
			await _unitOfWork.SaveAsync(trueMarkWaterIdentificationCode, cancellationToken: cancellationToken);

			var productCode = new SelfDeliveryDocumentItemTrueMarkProductCode
			{
				CreationTime = DateTime.Now,
				SourceCode = trueMarkWaterIdentificationCode,
				ResultCode = trueMarkWaterIdentificationCode,
				Problem = ProductCodeProblem.None,
				SourceCodeStatus = SourceProductCodeStatus.Accepted,
				SelfDeliveryDocumentItem = selfDeliveryDocumentItem
			};

			selfDeliveryDocumentItem.TrueMarkProductCodes.Add(productCode);
		}

		private void UpdateAmounts(SelfDeliveryDocument selfDeliveryDocument)
		{
			foreach(var item in selfDeliveryDocument.Items)
			{
				item.Amount = Math.Min(
					selfDeliveryDocument.GetNomenclaturesCountInOrder(item.Nomenclature) - item.AmountUnloaded,
					item.AmountInStock);
			}
		}
	}
}
