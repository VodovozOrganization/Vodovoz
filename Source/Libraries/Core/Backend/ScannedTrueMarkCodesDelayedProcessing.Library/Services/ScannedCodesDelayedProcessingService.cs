using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using VodovozBusiness.Services.TrueMark;

namespace ScannedTrueMarkCodesDelayedProcessing.Library.Services
{
	public class ScannedCodesDelayedProcessingService
	{
		private readonly ILogger<ScannedCodesDelayedProcessingService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRouteListItemTrueMarkProductCodesProcessingService _routeListItemTrueMarkProductCodesProcessingService;
		private readonly IGenericRepository<DriversScannedTrueMarkCode> _driversScannedCodesRepository;
		private readonly IGenericRepository<RouteListItemEntity> _routeListItemRepostory;
		private readonly IGenericRepository<OrderItemEntity> _orderItemRepository;

		public ScannedCodesDelayedProcessingService(
			ILogger<ScannedCodesDelayedProcessingService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IRouteListItemTrueMarkProductCodesProcessingService routeListItemTrueMarkProductCodesProcessingService,
			IGenericRepository<DriversScannedTrueMarkCode> driversScannedCodesRepository,
			IGenericRepository<RouteListItemEntity> routeListItemRepostory,
			IGenericRepository<OrderItemEntity> orderItemRepository)
		{
			_logger =
				logger ?? throw new System.ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = 
				unitOfWorkFactory ?? throw new System.ArgumentNullException(nameof(unitOfWorkFactory));
			_routeListItemTrueMarkProductCodesProcessingService =
				routeListItemTrueMarkProductCodesProcessingService ?? throw new System.ArgumentNullException(nameof(routeListItemTrueMarkProductCodesProcessingService));
			_driversScannedCodesRepository =
				driversScannedCodesRepository ?? throw new System.ArgumentNullException(nameof(driversScannedCodesRepository));
			_routeListItemRepostory =
				routeListItemRepostory ?? throw new System.ArgumentNullException(nameof(routeListItemRepostory));
			_orderItemRepository =
				orderItemRepository ?? throw new System.ArgumentNullException(nameof(orderItemRepository));
		}

		public async Task ProcessScannedCodesAsync(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(ScannedCodesDelayedProcessingService)))
			{
				var scannedCodes = _driversScannedCodesRepository
					.Get(uow, x => x.IsProcessingCompleted == false)
					.ToList();

				var orderItemsIds = scannedCodes.Select(x => x.OrderItemId).Distinct();

				var orderIds = _orderItemRepository
					.Get(uow, x => orderItemsIds.Contains(x.Id))
					.ToList();

				var orderItemScannedCodes = scannedCodes
					.GroupBy(x => x.OrderItemId)
					.ToDictionary(x => x.Key, x => x.ToList());
			}
		}

		private async Task<IEnumerable<DriversScannedTrueMarkCode>> GetScannedCodes(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			return await uow.Session.Query<DriversScannedTrueMarkCode>()
				.Where(x => !x.IsProcessingCompleted)
				.ToListAsync(cancellationToken);
		}
	}
}
