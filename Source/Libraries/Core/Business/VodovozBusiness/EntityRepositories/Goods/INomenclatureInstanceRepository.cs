using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.Domain.Goods
{
	public interface INomenclatureInstanceRepository
	{
		InventoryNomenclatureInstance GetInventoryNomenclatureInstance(IUnitOfWork uow, int id);
		decimal GetNomenclatureInstanceBalance(IUnitOfWork uow, int instanceId);
		IList<NomenclatureInstanceRepository.InstanceOnStorageData> GetOtherInstancesOnStorageBalance(
			IUnitOfWork uow, StorageType storageType, int storageId, int[] instanceIds, DateTime? date = null);
		ILookup<int, NomenclatureInstanceRepository.InstanceOnStorageData> GetCurrentInstancesOnOtherStorages(
			IUnitOfWork uow, StorageType storageType, int storageId, int[] instanceIds, DateTime? date = null);

		IList<NomenclatureInstanceRepository.NomenclatureInstanceBalanceNode> GetInventoryInstancesByStorage(
			IUnitOfWork uow,
			StorageType storageType,
			int storageId,
			IEnumerable<int> instancesToInclude,
			IEnumerable<int> instancesToExclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToInclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToExclude,
			IEnumerable<int> productGroupToInclude,
			IEnumerable<int> productGroupToExclude,
			DateTime? onDate = null);
	}
}
