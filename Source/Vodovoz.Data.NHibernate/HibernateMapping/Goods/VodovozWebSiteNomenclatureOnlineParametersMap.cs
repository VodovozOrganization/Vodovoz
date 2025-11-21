using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
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
