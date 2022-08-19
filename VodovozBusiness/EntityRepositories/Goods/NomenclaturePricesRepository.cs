using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;
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

		public IList<NomenclatureCostPurchasePrice> GetCostPurchasePriceForGroupSet(IUnitOfWork uow)
		{
			NomenclatureCostPurchasePrice nomenclatureCostPurchasePriceAlias = null;
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver(() => nomenclatureCostPurchasePriceAlias)
				.JoinAlias(() => nomenclatureCostPurchasePriceAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.UsingInGroupPriceSet);
			return query.List();
		}

		public IList<NomenclatureInnerDeliveryPrice> GetInnerDeliveryPriceForGroupSet(IUnitOfWork uow)
		{
			NomenclatureInnerDeliveryPrice nomenclatureInnerDeliveryPrice = null;
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver(() => nomenclatureInnerDeliveryPrice)
				.JoinAlias(() => nomenclatureInnerDeliveryPrice.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.UsingInGroupPriceSet);
			return query.List();
		}

		public IList<Nomenclature> GetNomenclaturesForGroupPricing(IUnitOfWork uow)
		{
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver(() => nomenclatureAlias)
				.Where(() => nomenclatureAlias.UsingInGroupPriceSet)
				.Future<Nomenclature>();

			var query2 = uow.Session.QueryOver(() => nomenclatureAlias)
				.Where(() => nomenclatureAlias.UsingInGroupPriceSet)
				.Fetch(SelectMode.Fetch, () => nomenclatureAlias.PurchasePrices)
				.Future<Nomenclature>();

			var query3 = uow.Session.QueryOver(() => nomenclatureAlias)
				.Where(() => nomenclatureAlias.UsingInGroupPriceSet)
				.Fetch(SelectMode.Fetch, () => nomenclatureAlias.InnerDeliveryPrices)
				.Future<Nomenclature>();

			return query3.ToList();
		}
	}
}
