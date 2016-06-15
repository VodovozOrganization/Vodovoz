using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class TrackPointMap : ClassMap<TrackPoint>
	{
		public TrackPointMap ()
		{
			Table("track_points");

			CompositeId()
				.KeyReference(x => x.Track, "track_id")
				.KeyProperty(x => x.TimeStamp, "time_stamp");

			Map (x => x.Latitude).Column ("latitude");
			Map (x => x.Longitude).Column ("longitude");
		}
	}
}