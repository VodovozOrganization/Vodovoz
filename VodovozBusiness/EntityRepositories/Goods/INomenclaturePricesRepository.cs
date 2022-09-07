using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Goods
{
	public interface INomenclaturePricesRepository
	{
		IList<NomenclatureFixedPrice> GetFixedPricesFor19LWater(IUnitOfWork uow);
		IList<Nomenclature> GetNomenclaturesForGroupPricing(IUnitOfWork uow);
	}
}
