using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class TrackMap : ClassMap<Track>
	{
		public TrackMap ()
		{
			Table("tracks");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map (x => x.StartDate).Column ("start_date_time");
			References (x => x.Driver).Column ("driver_id");
			References (x => x.RouteList).Column ("route_list_id");
			HasMany (x => x.TrackPoints).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("track_id");
		}
	}
}

