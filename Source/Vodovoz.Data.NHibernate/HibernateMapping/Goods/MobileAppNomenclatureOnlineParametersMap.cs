using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.HibernateMapping.Goods
{
	public class MobileAppNomenclatureOnlineParametersMap : SubclassMap<MobileAppNomenclatureOnlineParameters>
	{
		public MobileAppNomenclatureOnlineParametersMap()
		{
			DiscriminatorValue(nameof(NomenclatureOnlineParameterType.ForMobileApp));
		}
	}
}
