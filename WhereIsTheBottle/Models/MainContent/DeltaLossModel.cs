using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Models;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Goods.BottleAnalytics;

namespace WhereIsTheBottle.Models.MainContent
{
	public class DeltaLossModel : UoWFactoryModelBase
	{
		private readonly ICommonBottleAnalyticsRepository _commonBottleAnalyticsRepository;
		private readonly IDeltaLossBottleAnalyticsRepository _deltaLossBottleAnalyticsRepository;

		public DeltaLossModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonBottleAnalyticsRepository commonBottleAnalyticsRepository,
			IDeltaLossBottleAnalyticsRepository deltaLossBottleAnalyticsRepository)
			: base(unitOfWorkFactory)
		{
			_commonBottleAnalyticsRepository = commonBottleAnalyticsRepository
				?? throw new ArgumentNullException(nameof(commonBottleAnalyticsRepository));
			_deltaLossBottleAnalyticsRepository = deltaLossBottleAnalyticsRepository
				?? throw new ArgumentNullException(nameof(deltaLossBottleAnalyticsRepository));
		}

		public IEnumerable<SummaryNode> GetWarehouseSummaryNodes(DateTime startDate, DateTime endDate)
		{
			using var uow = UnitOfWorkFactory.CreateWithoutRoot();
			startDate = startDate.Date;
			endDate = endDate.Date.AddTicks(-1);

			var nomenclatureIds = _commonBottleAnalyticsRepository.GetBottleAnalyticsNomenclatureIdsFuture(uow).ToArray();

			var writeoffLossFuture = _deltaLossBottleAnalyticsRepository
				.GetWriteoffLossSummaryFuture(uow, startDate, endDate, nomenclatureIds);

			var inventarizationLossFuture = _deltaLossBottleAnalyticsRepository
				.GetInventarizationLossSummaryFuture(uow, startDate, endDate, nomenclatureIds);

			var driverReturnLossFuture = _deltaLossBottleAnalyticsRepository
				.GetDriverReturnSummaryLoss(uow, startDate, endDate, nomenclatureIds);

			var result = writeoffLossFuture
				.Concat(inventarizationLossFuture)
				.GroupBy(x => x.Name, (warehouseName, nodes) =>
					new SummaryNode
					{
						Name = warehouseName,
						Amount = nodes.Sum(x => x.Amount)
					})
				.ToList();

			var driverLossValue = new SummaryNode
			{
				Amount = driverReturnLossFuture.Value ?? 0,
				Name = "Водители"
			};

			if(driverLossValue.Amount != 0)
			{
				result.Add(driverLossValue);
			}

			return result;
		}

		public IEnumerable<DetailedNode> GetDetailedDriverNodes(DateTime startDate, DateTime endDate)
		{
			using var uow = UnitOfWorkFactory.CreateWithoutRoot();
			startDate = startDate.Date;
			endDate = endDate.Date.AddTicks(-1);

			var nomenclatureIds = _commonBottleAnalyticsRepository.GetBottleAnalyticsNomenclatureIds().ToArray();
			return _deltaLossBottleAnalyticsRepository.GetDriverLossByRouteListFuture(uow, startDate, endDate, nomenclatureIds)
				.ToList()
				.OrderByDescending(x => x.Date);
		}

		public IEnumerable<DetailedNode> GetDetailedWarehouseNodes(DateTime startDate, DateTime endDate, int? warehouseId)
		{
			using var uow = UnitOfWorkFactory.CreateWithoutRoot();
			startDate = startDate.Date;
			endDate = endDate.Date.AddTicks(-1);

			var nomenclatureIds = _commonBottleAnalyticsRepository.GetBottleAnalyticsNomenclatureIds().ToArray();
			return _deltaLossBottleAnalyticsRepository.GetWriteoffLoss(uow, startDate, endDate, nomenclatureIds, warehouseId)
				.Concat(_deltaLossBottleAnalyticsRepository.GetInventorizationLoss(uow, startDate, endDate, nomenclatureIds, warehouseId))
				.ToList()
				.OrderByDescending(x => x.Date)
				.ThenByDescending(x => x.DocumentNumber);
		}
	}
}
