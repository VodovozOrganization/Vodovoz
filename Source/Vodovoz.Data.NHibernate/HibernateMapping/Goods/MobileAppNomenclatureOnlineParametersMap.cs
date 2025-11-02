using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class MobileAppNomenclatureOnlineParametersMap : SubclassMap<MobileAppNomenclatureOnlineParameters>
	{
		public MobileAppNomenclatureOnlineParametersMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForMobileApp));
		}
	}
}
