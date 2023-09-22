using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Services.Logistics
{
	public interface IRouteListService
	{
		void AcceptConditions(IUnitOfWork unitOfWork, int driverId, IEnumerable<int> specialConditionsIds);
		IDictionary<int, string> GetSpecialConditionsDictionaryFor(IUnitOfWork unitOfWork, int routeListId);
		IEnumerable<RouteListSpecialCondition> GetSpecialConditionsFor(IUnitOfWork unitOfWork, int routeListId);
		void SendEnRoute(IUnitOfWork unitOfWork, int routeListId);
		void SendEnRoute(IUnitOfWork unitOfWork, RouteList routeList);
		bool TrySendEnRoute(IUnitOfWork unitOfWork, RouteList routeList, out IList<GoodsInRouteListResult> notLoadedGoods, CarLoadDocument withDocument = null);
	}
}
