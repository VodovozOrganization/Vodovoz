using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Models;
using Vodovoz.EntityRepositories.Goods.BottleAnalytics;
using WhereIsTheBottle.Models.MainContent.Nodes;
using static Vodovoz.EntityRepositories.Goods.BottleAnalytics.AmountOnDateNode;

namespace WhereIsTheBottle.Models.MainContent
{
	public class GeneralDeltaModel : UoWFactoryModelBase
	{
		private readonly ICommonBottleAnalyticsRepository _commonBottleAnalyticsRepository;
		private readonly IGeneralDeltaBottleAnalyticsRepository _generalDeltaBottleAnalyticsRepository;

		public GeneralDeltaModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonBottleAnalyticsRepository commonBottleAnalyticsRepository,
			IGeneralDeltaBottleAnalyticsRepository generalDeltaBottleAnalyticsRepository)
			: base(unitOfWorkFactory)
		{
			_commonBottleAnalyticsRepository = commonBottleAnalyticsRepository
				?? throw new ArgumentNullException(nameof(commonBottleAnalyticsRepository));
			_generalDeltaBottleAnalyticsRepository = generalDeltaBottleAnalyticsRepository
				?? throw new ArgumentNullException(nameof(generalDeltaBottleAnalyticsRepository));
		}

