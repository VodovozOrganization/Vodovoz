using FluentNHibernate.Mapping;
using NHibernate.Type;
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
			Map(x => x.From).Column("from_time").CustomType<TimeAsTimeSpanType>();
			Map(x => x.To).Column("to_time").CustomType<TimeAsTimeSpanType>();
		}
	}
}
