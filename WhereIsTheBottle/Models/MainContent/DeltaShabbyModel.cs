using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Models;
using Vodovoz.EntityRepositories.Goods.BottleAnalytics;

namespace WhereIsTheBottle.Models.MainContent
{
	public class DeltaShabbyModel : UoWFactoryModelBase
	{
		private readonly ICommonBottleAnalyticsRepository _commonBottleAnalyticsRepository;
		private readonly IDeltaShabbyBottleAnalyticsRepository _deltaShabbyBottleAnalyticsRepository;

		public DeltaShabbyModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonBottleAnalyticsRepository commonBottleAnalyticsRepository,
			IDeltaShabbyBottleAnalyticsRepository deltaShabbyBottleAnalyticsRepository)
			: base(unitOfWorkFactory)
		{
			_commonBottleAnalyticsRepository = commonBottleAnalyticsRepository
				?? throw new ArgumentNullException(nameof(commonBottleAnalyticsRepository));
			_deltaShabbyBottleAnalyticsRepository = deltaShabbyBottleAnalyticsRepository
				?? throw new ArgumentNullException(nameof(deltaShabbyBottleAnalyticsRepository));
		}

		public IEnumerable<SummaryNode> GetWarehouseSummaryNodes(DateTime startDate, DateTime endDate)
		{
			using var uow = UnitOfWorkFactory.CreateWithoutRoot();
			startDate = startDate.Date;
			endDate = endDate.Date.AddDays(1).AddTicks(-1);

			var nomenclatureIds = _commonBottleAnalyticsRepository.GetBottleAnalyticsNomenclatureIdsFuture(uow).ToArray();

			return _deltaShabbyBottleAnalyticsRepository
				.GetRegradingToShabbyBottlesSummaryFuture(uow, startDate, endDate, nomenclatureIds)
				.ToList();
		}

		public IEnumerable<DetailedNode> GetDetailedWarehouseNodes(DateTime startDate, DateTime endDate, int? warehouseId)
		{
			using var uow = UnitOfWorkFactory.CreateWithoutRoot();
			startDate = startDate.Date;
			endDate = endDate.Date.AddDays(1).AddTicks(-1);

			var nomenclatureIds = _commonBottleAnalyticsRepository.GetBottleAnalyticsNomenclatureIdsFuture(uow).ToArray();

			return _deltaShabbyBottleAnalyticsRepository
				.GetRegradingToShabbyBottlesDetailedFuture(uow, startDate, endDate, warehouseId, nomenclatureIds)
				.ToList();
		}
	}
}
