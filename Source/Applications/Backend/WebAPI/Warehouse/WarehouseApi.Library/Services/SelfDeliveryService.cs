using MassTransit.Initializers;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
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
using Vodovoz.Errors.TrueMark;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Client.Specifications;
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
		private readonly IGenericRepository<SubdivisionEntity> _subdivisionRepository;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly ISelfDeliveryDocumentItemTrueMarkProductCodesProcessingService _codesProcessingService;
		private readonly ICounterpartyEdoAccountController _edoAccountController;
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
			IGenericRepository<Order> orderRepository,
			IGenericRepository<Warehouse> warehouseRepository,
			IGenericRepository<SelfDeliveryDocument> selfDeliveryDocumentRepository,
			IGenericRepository<GroupGtin> groupGtinrepository,
			IGenericRepository<Gtin> gtinRepository,
			IGenericRepository<SubdivisionEntity> subdivisionRepository,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			ISelfDeliveryDocumentItemTrueMarkProductCodesProcessingService codesProcessingService,
			ICounterpartyEdoAccountController edoAccountController,
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
			_codesProcessingService = codesProcessingService
				?? throw new ArgumentNullException(nameof(codesProcessingService));
			_edoAccountController = edoAccountController
				?? throw new ArgumentNullException(nameof(edoAccountController));
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
				return Result.Failure<SelfDeliveryDocument>(Vodovoz.Errors.Common.RepositoryErrors.DataRetrievalError);
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
				return Result.Failure<SelfDeliveryDocument>(Vodovoz.Errors.Common.RepositoryErrors.DataRetrievalError);
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

			var selfDeliveryDocument = new SelfDeliveryDocument
			{
				AuthorId = author.Id,
				Order = order,
				Warehouse = warehouse,
			};

			var defaultBottleNomenclatureId =
				_nomenclatureRepository.GetDefaultBottleNomenclatureId(_unitOfWork, cancellationToken);

			selfDeliveryDocument.FillByOrder();
			selfDeliveryDocument.UpdateStockAmount(_unitOfWork, _stockRepository);
			selfDeliveryDocument.UpdateAlreadyUnloaded(_unitOfWork, _nomenclatureRepository, _bottlesRepository);

			UpdateAmounts(selfDeliveryDocument);

			return await Task.FromResult(selfDeliveryDocument);
		}

		public async Task<Result<SelfDeliveryDocument>> AddCodes(
			SelfDeliveryDocument selfDeliveryDocument,
			IEnumerable<string> codesToAdd,
			CancellationToken cancellationToken)
		{
			var stagingCodes = await CreateStagingTrueMarkCodes(selfDeliveryDocument, codesToAdd, cancellationToken);

			var isCheckAllCodesScanned = _codesProcessingService.IsAllTrueMarkProductCodesMustBeAdded(selfDeliveryDocument, _edoAccountController);

			var addCodesResult =
				await AddProductCodesToSelfDeliveryDocumentItemAndDeleteStagingCodes(selfDeliveryDocument, stagingCodes, isCheckAllCodesScanned);

			if(addCodesResult.IsFailure)
			{
				return Result.Failure<SelfDeliveryDocument>(addCodesResult.Errors);
			}

			if(isCheckAllCodesScanned)
			{
				var isAllCodesAddedResult = _codesProcessingService.IsAllTrueMarkProductCodesAdded(selfDeliveryDocument);
				if(isAllCodesAddedResult.IsFailure)
				{
					return Result.Failure<SelfDeliveryDocument>(isAllCodesAddedResult.Errors);
				}
			}

			return selfDeliveryDocument;
		}

		private async Task<IEnumerable<StagingTrueMarkCode>> CreateStagingTrueMarkCodes(
			SelfDeliveryDocument document,
			IEnumerable<string> codes,
			CancellationToken cancellationToken)
		{
			var stagingCodes = new List<StagingTrueMarkCode>();

			foreach(var code in codes)
			{
				var createStagingCodeResult = await CreateStagingTrueMarkCode(code, cancellationToken);
				if(createStagingCodeResult.IsFailure)
				{
					_logger.LogError(
						"Ошибка создания кода для промежуточного хранения {Code}: {ErrorMessage}",
						code,
						string.Join(", ", createStagingCodeResult.Errors.Select(e => e.Message)));
				}

				var stagingCode = createStagingCodeResult.Value;

				var isCodeCanBeAddedResult =
					await _codesProcessingService.IsStagingTrueMarkCodeCanBeAddedToDocument(_unitOfWork, document, stagingCode, cancellationToken);

				if(isCodeCanBeAddedResult.IsFailure)
				{
					_logger.LogError(
						"Код для промежуточного хранения не может быть добавлен к документу {Code}: {ErrorMessage}",
						code,
						string.Join(", ", isCodeCanBeAddedResult.Errors.Select(e => e.Message)));
				}

				AddStagingTrueMarkCode(stagingCode, ref stagingCodes);
			}

			return stagingCodes;
		}

		private async Task<Result<StagingTrueMarkCode>> CreateStagingTrueMarkCode(string code, CancellationToken cancellationToken)
		{
			var createStagingTrueMarkCodeResult = await _codesProcessingService
				.CreateStagingTrueMarkCode(_unitOfWork, code, 0, cancellationToken);

			if(createStagingTrueMarkCodeResult.IsFailure)
			{
				return Result.Failure<StagingTrueMarkCode>(createStagingTrueMarkCodeResult.Errors);

			}

			return createStagingTrueMarkCodeResult.Value;
		}

		private void AddStagingTrueMarkCode(StagingTrueMarkCode newStagingCode, ref List<StagingTrueMarkCode> stagingCodes)
		{
			var alreadyAddedRootCode =
				GetStagingTrueMarkCodesAddedDuplicates(new[] { newStagingCode }, stagingCodes);

			if(alreadyAddedRootCode.Any())
			{
				return;
			}

			var alreadyAddedInnerCodes =
				GetStagingTrueMarkCodesAddedDuplicates(newStagingCode.AllCodes, stagingCodes);

			foreach(var code in alreadyAddedInnerCodes)
			{
				if(!stagingCodes.Contains(code))
				{
					continue;
				}

				stagingCodes.Remove(code);
			}

			stagingCodes.Add(newStagingCode);
		}

		private IEnumerable<StagingTrueMarkCode> GetStagingTrueMarkCodesAddedDuplicates(
			IEnumerable<StagingTrueMarkCode> newCodes,
			IEnumerable<StagingTrueMarkCode> addedCodes)
		{
			var existingCodes = new List<StagingTrueMarkCode>();
			var allScannedCodes = addedCodes.SelectMany(x => x.AllCodes).ToList();

			foreach(var code in newCodes)
			{
				var existingCodesPredicate = StagingTrueMarkCodeSpecification.CreateForEqualStagingCodes(code).Expression.Compile();

				existingCodes.AddRange(allScannedCodes
					.Where(existingCodesPredicate)
					.ToList());
			}

			return existingCodes;
		}

		private async Task<Result> AddProductCodesToSelfDeliveryDocumentItemAndDeleteStagingCodes(
			SelfDeliveryDocument document,
			IEnumerable<StagingTrueMarkCode> stagingCodes,
			bool isCheckAllCodesScanned)
		{
			if(isCheckAllCodesScanned)
			{
				var isAllCodesScanned = _codesProcessingService.IsAllCodesScanned(document, stagingCodes);

				if(!isAllCodesScanned)
				{
					return Result.Failure(TrueMarkCodeErrors.NotAllCodesAdded);
				}
			}

			var documentItemsScannedStagingCodes =
				_codesProcessingService.GetSelfDeliveryDocumentItemStagingTrueMarkCodes(document, stagingCodes);

			foreach(var item in document.Items)
			{
				var itemStagingCodes = documentItemsScannedStagingCodes.TryGetValue(item, out var itemCodes)
					? itemCodes
					: Enumerable.Empty<StagingTrueMarkCode>();

				if(!itemStagingCodes.Any())
				{
					continue;
				}

				var addingCodesResult = await _codesProcessingService.AddProductCodesToSelfDeliveryDocumentItem(
					_unitOfWork,
					item,
					itemStagingCodes);

				if(addingCodesResult.IsFailure)
				{
					return addingCodesResult;
				}
			}

			return Result.Success();
		}

		public async Task<Result<SelfDeliveryDocument>> EndLoad(SelfDeliveryDocument selfDeliveryDocument, CancellationToken cancellationToken)
		{
			selfDeliveryDocument.FullyShiped(
				_unitOfWork,
				_nomenclatureSettings,
				_routeListItemRepository,
				_selfDeliveryRepository,
				_cashRepository,
				_callTaskWorker);

			return await Task.FromResult(selfDeliveryDocument);
		}

		public async Task<Result<IEnumerable<Order>>> GetSelfDeliveryOrders(int warehouseId, CancellationToken cancellationToken)
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
					selfDeliveryDocument.GetNomenclaturesCountInOrder(item.Nomenclature.Id) - item.AmountUnloaded,
					item.AmountInStock);
			}
		}

		private Result<SelfDeliveryDocument> RemoveDistributedCodes(
			IList<TrueMarkAnyCode> anyCodes,
			SelfDeliveryDocument selfDeliveryDocument,
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

		private SelfDeliveryDocumentItem GetNextNotScannedDocumentItem(SelfDeliveryDocument selfDeliveryDocument, TrueMarkWaterIdentificationCode code)
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
