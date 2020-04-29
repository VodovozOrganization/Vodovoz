using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Store;

namespace Vodovoz.Repository.Store
{
	[Obsolete("Утарел. Вместо него использовать Vodovoz.EntityRepositories.Store.WarehouseRepository")]
	public static class WarehouseRepository
	{
		public static IList<Warehouse> GetActiveWarehouse(IUnitOfWork uow)
		{
			return new EntityRepositories.Store.WarehouseRepository().GetActiveWarehouse(uow);
		}

		public static IList<Warehouse> WarehouseForShipment(IUnitOfWork uow, int routeListId)
		{
			return new EntityRepositories.Store.WarehouseRepository().WarehouseForShipment(uow, routeListId);
		}

		public static IList<Warehouse> WarehouseForReception(IUnitOfWork uow, int id)
		{
			return new EntityRepositories.Store.WarehouseRepository().WarehouseForReception(uow, id);
		}

		public static IList<Warehouse> WarehousesForPublishOnlineStore(IUnitOfWork uow)
		{
			return new EntityRepositories.Store.WarehouseRepository().WarehousesForPublishOnlineStore(uow);
		}
	}
}