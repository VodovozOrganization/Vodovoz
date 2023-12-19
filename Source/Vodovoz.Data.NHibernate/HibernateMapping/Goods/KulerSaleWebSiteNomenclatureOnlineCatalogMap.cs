using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class KulerSaleWebSiteNomenclatureOnlineCatalogMap : SubclassMap<KulerSaleWebSiteNomenclatureOnlineCatalog>
	{
		public KulerSaleWebSiteNomenclatureOnlineCatalogMap()
		{
			DiscriminatorValue(nameof(NomenclatureOnlineParameterType.ForKulerSaleWebSite));
		}
	}
}
