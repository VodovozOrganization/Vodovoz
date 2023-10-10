﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods.Rent;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods.Rent
{
	public class KulerSaleWebSiteFreeRentPackageOnlineParametersMap : SubclassMap<KulerSaleWebSiteFreeRentPackageOnlineParameters>
	{
		public KulerSaleWebSiteFreeRentPackageOnlineParametersMap()
		{
			DiscriminatorValue(nameof(NomenclatureOnlineParameterType.ForKulerSaleWebSite));
		}
	}
}
