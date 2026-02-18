using Edo.Contracts.Messages.Events;
using MassTransit;
using MassTransit.Initializers;
using Microsoft.Extensions.Logging;
using MoreLinq;
using MySqlConnector;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
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
using Vodovoz.Errors.Stores;
using Vodovoz.Errors.TrueMark;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Controllers;
using VodovozBusiness.Services.TrueMark;
using WarehouseApi.Contracts.Responses.V1;
using WarehouseApi.Library.Extensions;

namespace WarehouseApi.Library.Services
{
	internal sealed class SelfDeliveryService : ISelfDeliveryService
	{
		private readonly ILogger<SelfDeliveryService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly IGenericRepository<Warehouse> _warehouseRepository;
		private readonly IGenericRepository<Subdivision> _subdivisionRepository;
		private readonly ISelfDeliveryDocumentItemTrueMarkProductCodesProcessingService _codesProcessingService;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly ICounterpartyEdoAccountController _edoAccountController;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly ISelfDeliveryRepository _selfDeliveryRepository;
		private readonly ICashRepository _cashRepository;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IStockRepository _stockRepository;
		private readonly IBottlesRepository _bottlesRepository;
		private readonly IBus _messageBus;

		public SelfDeliveryService(
			ILogger<SelfDeliveryService> logger,
			IUnitOfWork unitOfWork,
			IGenericRepository<Order> orderRepository,
			IGenericRepository<Warehouse> warehouseRepository,
			IGenericRepository<Subdivision> subdivisionRepository,
			ISelfDeliveryDocumentItemTrueMarkProductCodesProcessingService codesProcessingService,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			ICounterpartyEdoAccountController edoAccountController,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureRepository nomenclatureRepository,
			IRouteListItemRepository routeListItemRepository,
			ISelfDeliveryRepository selfDeliveryRepository,
			ICashRepository cashRepository,
			ICallTaskWorker callTaskWorker,
			IStockRepository stockRepository,
			IBottlesRepository bottlesRepository,
			IBus messageBus)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
			_orderRepository = orderRepository
				?? throw new ArgumentNullException(nameof(orderRepository));
			_warehouseRepository = warehouseRepository
				?? throw new ArgumentNullException(nameof(warehouseRepository));
			_subdivisionRepository = subdivisionRepository
				?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_codesProcessingService = codesProcessingService
				?? throw new ArgumentNullException(nameof(codesProcessingService));
			_trueMarkWaterCodeService = trueMarkWaterCodeService
				?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
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
			_messageBus = messageBus
				?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task<Result<SelfDeliveryDocument>> CreateDocument(
			Employee author,
			int orderId,
			int warehouseId,
			CancellationToken cancellationToken)
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
				LastEditorId = author.Id,
				LastEditedTime = DateTime.Now,
				Order = order,
				Warehouse = warehouse
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
			var createStagingCodesResult = await CreateStagingTrueMarkCodes(selfDeliveryDocument, codesToAdd, cancellationToken);

			if(createStagingCodesResult.IsFailure)
			{
				return Result.Failure<SelfDeliveryDocument>(createStagingCodesResult.Errors);
			}

			var stagingCodes = createStagingCodesResult.Value;

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

		private async Task<Result<IEnumerable<StagingTrueMarkCode>>> CreateStagingTrueMarkCodes(
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

					return Result.Failure<IEnumerable<StagingTrueMarkCode>>(createStagingCodeResult.Errors);
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

					return Result.Failure<IEnumerable<StagingTrueMarkCode>>(isCodeCanBeAddedResult.Errors);
				}

				AddStagingTrueMarkCode(stagingCode, ref stagingCodes);
			}

			return Result.Success<IEnumerable<StagingTrueMarkCode>>(stagingCodes);
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
			var addedCodesData = addedCodes.SelectMany(x => x.AllCodes).ToLookup(code => (
				code.IsTransport,
				RawCode: code.IsTransport ? code.RawCode : null,
				Gtin: !code.IsTransport ? code.Gtin : null,
				Serial: !code.IsTransport ? code.SerialNumber : null
			));

			var existingCodes = new List<StagingTrueMarkCode>();

