using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.Rent;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods.Rent
{
	public class VodovozWebSiteFreeRentPackageOnlineParametersMap : SubclassMap<VodovozWebSiteFreeRentPackageOnlineParameters>
	{
		public VodovozWebSiteFreeRentPackageOnlineParametersMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForVodovozWebSite));
		}
	}
}
