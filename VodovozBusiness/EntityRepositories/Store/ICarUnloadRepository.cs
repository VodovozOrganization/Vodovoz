using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;

namespace Vodovoz.Repository.Store
{
	public interface ICarUnloadRepository
	{
		bool IsUniqueDocumentAtDay(IUnitOfWork UoW, RouteList routeList, Warehouse warehouse, int documentId);
		Dictionary<int, decimal> NomenclatureUnloaded(IUnitOfWork UoW, RouteList routeList, Warehouse warehouse, CarUnloadDocument excludeDoc);
	}
}