			foreach(var code in newCodes)
			{
				var key = (
					code.IsTransport,
					RawCode: code.IsTransport ? code.RawCode : null,
					Gtin: !code.IsTransport ? code.Gtin : null,
					Serial: !code.IsTransport ? code.SerialNumber : null
				);

				if(addedCodesData.Contains(key))
				{
					existingCodes.AddRange(addedCodesData[key]);
				}
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

		public async Task<Result<SelfDeliveryDocument>> SetTareToReturn(SelfDeliveryDocument selfDeliveryDocument, int tareToReturn)
		{
			selfDeliveryDocument.TareToReturn = tareToReturn;

			return await Task.FromResult(selfDeliveryDocument);
		}

		public async Task<Result<SelfDeliveryDocument>> EndLoad(SelfDeliveryDocument selfDeliveryDocument, CancellationToken cancellationToken)
		{
			selfDeliveryDocument.InitializeDefaultValues(_unitOfWork, _nomenclatureRepository);
			selfDeliveryDocument.UpdateOperations(_unitOfWork);

			selfDeliveryDocument.UpdateReceptions(
				_unitOfWork,
				new List<GoodsReceptionVMNode>(),
				_nomenclatureRepository,
				_bottlesRepository);

			var isFullyShiped = selfDeliveryDocument.FullyShiped(
				_unitOfWork,
				_nomenclatureSettings,
				_routeListItemRepository,
				_selfDeliveryRepository,
				_cashRepository,
				_callTaskWorker);

			if(!isFullyShiped)
			{
				return Result.Failure<SelfDeliveryDocument>(SelfDeliveryDocumentErrors.IsNotFullyShiped);
			}

			var edoRequest = CreateEdoRequest(selfDeliveryDocument);

			await _unitOfWork.SaveAsync(selfDeliveryDocument, cancellationToken: cancellationToken);
			await _unitOfWork.SaveAsync(edoRequest, cancellationToken: cancellationToken);

			try
			{
				await _unitOfWork.CommitAsync(cancellationToken);
			}
			catch(MySqlException mysqlException) when(mysqlException.ErrorCode == MySqlErrorCode.DuplicateKey)
			{
				_logger.LogError(mysqlException, "DuplicateEntry: {ExceptionMessage}", mysqlException.Message);
				var error = new Error("Database.Commit.Error", "Код уже был добавлен в другом документе");
				return Result.Failure<SelfDeliveryDocument>(error);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Exception while commiting: {ExceptionMessage}", e.Message);
				var error = new Error("Database.Commit.Error", e.Message);
				return Result.Failure<SelfDeliveryDocument>(error);
			}

			await SendEdoRequestCreatedEvent(edoRequest.Id);

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

			var warehouseGeoGroupId = GetWarehouseGeoGroupId(warehouse);

			var orders = _orderRepository
				.Get(
					_unitOfWork,
					o =>
						o.SelfDelivery
						&& o.SelfDeliveryGeoGroup.Id == warehouseGeoGroupId
						&& o.OrderStatus == OrderStatus.OnLoading)
				.ToList();

			return orders;
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

		private int? GetWarehouseGeoGroupId(Warehouse warehouse)
		{
			var subdivisionId = warehouse.OwningSubdivisionId;
			int? warehouseGeoGroupId = null;

			while(subdivisionId != null && warehouseGeoGroupId is null)
			{
				var subdivision = _subdivisionRepository
					.GetFirstOrDefault(_unitOfWork, s => s.Id == subdivisionId);

				subdivisionId = subdivision?.ParentSubdivision?.Id;
				warehouseGeoGroupId = subdivision?.GeographicGroup?.Id;
			}

			return warehouseGeoGroupId;
		}

		private PrimaryEdoRequest CreateEdoRequest(SelfDeliveryDocument selfDeliveryDocument)
		{
			var codes = selfDeliveryDocument.Items
				.SelectMany(x => x.TrueMarkProductCodes)
				.ToList();

			var edoRequest = new PrimaryEdoRequest
			{
				Time = DateTime.Now,
				Source = CustomerEdoRequestSource.Selfdelivery,
				Order = selfDeliveryDocument.Order
			};

			foreach(var code in codes)
			{
				edoRequest.ProductCodes.Add(code);
			}

			return edoRequest;
		}

		private async Task SendEdoRequestCreatedEvent(int requestId)
		{
			_logger.LogInformation(
				"Отправляем событие создания новой заявки на отправку документов ЭДО.  Id заявки: {RequestId}.",
				requestId);

			try
			{
				await _messageBus.Publish(new EdoRequestCreatedEvent { Id = requestId });
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при отправке события создания новой заявки на отправку документов ЭДО. Id заявки: {RequestId}. Exception: {ExceptionMessage}",
					requestId,
					ex.Message);
			}
		}

		public GetSelfDeliveryResponse CreateSelfDeliveryResponse(SelfDeliveryDocument selfDeliveryDocument)
		{
			var nomenclatures =
				selfDeliveryDocument.Order.OrderItems
				.Select(x => x.Nomenclature)
				.ToArray();

			var response = new GetSelfDeliveryResponse
			{
				SelfDeliveryDocumentId = selfDeliveryDocument.Id,
				Order = selfDeliveryDocument.Order.ToApiDtoV1(nomenclatures, selfDeliveryDocument)
			};

			response.Order.Items
				.PopulateRelatedCodes(_unitOfWork, _trueMarkWaterCodeService, selfDeliveryDocument.Items.SelectMany(x => x.TrueMarkProductCodes));

			response.Order.Items.ForEach(item =>
				item.Codes.ForEach((code, i) =>
					code.SequenceNumber = i));

			return response;
		}
	}
}
