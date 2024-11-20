using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.Infrastructure.Persistance.Goods
{
	internal sealed class NomenclaturePricesRepository : INomenclaturePricesRepository
	{
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
