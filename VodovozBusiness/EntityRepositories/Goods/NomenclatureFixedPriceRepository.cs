using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Goods
{
	public class NomenclatureFixedPriceRepository : INomenclatureFixedPriceRepository
	{
		public IList<NomenclatureFixedPrice> GetFixedPricesFor19LWater(IUnitOfWork uow)
		{
			Nomenclature nomAlias = null;
			
			return uow.Session.QueryOver<NomenclatureFixedPrice>()
				.Inner.JoinAlias(fp => fp.Nomenclature, () => nomAlias)
				.Where(() => nomAlias.Category == NomenclatureCategory.water)
				.And(() => nomAlias.TareVolume == TareVolume.Vol19L)
				.List();
		}
	}
}
