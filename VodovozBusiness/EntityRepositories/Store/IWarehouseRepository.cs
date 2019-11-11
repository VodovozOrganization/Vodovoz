using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Store;

namespace Vodovoz.EntityRepositories.Store
{
	public interface IWarehouseRepository
	{
		IList<Warehouse> GetActiveWarehouse(IUnitOfWork uow);

		IList<Warehouse> WarehouseForShipment(IUnitOfWork uow, int routeListId);

		IList<Warehouse> WarehouseForReception(IUnitOfWork uow, int id);

		IList<Warehouse> WarehousesForPublishOnlineStore(IUnitOfWork uow);

		IEnumerable<NomanclatureStockNode> GetWarehouseNomenclatureStock(IUnitOfWork uow, int warehouseId, IEnumerable<int> nomenclatureIds);
	}

	public class NomanclatureStockNode
	{
		public int NomenclatureId { get; set; }
		public decimal Stock { get; set; }

		public NomanclatureStockNode()
		{

		}

		public NomanclatureStockNode(int nomenclatureId, decimal stock)
		{
			NomenclatureId = nomenclatureId;
			Stock = stock;
		}
	}
}