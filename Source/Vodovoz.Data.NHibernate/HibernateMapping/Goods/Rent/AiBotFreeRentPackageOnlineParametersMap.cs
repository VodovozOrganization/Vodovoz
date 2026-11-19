using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using VodovozBusiness.Domain.Goods.Rent;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods.Rent
{
	public class AiBotFreeRentPackageOnlineParametersMap : SubclassMap<AiBotFreeRentPackageOnlineParameters>
	{
		public AiBotFreeRentPackageOnlineParametersMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForAiBot));
		}
	}
}
