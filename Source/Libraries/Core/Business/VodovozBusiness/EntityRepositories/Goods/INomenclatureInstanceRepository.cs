using QS.DomainModel.UoW;

namespace Vodovoz.Domain.Goods
{
	public interface INomenclatureInstanceRepository
	{
		InventoryNomenclatureInstance GetInventoryNomenclatureInstance(IUnitOfWork uow, int id);
	}
}
