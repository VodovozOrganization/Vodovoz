﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping
{
	public class TrackMap : ClassMap<Track>
	{
		public TrackMap ()
		{
			Table("tracks");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map (x => x.StartDate).Column ("start_date_time");
			Map (x => x.Distance).Column ("distance");
			Map (x => x.DistanceEdited).Column ("distance_edited");
			Map (x => x.DistanceToBase).Column ("distance_to_base");
			References (x => x.Driver).Column ("driver_id");
			References (x => x.RouteList).Column ("route_list_id");
			HasMany (x => x.TrackPoints).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("track_id");
		}
	}
}

