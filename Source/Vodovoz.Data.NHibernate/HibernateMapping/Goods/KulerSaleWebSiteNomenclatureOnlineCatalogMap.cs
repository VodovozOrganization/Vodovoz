using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class KulerSaleWebSiteNomenclatureOnlineCatalogMap : SubclassMap<KulerSaleWebSiteNomenclatureOnlineCatalog>
	{
		public KulerSaleWebSiteNomenclatureOnlineCatalogMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForKulerSaleWebSite));
		}
	}
}
