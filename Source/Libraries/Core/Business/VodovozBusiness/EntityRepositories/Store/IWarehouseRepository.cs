using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.EntityRepositories.Store
{
	public interface IWarehouseRepository
	{
		IList<Warehouse> GetActiveWarehouse(IUnitOfWork uow);
		IList<Warehouse> WarehousesForPublishOnlineStore(IUnitOfWork uow);
		IEnumerable<NomenclatureStockNode> GetWarehouseNomenclatureStock(
			IUnitOfWork uow, OperationType operationType, int storageId, IEnumerable<int> nomenclatureIds);
		IEnumerable<Nomenclature> GetDiscrepancyNomenclatures(IUnitOfWork uow, int warehouseId);
		bool WarehouseByMovementDocumentsNotificationsSubdivisionExists(IUnitOfWork uow, int subdivisionId);
		int GetTotalShippedKgByWarehousesAndProductGroups(
			IUnitOfWork uow, DateTime dateFrom, DateTime dateTo, IEnumerable<int> productGroupsIds, IEnumerable<int> warehousesIds);
	}

	public class NomenclatureStockNode
	{
		public int NomenclatureId { get; set; }
		public decimal Stock { get; set; }

		public NomenclatureStockNode()
		{

		}

		public NomenclatureStockNode(int nomenclatureId, decimal stock)
		{
			NomenclatureId = nomenclatureId;
			Stock = stock;
		}
	}
}
