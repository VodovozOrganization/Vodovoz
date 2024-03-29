﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

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
