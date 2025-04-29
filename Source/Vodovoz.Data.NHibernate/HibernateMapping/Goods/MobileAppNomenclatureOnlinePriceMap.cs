using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class MobileAppNomenclatureOnlinePriceMap : SubclassMap<MobileAppNomenclatureOnlinePrice>
	{
		public MobileAppNomenclatureOnlinePriceMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForMobileApp));
		}
	}
}
