using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class VodovozWebSiteNomenclatureOnlineCatalogMap : SubclassMap<VodovozWebSiteNomenclatureOnlineCatalog>
	{
		public VodovozWebSiteNomenclatureOnlineCatalogMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForVodovozWebSite));
		}
	}
}
