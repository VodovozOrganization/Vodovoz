using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.Domain.Goods
{
	public interface INomenclatureInstanceRepository
	{
		InventoryNomenclatureInstance GetInventoryNomenclatureInstance(IUnitOfWork uow, int id);

		IList<NomenclatureInstanceRepository.NomenclatureInstanceBalanceNode> GetInventoryInstancesByStorage(
			IUnitOfWork uow, OperationTypeByStorage operationTypeByStorage, int storageId);
	}
}
