using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;

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
		IEnumerable<SelfDeliveryAddressDto> GetSelfDeliveriesAddresses(IUnitOfWork unitOfWork);

		/// <summary>
		/// Проверяет, есть ли у склада ненулевые остатки ТМЦ
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="warehouseId">Идентификатор склада</param>
		/// <returns>True - за складом числятся ненулевые остатки, иначе - False</returns>
		bool HasNonZeroBalance(IUnitOfWork uow, int warehouseId);
	}
}