		public IEnumerable<GeneralDeltaNode> GetGeneralDeltaNodes(DateTime startDate, DateTime endDate)
		{
			using var uow = UnitOfWorkFactory.CreateWithoutRoot();
			startDate = startDate.Date;
			endDate = endDate.Date.AddTicks(-1);

			#region Получение данных

			var nomenclatureIds = _commonBottleAnalyticsRepository.GetBottleAnalyticsNomenclatureIdsFuture(uow).ToArray();

			var warehouseIncomeFuture = _commonBottleAnalyticsRepository
				.GetIncomeWarehouseAssetFuture(uow, startDate.AddSeconds(-1), nomenclatureIds);
			var warehouseWriteoffFuture = _commonBottleAnalyticsRepository
				.GetWriteoffWarehouseAssetFuture(uow, startDate.AddSeconds(-1), nomenclatureIds);
			var warehouseIncomeByDatesFuture = _commonBottleAnalyticsRepository
				.GetIncomeWarehouseAssetByDatesFuture(uow, startDate, endDate, nomenclatureIds);
			var warehouseWriteoffByDatesFuture = _commonBottleAnalyticsRepository
				.GetWriteoffWarehouseAssetByDatesFuture(uow, startDate, endDate, nomenclatureIds);

			var movementDocumentIncomeFuture = _commonBottleAnalyticsRepository
				.GetIncomeMovementDocumentAssetFuture(uow, startDate.AddSeconds(-1), nomenclatureIds);
			var movementDocumentWriteoffFuture = _commonBottleAnalyticsRepository
				.GetWriteoffMovementDocumentAssetFuture(uow, startDate.AddSeconds(-1), nomenclatureIds);
			var movementDocumentIncomeByDatesFuture = _commonBottleAnalyticsRepository
				.GetIncomeMovementDocumentAssetByDatesFuture(uow, startDate, endDate, nomenclatureIds);
			var movementDocumentWriteoffByDatesFuture = _commonBottleAnalyticsRepository
				.GetWriteoffMovementDocumentAssetByDatesFuture(uow, startDate, endDate, nomenclatureIds);

			var routeListFuture = _commonBottleAnalyticsRepository
				.GetRouteListAssetFuture(uow, startDate.AddSeconds(-1), nomenclatureIds);
			var routeListByDatesFuture = _commonBottleAnalyticsRepository
				.GetRouteListAsseetByDatesFuture(uow, startDate, endDate, nomenclatureIds);

			var counterpartyReturnIncomeFuture = _generalDeltaBottleAnalyticsRepository
				.GetCounterpartyReturnIncome(uow, startDate, endDate.AddDays(1), nomenclatureIds);
			var counterpartyReturnLossFuture = _generalDeltaBottleAnalyticsRepository
				.GetCounterpartyReturnLoss(uow, startDate, endDate.AddDays(1), nomenclatureIds);

			var regradingIncomeFuture = _generalDeltaBottleAnalyticsRepository
				.GetRegradingIncome(uow, startDate, endDate.AddDays(1), nomenclatureIds);
			var regradingLossFuture = _generalDeltaBottleAnalyticsRepository
				.GetRegradingLoss(uow, startDate, endDate.AddDays(1), nomenclatureIds);

			var driverReturnIncomeFuture = _generalDeltaBottleAnalyticsRepository
				.GetDriverReturnIncome(uow, startDate, endDate.AddDays(1), nomenclatureIds);
			var driverReturnLossFuture = _generalDeltaBottleAnalyticsRepository
				.GetDriverReturnLoss(uow, startDate, endDate.AddDays(1), nomenclatureIds);

			var writeoffLossFuture = _generalDeltaBottleAnalyticsRepository
				.GetWriteoffLoss(uow, startDate, endDate.AddDays(1), nomenclatureIds);
			var incomingInvoiceIncomeFuture = _generalDeltaBottleAnalyticsRepository
				.GetIncomingInvoiceIncome(uow, startDate, endDate.AddDays(1), nomenclatureIds);

			var inventorizationIncomeFuture = _generalDeltaBottleAnalyticsRepository
				.GetInventorizationIncome(uow, startDate, endDate.AddDays(1), nomenclatureIds);
			var inventorizationLossFuture = _generalDeltaBottleAnalyticsRepository
				.GetInventorizationLossByDates(uow, startDate, endDate.AddDays(1), nomenclatureIds);

			var selfDeliveryIncomeFuture = _generalDeltaBottleAnalyticsRepository
				.GetSelfDeliveryIncome(uow, startDate, endDate.AddDays(1), nomenclatureIds);
			var selfDeliveryLossFuture = _generalDeltaBottleAnalyticsRepository
				.GetSelfDeliveryLoss(uow, startDate, endDate.AddDays(1), nomenclatureIds);

			var warehousesAsset = (warehouseIncomeFuture.Value ?? 0) - (warehouseWriteoffFuture.Value ?? 0);
			var warehouseIncomeByDates = warehouseIncomeByDatesFuture.ToDictionary(x => x.DateTime, x => x.Amount);
			var warehouseWriteoffByDates = warehouseWriteoffByDatesFuture.ToDictionary(x => x.DateTime, x => x.Amount);

			var routeListsAsset = routeListFuture.Value ?? 0;
			var routeListByDates = routeListByDatesFuture.ToDictionary(x => x.DateTime, x => x.Amount);

			var movementDocumentsAsset = (movementDocumentIncomeFuture.Value ?? 0) - (movementDocumentWriteoffFuture.Value ?? 0);
			var movementDocumentIncomeByDates = movementDocumentIncomeByDatesFuture.ToDictionary(x => x.DateTime, x => x.Amount);
			var movementDocumentWriteoffByDates = movementDocumentWriteoffByDatesFuture.ToDictionary(x => x.DateTime, x => x.Amount);

			var counterpartyReturnIncome = counterpartyReturnIncomeFuture.ToDictionary(x => x.DateTime, x => x.Amount);
			var counterpartyReturnLoss = counterpartyReturnLossFuture.ToDictionary(x => x.DateTime, x => x.Amount);

			var regradingIncome = regradingIncomeFuture.ToDictionary(x => x.DateTime, x => x.Amount);
			var regradingLoss = regradingLossFuture.ToDictionary(x => x.DateTime, x => x.Amount);

			var writeoffLoss = writeoffLossFuture.ToDictionary(x => x.DateTime, x => x.Amount);
			var incomingInvoiceIncome = incomingInvoiceIncomeFuture.ToDictionary(x => x.DateTime, x => x.Amount);

			var driverReturnIncome = driverReturnIncomeFuture.ToDictionary(x => x.DateTime, x => x.Amount);
			var driverReturnLoss = driverReturnLossFuture.ToDictionary(x => x.DateTime, x => x.Amount);

			var inventorizationIncome = inventorizationIncomeFuture.ToDictionary(x => x.DateTime, x => x.Amount);
			var inventorizationLoss = inventorizationLossFuture.ToDictionary(x => x.DateTime, x => x.Amount);

			var selfDeliveryIncome = selfDeliveryIncomeFuture.ToDictionary(x => x.DateTime, x => x.Amount);
			var selfDeliveryLoss = selfDeliveryLossFuture.ToDictionary(x => x.DateTime, x => x.Amount);

			#endregion

			#region Заполнение списка GeneralDeltaNode

			var firstNode = new GeneralDeltaNode
			{
				Date = startDate,

				WarehousesAsset = warehousesAsset,
				RouteListAsset = routeListsAsset,
				MovementDocumentsAsset = movementDocumentsAsset,

				InventarizationIncome = GetAmountOnDate(inventorizationIncome, startDate),
				InventarizationLoss = -GetAmountOnDate(inventorizationLoss, startDate),

				RegradingOfGoodsIncome = GetAmountOnDate(regradingIncome, startDate),
				RegradingOfGoodsLoss = -GetAmountOnDate(regradingLoss, startDate),

				CounterpartySelfDeliveryIncome = GetAmountOnDate(selfDeliveryIncome, startDate),
				CounterpartySelfDeliveryLoss = -GetAmountOnDate(selfDeliveryLoss, startDate),

				DriversDiscrepancyIncome = GetAmountOnDate(driverReturnIncome, startDate),
				DriversDiscrepancyLoss = -GetAmountOnDate(driverReturnLoss, startDate),

				IncomingInvoiceIncome = GetAmountOnDate(incomingInvoiceIncome, startDate),
				WriteoffDocumentLoss = -GetAmountOnDate(writeoffLoss, startDate),

				CounterpartyReturnIncome = GetAmountOnDate(counterpartyReturnIncome, startDate),
				CounterpartyReturnLoss = -GetAmountOnDate(counterpartyReturnLoss, startDate)
			};

			IList<GeneralDeltaNode> results = new List<GeneralDeltaNode> { firstNode };
			var previosNode = firstNode;
			for(var date = startDate; date < endDate; date = date.AddDays(1))
			{
				var node = new GeneralDeltaNode
				{
					Date = date.AddDays(1),
					WarehousesAsset = previosNode.WarehousesAsset
						+ GetAmountOnDate(warehouseIncomeByDates, date) - GetAmountOnDate(warehouseWriteoffByDates, date),
					RouteListAsset = previosNode.RouteListAsset
						+ GetAmountOnDate(routeListByDates, date),
					MovementDocumentsAsset = previosNode.MovementDocumentsAsset
						+ GetAmountOnDate(movementDocumentIncomeByDates, date) - GetAmountOnDate(movementDocumentWriteoffByDates, date)
				};
				previosNode = node;
				results.Add(node);
			}

			foreach(var node in results)
			{
				var date = node.Date;

				node.InventarizationIncome = GetAmountOnDate(inventorizationIncome, date);
				node.InventarizationLoss = -GetAmountOnDate(inventorizationLoss, date);

				node.RegradingOfGoodsIncome = GetAmountOnDate(regradingIncome, date);
				node.RegradingOfGoodsLoss = -GetAmountOnDate(regradingLoss, date);

				node.CounterpartySelfDeliveryIncome = GetAmountOnDate(selfDeliveryIncome, date);
				node.CounterpartySelfDeliveryLoss = -GetAmountOnDate(selfDeliveryLoss, date);

				node.DriversDiscrepancyIncome = GetAmountOnDate(driverReturnIncome, date);
				node.DriversDiscrepancyLoss = -GetAmountOnDate(driverReturnLoss, date);

				node.IncomingInvoiceIncome = GetAmountOnDate(incomingInvoiceIncome, date);
				node.WriteoffDocumentLoss = -GetAmountOnDate(writeoffLoss, date);

				node.CounterpartyReturnIncome = GetAmountOnDate(counterpartyReturnIncome, date);
				node.CounterpartyReturnLoss = -GetAmountOnDate(counterpartyReturnLoss, date);

				node.Calculate();
			}

			#endregion

			return results.OrderByDescending(x => x.Date);
		}
	}
}
