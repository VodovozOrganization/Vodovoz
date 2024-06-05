using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Goods
{
	public interface INomenclaturePricesRepository
	{
		IList<Nomenclature> GetNomenclaturesForGroupPricing(IUnitOfWork uow);
	}
}
