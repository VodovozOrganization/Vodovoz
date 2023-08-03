using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic.Drivers
{
	public class DriverStopListRemovalMap : ClassMap<DriverStopListRemoval>
	{
		public DriverStopListRemovalMap()
		{
			Table("driver_stop_list_removal");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DateFrom).Column("date_from");
			Map(x => x.DateTo).Column("date_to");
			Map(x => x.Comment).Column("comment");

			References(x => x.Driver).Column("driver_id");
			References(x => x.Author).Column("author_id");
		}
	}
}
