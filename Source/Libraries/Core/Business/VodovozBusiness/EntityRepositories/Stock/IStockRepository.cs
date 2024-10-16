using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Documents.MovementDocuments;

namespace Vodovoz.EntityRepositories.Stock
{
	public interface IStockRepository
	{
		decimal NomenclatureReserved(IUnitOfWork uow, int nomenclatureId);
		decimal GetStockForNomenclature(IUnitOfWork uow, int nomenclatureId);
		Dictionary<int, decimal> NomenclatureInStock(
			IUnitOfWork uow,
			int[] nomenclatureIds,
			IEnumerable<int> warehouseIds = null,
			DateTime? onDate = null);
		Dictionary<int, decimal> NomenclatureInStock(
			IUnitOfWork uow,
			int storageId,
			StorageType storageType,
			IEnumerable<int> nomenclaturesToInclude,
			IEnumerable<int> nomenclaturesToExclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToInclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToExclude,
			IEnumerable<int> productGroupToInclude,
			IEnumerable<int> productGroupToExclude,
			DateTime? onDate = null);
	}
}
