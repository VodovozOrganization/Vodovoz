using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Documents
{
	public class CarLoadingDailyQueueMap : ClassMap<CarLoadingDailyQueue>
	{
		public CarLoadingDailyQueueMap()
		{
			Table("car_loading_daily_queue");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.DailyNumber)
				.Column("daily_number");

			Map(x => x.Date)
				.Column("date");

			References(x => x.RouteList)
				.Column("route_list_id");
		}
	}
}
