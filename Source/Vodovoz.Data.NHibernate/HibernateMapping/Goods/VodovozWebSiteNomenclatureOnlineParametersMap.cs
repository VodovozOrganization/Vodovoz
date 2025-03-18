using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class VodovozWebSiteNomenclatureOnlineParametersMap : SubclassMap<VodovozWebSiteNomenclatureOnlineParameters>
	{
		public VodovozWebSiteNomenclatureOnlineParametersMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForVodovozWebSite));
		}
	}
}
