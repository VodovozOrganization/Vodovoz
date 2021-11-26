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
	public class GeneralSummaryModel : UoWFactoryModelBase
	{
		private readonly ICommonBottleAnalyticsRepository _commonBottleAnalyticsRepository;
		private int _necessaryAssetValue = 100;
		private int _minimalAssetValue = 50;

		public GeneralSummaryModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonBottleAnalyticsRepository commonBottleAnalyticsRepository)
			: base(unitOfWorkFactory)
		{
			_commonBottleAnalyticsRepository = commonBottleAnalyticsRepository ??
				throw new ArgumentNullException(nameof(commonBottleAnalyticsRepository));
		}

		public int NecessaryAssetValue
		{
			get => _necessaryAssetValue;
			set
			{
				if(SetField(ref _necessaryAssetValue, value))
				{
					if(MinimalAssetValue > NecessaryAssetValue)
					{
						MinimalAssetValue = NecessaryAssetValue;
					}
					OnPropertyChanged(nameof(MinimalAssetPercent));
				}
			}
		}

		public int MinimalAssetValue
		{
			get => _minimalAssetValue;
			set
			{
				if(SetField(ref _minimalAssetValue, value > NecessaryAssetValue ? NecessaryAssetValue : value))
				{
					OnPropertyChanged(nameof(MinimalAssetPercent));
				}
			}
		}

		public double MinimalAssetPercent
		{
			get => NecessaryAssetValue == 0 ? double.NaN : Math.Round((double)MinimalAssetValue / NecessaryAssetValue * 100, 2);
			set
			{
				if(value > 100)
				{
					value = 100;
				}
				if(value < 0)
				{
					value = 0;
				}
				MinimalAssetValue = Convert.ToInt32(NecessaryAssetValue * (value / 100));
			}
		}

		public IEnumerable<GeneralSummaryNode> GetGeneralSummaryNodes(DateTime startDate, DateTime endDate,
			int necessaryAsset, int minimalAsset)
		{
			startDate = startDate.Date;
			endDate = endDate.Date.AddTicks(-1);
			using var uow = UnitOfWorkFactory.CreateWithoutRoot();

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

			var warehousesAsset = (warehouseIncomeFuture.Value ?? 0) - (warehouseWriteoffFuture.Value ?? 0);
			var warehouseIncomeByDates = warehouseIncomeByDatesFuture.ToDictionary(x => x.DateTime, x => x.Amount);
			var warehouseWriteoffByDates = warehouseWriteoffByDatesFuture.ToDictionary(x => x.DateTime, x => x.Amount);

			var routeListsAsset = routeListFuture.Value ?? 0;
			var routeListByDates = routeListByDatesFuture.ToDictionary(x => x.DateTime, x => x.Amount);

			var movementDocumentsAsset = (movementDocumentIncomeFuture.Value ?? 0) - (movementDocumentWriteoffFuture.Value ?? 0);
			var movementDocumentIncomeByDates = movementDocumentIncomeByDatesFuture.ToDictionary(x => x.DateTime, x => x.Amount);
			var movementDocumentWriteoffByDates = movementDocumentWriteoffByDatesFuture.ToDictionary(x => x.DateTime, x => x.Amount);

			var result = new List<GeneralSummaryNode>
			{
				new()
				{
					Date = startDate,
					WarehousesAsset = warehousesAsset,
					RouteListsAsset = routeListsAsset,
					MovementDocumentsAsset = movementDocumentsAsset
				}
			};

			GeneralSummaryNode previosNode = result.First();
			previosNode.NecessaryAssetDifference = previosNode.AssetByMorning - necessaryAsset;
			previosNode.NecessaryAssetPercent = necessaryAsset == 0 ? 0 : (double)previosNode.AssetByMorning / necessaryAsset;

			for(var date = startDate; date < endDate; date = date.AddDays(1))
			{
				var newNode = new GeneralSummaryNode
				{
					Date = date.AddDays(1),
					DayOfWeek = date.AddDays(1).DayOfWeek,
					MinimalAsset = minimalAsset,
					NecessaryAsset = necessaryAsset,
					WarehousesAsset = previosNode.WarehousesAsset
						+ GetAmountOnDate(warehouseIncomeByDates, date) - GetAmountOnDate(warehouseWriteoffByDates, date),
					RouteListsAsset = previosNode.RouteListsAsset + GetAmountOnDate(routeListByDates, date),
					MovementDocumentsAsset = previosNode.MovementDocumentsAsset
						+ GetAmountOnDate(movementDocumentIncomeByDates, date) - GetAmountOnDate(movementDocumentWriteoffByDates, date)
				};
				newNode.NecessaryAssetDifference = newNode.AssetByMorning - necessaryAsset;
				newNode.NecessaryAssetPercent = necessaryAsset == 0 ? 0 : (double)newNode.AssetByMorning / necessaryAsset;

				previosNode.BottleDifference = newNode.AssetByMorning - previosNode.AssetByMorning;
				previosNode.PercentDifference = previosNode.AssetByMorning == 0
					? 0
					: (double)previosNode.BottleDifference / previosNode.AssetByMorning;
				result.Add(newNode);
				previosNode = newNode;
			}

			return result.OrderByDescending(x => x.Date);
		}
	}
}
