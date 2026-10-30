using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Sale;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class OnlineOrderTimestampMap : ClassMap<OnlineOrderTimestamp>
	{
		public OnlineOrderTimestampMap()
		{
			Table("online_orders_timestamps");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.StartTime).Column("start_time");
			Map(x => x.TimerSeconds).Column("timer_seconds");
			Map(x => x.Type).Column("type");
		}
	}
}
