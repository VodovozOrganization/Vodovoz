﻿using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping
{
	public class AtWorkDriverMap : ClassMap<AtWorkDriver>
	{
		public AtWorkDriverMap ()
		{
			Table("at_work_drivers");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Date).Column("date");
			Map(x => x.PriorityAtDay).Column("piority_at_day");
			Map(x => x.EndOfDay).Column("end_of_day").CustomType<TimeAsTimeSpanType>();
			Map(x => x.Status).Column("status");
			Map(x => x.Reason).Column("reason");
			Map(x => x.Comment).Column("comment");
			Map(x => x.RemovedDate).Column("removed_date");
			Map(x => x.CommentLastEditedDate).Column("comment_last_edited_date");
			

			References(x => x.Employee).Column("employee_id");
			References(x => x.Car).Column("car_id");
			References(x => x.DaySchedule).Column("delivery_day_schedule_id");
			References(x => x.WithForwarder).Column("forwarder_id");
			References(x => x.GeographicGroup).Column("geographic_group_id");
			References(x => x.AuthorRemovedDriver).Column("author_removed_driver");
			References(x => x.CommentLastEditedAuthor).Column("comment_last_edited_author_id");


			HasMany(x => x.DistrictsPriorities).Cascade.AllDeleteOrphan().Inverse()
									 .KeyColumn("at_work_driver_id")
									 .AsList(x => x.Column("priority"));
		}
	}
}

