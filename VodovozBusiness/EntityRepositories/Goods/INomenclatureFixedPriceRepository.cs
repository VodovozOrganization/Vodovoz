using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Goods
{
	public interface INomenclatureFixedPriceRepository
	{
		IList<NomenclatureFixedPrice> GetFixedPricesFor19LWater(IUnitOfWork uow);
	}
}
