using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class TariffZoneMap : ClassMap<TariffZone>
	{
		public TariffZoneMap()
		{
			Table("tariff_zones");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.IsFastDeliveryAvailable).Column("is_fast_delivery_available");
			Map(x => x.FastDeliveryTimeFrom).Column("fast_delivery_from_time").CustomType<TimeAsTimeSpanType>();
			Map(x => x.FastDeliveryTimeTo).Column("fast_delivery_to_time").CustomType<TimeAsTimeSpanType>();
		}
	}
}
