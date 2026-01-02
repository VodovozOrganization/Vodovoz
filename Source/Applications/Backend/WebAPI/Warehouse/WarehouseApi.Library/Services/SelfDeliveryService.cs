using MassTransit.Initializers;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Domain.Goods;
using VodovozBusiness.Services.TrueMark;

namespace WarehouseApi.Library.Services
{
	internal sealed class SelfDeliveryService : ISelfDeliveryService
	{
		private ILogger<SelfDeliveryService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGenericRepository<OrderEntity> _orderRepository;
		private readonly IGenericRepository<Warehouse> _warehouseRepository;
		private readonly IGenericRepository<SelfDeliveryDocumentEntity> _selfDeliveryDocumentRepository;
		private readonly IGenericRepository<SubdivisionEntity> _subdivisionRepository;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly ISelfDeliveryRepository _selfDeliveryRepository;
		private readonly ICashRepository _cashRepository;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IStockRepository _stockRepository;
		private readonly IBottlesRepository _bottlesRepository;

		public SelfDeliveryService(
			ILogger<SelfDeliveryService> logger,
			IUnitOfWork unitOfWork,
			IGenericRepository<OrderEntity> orderRepository,
			IGenericRepository<Warehouse> warehouseRepository,
			IGenericRepository<SelfDeliveryDocumentEntity> selfDeliveryDocumentRepository,
			IGenericRepository<GroupGtin> groupGtinrepository,
			IGenericRepository<Gtin> gtinRepository,
			IGenericRepository<SubdivisionEntity> subdivisionRepository,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureRepository nomenclatureRepository,
			IRouteListItemRepository routeListItemRepository,
			ISelfDeliveryRepository selfDeliveryRepository,
			ICashRepository cashRepository,
			ICallTaskWorker callTaskWorker,
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
			_subdivisionRepository = subdivisionRepository
				?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_trueMarkWaterCodeService = trueMarkWaterCodeService
				?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_nomenclatureSettings = nomenclatureSettings
				?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_nomenclatureRepository = nomenclatureRepository
				?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_routeListItemRepository = routeListItemRepository
				?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_selfDeliveryRepository = selfDeliveryRepository
				?? throw new ArgumentNullException(nameof(selfDeliveryRepository));
			_cashRepository = cashRepository
				?? throw new ArgumentNullException(nameof(cashRepository));
			_callTaskWorker = callTaskWorker
				?? throw new ArgumentNullException(nameof(callTaskWorker));
			_stockRepository = stockRepository
				?? throw new ArgumentNullException(nameof(stockRepository));
			_bottlesRepository = bottlesRepository
				?? throw new ArgumentNullException(nameof(bottlesRepository));
		}

		public async Task<Result<SelfDeliveryDocumentEntity>> GetSelfDeliveryDocumentByOrderId(int orderId, CancellationToken cancellationToken)
		{
			try
			{
				if(_selfDeliveryDocumentRepository
					.Get(_unitOfWork, x => x.Order.Id == orderId)
					.FirstOrDefault() is SelfDeliveryDocumentEntity selfDeliveryDocument)
				{
					return await Task.FromResult(selfDeliveryDocument);
				}

				return VodovozBusiness.Errors.Warehouses.Documents.SelfDeliveryDocument.NotFound;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка получения документа самовывоза по id заказа {orderId}");
				return Result.Failure<SelfDeliveryDocumentEntity>(Vodovoz.Errors.Common.RepositoryErrors.DataRetrievalError);
			}
		}

		public async Task<Result<SelfDeliveryDocumentEntity>> GetSelfDeliveryDocumentById(int selfDeliveryDocumentId, CancellationToken cancellationToken)
		{
			try
			{
				if(_selfDeliveryDocumentRepository
					.Get(_unitOfWork, x => x.Id == selfDeliveryDocumentId)
					.FirstOrDefault() is SelfDeliveryDocumentEntity selfDeliveryDocument)
				{
					return await Task.FromResult(selfDeliveryDocument);
				}

				return VodovozBusiness.Errors.Warehouses.Documents.SelfDeliveryDocument.NotFound;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка получения документа самовывоза по id {selfDeliveryDocumentId}");
				return Result.Failure<SelfDeliveryDocumentEntity>(Vodovoz.Errors.Common.RepositoryErrors.DataRetrievalError);
			}
		}

