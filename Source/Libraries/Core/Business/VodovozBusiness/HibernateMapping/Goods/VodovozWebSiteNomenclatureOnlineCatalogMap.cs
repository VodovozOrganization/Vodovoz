using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.HibernateMapping.Goods
{
	public class VodovozWebSiteNomenclatureOnlineCatalogMap : SubclassMap<VodovozWebSiteNomenclatureOnlineCatalog>
	{
		public VodovozWebSiteNomenclatureOnlineCatalogMap()
		{
			DiscriminatorValue(nameof(NomenclatureOnlineParameterType.ForVodovozWebSite));
		}
	}
}
