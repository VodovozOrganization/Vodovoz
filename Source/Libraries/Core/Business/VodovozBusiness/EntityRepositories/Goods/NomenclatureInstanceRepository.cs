using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Goods
{
	public class NomenclatureInstanceRepository : INomenclatureInstanceRepository
	{
		public InventoryNomenclatureInstance GetInventoryNomenclatureInstance(IUnitOfWork uow, int id)
			=> uow.GetById<InventoryNomenclatureInstance>(id);
	}
}
