using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Repository.Store
{
	public interface ICarUnloadRepository
	{
		bool IsUniqueDocumentAtDay(IUnitOfWork UoW, RouteList routeList, Warehouse warehouse, int documentId);
		decimal UnloadedTerminalAmount(IUnitOfWork uow, int routelistId, int terminalId);
		Dictionary<int, decimal> NomenclatureUnloaded(IUnitOfWork UoW, RouteList routeList, Warehouse warehouse, CarUnloadDocument excludeDoc);
	}
}