		public async Task<Result<SelfDeliveryDocumentEntity>> CreateDocument(Employee author, int orderId, int warehouseId, CancellationToken cancellationToken)
		{
			var order = _orderRepository
				.Get(_unitOfWork, x => x.Id == orderId, 1)
				.FirstOrDefault();

			if(order is null)
			{
				_logger.LogWarning($"Заказ с id {orderId} не найден.");
				return Vodovoz.Errors.Orders.OrderErrors.NotFound;
			}

			if(!order.SelfDelivery)
			{
				_logger.LogWarning($"Заказ с id {order.Id} не является самовывозом.");
				return Vodovoz.Errors.Orders.OrderErrors.IsNotSelfDelivery;
			}

			var warehouse = _warehouseRepository
				.Get(_unitOfWork, x => x.Id == warehouseId, 1)
				.FirstOrDefault();

			if(warehouse is null)
			{
				_logger.LogWarning($"Склад с id {warehouseId} не найден.");
				return VodovozBusiness.Errors.Warehouses.Warehouse.NotFound;
			}

			var selfDeliveryDocument = new SelfDeliveryDocumentEntity
			{
				AuthorId = author.Id,
				Order = order,
				Warehouse = warehouse,
			};

			var defaultBottleNomenclatureId =
				_nomenclatureRepository.GetDefaultBottleNomenclatureId(_unitOfWork, cancellationToken);

			//selfDeliveryDocument.FillByOrder();
			//selfDeliveryDocument.UpdateStockAmount(_unitOfWork, _stockRepository);
			//selfDeliveryDocument.UpdateAlreadyUnloaded(_unitOfWork, _nomenclatureRepository, _bottlesRepository);

			//UpdateAmounts(selfDeliveryDocument);

			return await Task.FromResult(selfDeliveryDocument);
		}

		public async Task<Result<SelfDeliveryDocumentEntity>> AddCodes(
			SelfDeliveryDocumentEntity selfDeliveryDocument,
			IEnumerable<string> codesToAdd,
			CancellationToken cancellationToken)
		{
			return await _trueMarkWaterCodeService
				.GetTrueMarkAnyCodesByScannedCodes(codesToAdd)
				.BindAsync(anyCodes => DistributeCodesAsync(anyCodes.Select(x => x.Value).ToList(), selfDeliveryDocument, cancellationToken));
		}

		private async Task<Result<SelfDeliveryDocumentEntity>> DistributeCodesAsync(
			IList<TrueMarkAnyCode> trueMarkAnyCodes,
			SelfDeliveryDocumentEntity selfDeliveryDocument,
			CancellationToken cancellationToken)
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

				SelfDeliveryDocumentItemEntity nextSelfDeliveryItemToDistributeByGtin;

				nextSelfDeliveryItemToDistributeByGtin = GetNextNotScannedDocumentItem(selfDeliveryDocument, code);

				if(nextSelfDeliveryItemToDistributeByGtin == null)
				{
					return Errors.TrueMarkErrors.TooMuchCodesGiven;
				}

				await AddCodeToSelfDeliveryDocumentItemAsync(nextSelfDeliveryItemToDistributeByGtin, code, cancellationToken);
			}

