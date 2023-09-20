using QS.DomainModel.UoW;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories;
using Vodovoz.Services.Logistics;

namespace Vodovoz.Application.Services.Logistics
{
	public class RouteListService : IRouteLostService
	{
		private readonly IGenericRepository<RouteList> _routeListRepository;

		public RouteListService(
			IGenericRepository<RouteList> routeListRepository)
		{
			_routeListRepository = routeListRepository
				?? throw new System.ArgumentNullException(nameof(routeListRepository));
		}

		public RouteList GetRouteListWithSpecialConditions(IUnitOfWork unitOfWork, int routeListId)
		{
			var routeList = _routeListRepository
				.Get(unitOfWork, x => x.Id == routeListId)
				.FirstOrDefault();

			return routeList;
		}
	}
}
