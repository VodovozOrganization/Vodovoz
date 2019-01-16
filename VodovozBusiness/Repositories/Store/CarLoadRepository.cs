using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.Repositories.Store
{
	public static class CarLoadRepository
	{
		public static Dictionary<int, decimal> NomenclaturesLoadedOnWarehouse(IUnitOfWork uow, RouteList routeList, Warehouse warehouse, int? excludeDocId = null)
		{
			CarLoadDocument docAlias = null;
			CarLoadDocumentItem docItemsAlias = null;
			WarehouseMovementOperation movementOperationAlias = null;

			var query = uow.Session.QueryOver(() => docAlias)
						   .Where(d => d.RouteList.Id == routeList.Id)
						   .Where(d => d.Warehouse.Id == warehouse.Id);
			if(excludeDocId.HasValue)
				query.Where(d => d.Id != excludeDocId.Value);
			var loadedList = query.JoinAlias(d => d.Items, () => docItemsAlias)
									.JoinAlias(() => docItemsAlias.MovementOperation, () => movementOperationAlias)
									.SelectList(
										list => list
										.SelectGroup(() => movementOperationAlias.Nomenclature.Id)
										.SelectSum(() => movementOperationAlias.Amount)
						   			).List<object[]>();
			return loadedList.ToDictionary(r => (int)r[0], r => (decimal)r[1]);
		}
	}
}
