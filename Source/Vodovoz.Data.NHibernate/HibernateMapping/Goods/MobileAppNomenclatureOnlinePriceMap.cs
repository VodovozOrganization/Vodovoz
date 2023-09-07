using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.HibernateMapping.Goods
{
	public class MobileAppNomenclatureOnlinePriceMap : SubclassMap<MobileAppNomenclatureOnlinePrice>
	{
		public MobileAppNomenclatureOnlinePriceMap()
		{
			DiscriminatorValue(nameof(NomenclatureOnlineParameterType.ForMobileApp));
		}
	}
}
