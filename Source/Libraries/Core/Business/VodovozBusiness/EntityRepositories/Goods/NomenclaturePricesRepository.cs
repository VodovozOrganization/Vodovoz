using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Goods
{
	public class NomenclaturePricesRepository : INomenclaturePricesRepository
	{
		public IList<NomenclatureFixedPrice> GetFixedPricesFor19LWater(IUnitOfWork uow)
		{
			Nomenclature nomenclatureAlias = null;

			return uow.Session.QueryOver<NomenclatureFixedPrice>()
				.Inner.JoinAlias(fp => fp.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water)
				.And(() => nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.List();
		}

		public IList<Nomenclature> GetNomenclaturesForGroupPricing(IUnitOfWork uow)
		{
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;

			var query = uow.Session.QueryOver(() => nomenclatureAlias)
				.Where(() => nomenclatureAlias.UsingInGroupPriceSet)
				.Future<Nomenclature>();

			var query2 = uow.Session.QueryOver(() => productGroupAlias)
				.Future<ProductGroup>();

			var query3 = uow.Session.QueryOver(() => nomenclatureAlias)
				.Where(() => nomenclatureAlias.UsingInGroupPriceSet)
				.Fetch(SelectMode.Fetch, () => nomenclatureAlias.PurchasePrices)
				.Future<Nomenclature>();

			var query4 = uow.Session.QueryOver(() => nomenclatureAlias)
				.Where(() => nomenclatureAlias.UsingInGroupPriceSet)
				.Fetch(SelectMode.Fetch, () => nomenclatureAlias.InnerDeliveryPrices)
				.Future<Nomenclature>();

			var query5 = uow.Session.QueryOver(() => nomenclatureAlias)
				.Where(() => nomenclatureAlias.UsingInGroupPriceSet)
				.Fetch(SelectMode.Fetch, () => nomenclatureAlias.ProductGroup)
				.Future<Nomenclature>();

			return query5.ToList();
		}
	}
}
