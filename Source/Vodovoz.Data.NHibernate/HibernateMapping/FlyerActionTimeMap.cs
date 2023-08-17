using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.Data.NHibernate.HibernateMapping
{
	public class FlyerActionTimeMap : ClassMap<FlyerActionTime>
	{
		public FlyerActionTimeMap()
		{
			Table("flyers_action_times");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");

			References(x => x.Flyer).Column("flyer_id");
		}
	}
}