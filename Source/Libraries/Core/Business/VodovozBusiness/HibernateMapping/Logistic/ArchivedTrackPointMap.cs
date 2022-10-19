using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping
{
	public class ArchivedTrackPointMap : ClassMap<ArchivedTrackPoint>
	{
		public ArchivedTrackPointMap()
		{
			Schema("Vodovoz_old_monitoring");
			Table("track_points");

			CompositeId()
				.KeyReference(x => x.Track, "track_id")
				.KeyProperty(x => x.TimeStamp, "time_stamp");

			Map (x => x.Latitude).Column ("latitude");
			Map (x => x.Longitude).Column ("longitude");
			Map(x => x.ReceiveTimeStamp).Column("receive_time_stamp").ReadOnly();
		}
	}
}
