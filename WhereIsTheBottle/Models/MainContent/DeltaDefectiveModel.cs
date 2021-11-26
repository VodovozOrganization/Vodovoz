using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Models;
using Vodovoz.EntityRepositories.Goods.BottleAnalytics;

namespace WhereIsTheBottle.Models.MainContent
{
	public class DeltaDefectiveModel : UoWFactoryModelBase
	{
		private readonly ICommonBottleAnalyticsRepository _commonBottleAnalyticsRepository;
		private readonly IDeltaDefectiveBottleAnalyticsRepository _deltaDefectiveBottleAnalyticsRepository;

		public DeltaDefectiveModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonBottleAnalyticsRepository commonBottleAnalyticsRepository,
			IDeltaDefectiveBottleAnalyticsRepository deltaDefectiveBottleAnalyticsRepository)
			: base(unitOfWorkFactory)
		{
			_commonBottleAnalyticsRepository = commonBottleAnalyticsRepository
				?? throw new ArgumentNullException(nameof(commonBottleAnalyticsRepository));
			_deltaDefectiveBottleAnalyticsRepository = deltaDefectiveBottleAnalyticsRepository
				?? throw new ArgumentNullException(nameof(deltaDefectiveBottleAnalyticsRepository));
		}

		public IEnumerable<SummaryNode> GetWarehouseSummaryNodes(DateTime startDate, DateTime endDate)
		{
			using var uow = UnitOfWorkFactory.CreateWithoutRoot();
			startDate = startDate.Date;
			endDate = endDate.Date.AddDays(1).AddTicks(-1);

			var nomenclatureIds = _commonBottleAnalyticsRepository.GetBottleAnalyticsNomenclatureIdsFuture(uow).ToArray();

			return _deltaDefectiveBottleAnalyticsRepository
				.GetRegradingToDefectiveBottlesSummaryFuture(uow, startDate, endDate, nomenclatureIds)
				.ToList();
		}

		public IEnumerable<DetailedNode> GetDetailedWarehouseNodes(DateTime startDate, DateTime endDate, int? warehouseId)
		{
			using var uow = UnitOfWorkFactory.CreateWithoutRoot();
			startDate = startDate.Date;
			endDate = endDate.Date.AddDays(1).AddTicks(-1);

			var nomenclatureIds = _commonBottleAnalyticsRepository.GetBottleAnalyticsNomenclatureIdsFuture(uow).ToArray();

			return _deltaDefectiveBottleAnalyticsRepository
				.GetRegradingToDefectiveBottlesDetailedFuture(uow, startDate, endDate, warehouseId, nomenclatureIds)
				.ToList();
		}
	}
}
