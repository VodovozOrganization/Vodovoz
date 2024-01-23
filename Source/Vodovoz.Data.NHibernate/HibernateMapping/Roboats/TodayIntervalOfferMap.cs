using FluentNHibernate.Mapping;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Roboats
{
	public class TodayIntervalOfferMap : ClassMap<TodayIntervalOffer>
	{
		public TodayIntervalOfferMap()
		{
			Table("roboats_today_interval_offers");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.DeliveryInterval).Column("interval_id");
			Map(x => x.StartHour).Column("start_offer_hour");
		}
	}
}
