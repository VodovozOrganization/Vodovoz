using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Stock
{
	public interface IStockRepository
	{
		decimal NomenclatureReserved(IUnitOfWork uow, int nomenclatureId);
		int GetStockForNomenclature(IUnitOfWork uow, int nomenclatureId);
		Dictionary<int, decimal> NomenclatureInStock(IUnitOfWork uow, int warehouseId, int[] nomenclatureIds, DateTime? onDate = null);
		Dictionary<int, decimal> NomenclatureInStock(
			IUnitOfWork uow,
			int warehouseId,
			DateTime? onDate = null,
			NomenclatureCategory? nomenclatureCategory = null,
			ProductGroup nomenclatureType = null,
			Nomenclature nomenclature = null);
		Dictionary<int, decimal> NomenclatureInStock(
			IUnitOfWork uow,
			int warehouseId,
			int[] nomenclaturesToInclude,
			int[] nomenclaturesToExclude,
			string[] nomenclatureTypeToInclude,
			string[] nomenclatureTypeToExclude,
			int[] productGroupToInclude,
			int[] productGroupToExclude,
			DateTime? onDate = null);
		Dictionary<int, decimal> NomenclatureInStock(IUnitOfWork uow, int[] warehouseIds, int[] nomenclatureIds);
		decimal EquipmentInStock(IUnitOfWork uow, int warehouseId, int equipmentId);
		Dictionary<int, decimal> EquipmentInStock(IUnitOfWork uow, int warehouseId, int[] equipmentIds);
	}
}