			return await Task.FromResult(selfDeliveryDocument);
		}

		public async Task<Result<SelfDeliveryDocumentEntity>> ChangeCodes(
			SelfDeliveryDocumentEntity selfDeliveryDocument,
			IDictionary<string, string> codesToChange,
			CancellationToken cancellationToken)
		{
			var allCodesResult = await _trueMarkWaterCodeService
				.GetTrueMarkAnyCodesByScannedCodes(
					codesToChange.Keys.Concat(codesToChange.Values),
					cancellationToken);

			if(allCodesResult.IsFailure)
			{
				return Result.Failure<SelfDeliveryDocumentEntity>(allCodesResult.Errors);
			}

			var allCodes = allCodesResult.Value.Select(x => x.Value);

			var keys = allCodes
				.Where(x => x.IsTrueMarkWaterIdentificationCode)
				.Select(x => x.TrueMarkWaterIdentificationCode)
				.Where(x => codesToChange.ContainsKey(x.RawCode));

			var values = allCodes
				.Where(x => x.IsTrueMarkWaterIdentificationCode)
				.Select(x => x.TrueMarkWaterIdentificationCode)
				.Where(x => codesToChange.Values.Contains(x.RawCode));

			var pairsToChange = codesToChange.ToDictionary(
				kv => keys.FirstOrDefault(x => x.RawCode == kv.Key),
				kv => values.FirstOrDefault(x => x.RawCode == kv.Value));

			foreach(var pair in pairsToChange)
			{
				var documentItem = selfDeliveryDocument.Items
					.FirstOrDefault(di => di.TrueMarkProductCodes
						.Any(pc => pc.SourceCode.Id == pair.Key.Id));

				if(documentItem != null)
				{
					var productCode = documentItem.TrueMarkProductCodes
						.FirstOrDefault(x => x.SourceCode.Id == pair.Key.Id);

					productCode.SourceCode = pair.Value;
					productCode.ResultCode = pair.Value;
				}
			}

			return selfDeliveryDocument;
		}

		public async Task<Result<SelfDeliveryDocumentEntity>> RemoveCodes(SelfDeliveryDocumentEntity selfDeliveryDocument, IEnumerable<string> codesToDelete, CancellationToken cancellationToken)
		{
			return await _trueMarkWaterCodeService
				.GetTrueMarkAnyCodesByScannedCodes(codesToDelete)
				.BindAsync(anyCodes => RemoveDistributedCodes(anyCodes.Select(x => x.Value).ToList(), selfDeliveryDocument, cancellationToken));
		}


		public async Task<Result<SelfDeliveryDocumentEntity>> EndLoad(SelfDeliveryDocumentEntity selfDeliveryDocument, CancellationToken cancellationToken)
		{
			//selfDeliveryDocument.FullyShiped(
			//	_unitOfWork,
			//	_nomenclatureSettings,
			//	_routeListItemRepository,
			//	_selfDeliveryRepository,
			//	_cashRepository,
			//	_callTaskWorker);

			return await Task.FromResult(selfDeliveryDocument);
		}

		public async Task<Result<IEnumerable<OrderEntity>>> GetSelfDeliveryOrders(int warehouseId, CancellationToken cancellationToken)
		{
			var warehouse = _warehouseRepository
				.Get(_unitOfWork, w => w.Id == warehouseId, 1)
				.FirstOrDefault();

			if(warehouse is null)
			{
				return VodovozBusiness.Errors.Warehouses.Warehouse.NotFound;
			}

			SubdivisionEntity warehouseOwnerSubdivision = null;

			if(warehouse.OwningSubdivisionId.HasValue)
			{
				warehouseOwnerSubdivision = _subdivisionRepository
					.GetFirstOrDefault(_unitOfWork, s => s.Id == warehouse.OwningSubdivisionId);
			}

			var warehouseGeoGroupId = warehouseOwnerSubdivision?.GeographicGroup?.Id;

			var orders = _orderRepository
				.Get(
					_unitOfWork,
					o =>
						o.SelfDelivery
						&& (warehouseGeoGroupId == null || o.SelfDeliveryGeoGroup.Id == warehouseGeoGroupId)
						&& o.OrderStatus == OrderStatus.OnLoading)
				.ToList();

			return orders;
		}

		private async Task AddCodeToSelfDeliveryDocumentItemAsync(
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem,
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
					selfDeliveryDocument.GetNomenclaturesCountInOrder(item.Nomenclature.Id) - item.AmountUnloaded,
					item.AmountInStock);
			}
		}

		private Result<SelfDeliveryDocumentEntity> RemoveDistributedCodes(
			IList<TrueMarkAnyCode> anyCodes,
			SelfDeliveryDocumentEntity selfDeliveryDocument,
			CancellationToken cancellationToken)
		{
			var codesToRemove = anyCodes
				.Where(x => x.IsTrueMarkWaterIdentificationCode)
				.Select(x => x.TrueMarkWaterIdentificationCode)
				.ToArray();

			foreach(var unitCode in codesToRemove)
			{
				var documentItem = selfDeliveryDocument.Items
					.FirstOrDefault(di => di.TrueMarkProductCodes
						.Any(pc => pc.SourceCode.RawCode == unitCode.RawCode));

				var toRemoveProducCode = documentItem?.TrueMarkProductCodes?
					.FirstOrDefault(x => x.SourceCode.RawCode == unitCode.RawCode);

				documentItem?.TrueMarkProductCodes?.Remove(toRemoveProducCode);
			}

			return selfDeliveryDocument;
		}

		private SelfDeliveryDocumentItemEntity GetNextNotScannedDocumentItem(SelfDeliveryDocumentEntity selfDeliveryDocument, TrueMarkWaterIdentificationCode code)
		{
			var documentItem = selfDeliveryDocument.Items?
				.Where(x => x.Nomenclature.IsAccountableInTrueMark)
				.FirstOrDefault(s =>
					s.Nomenclature.Gtins.Select(g => g.GtinNumber).Contains(code.Gtin)
					&& s.TrueMarkProductCodes.Count < s.Amount
					&& s.TrueMarkProductCodes.All(c =>
						!c.SourceCode.RawCode.Contains(code.RawCode)
						&& !code.RawCode.Contains(c.SourceCode.RawCode)));

			return documentItem;
		}
	}
}
