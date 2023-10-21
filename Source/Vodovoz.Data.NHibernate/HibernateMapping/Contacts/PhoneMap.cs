using FluentNHibernate.Mapping;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Contacts
{
	public class PhoneMap : SubclassMap<Phone>
	{
		public PhoneMap()
		{
			References(x => x.PhoneType).Column("type_id").Not.LazyLoad();
			References(x => x.DeliveryPoint).Column("delivery_point_id");
			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.RoboAtsCounterpartyName).Column("roboats_counterparty_name_id");
			References(x => x.RoboAtsCounterpartyPatronymic).Column("roboats_counterparty_patronymic_id");
		}
	}
}

