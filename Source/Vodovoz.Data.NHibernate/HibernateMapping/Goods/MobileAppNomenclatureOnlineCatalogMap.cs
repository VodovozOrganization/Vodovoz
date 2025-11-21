using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class MobileAppNomenclatureOnlineCatalogMap : SubclassMap<MobileAppNomenclatureOnlineCatalog>
	{
		public MobileAppNomenclatureOnlineCatalogMap()
		{
			DiscriminatorValue(nameof(GoodsOnlineParameterType.ForMobileApp));
		}
	}
}
