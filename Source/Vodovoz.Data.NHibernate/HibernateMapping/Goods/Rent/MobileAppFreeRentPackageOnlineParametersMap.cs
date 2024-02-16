using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.Rent;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods.Rent
{
	public class MobileAppFreeRentPackageOnlineParametersMap : SubclassMap<MobileAppFreeRentPackageOnlineParameters>
	{
		public MobileAppFreeRentPackageOnlineParametersMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForMobileApp));
		}
	}
}
