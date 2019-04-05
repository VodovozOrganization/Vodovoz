using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sale;

namespace Vodovoz.HibernateMapping.Sale
{
	public class TariffZoneMap : ClassMap<TariffZone>
	{
		public TariffZoneMap()
		{
			Table("tariff_zones");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
		}
	}
}
