using FluentNHibernate.Mapping;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Contacts
{
	public class PhoneMap : ClassMap<Phone>
	{
		public PhoneMap()
		{
			Table("phones");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Number).Column("number");
			Map(x => x.DigitsNumber).Column("digits_number");
			Map(x => x.Additional).Column("additional");
			Map(x => x.Comment).Column("comment");
			Map(x => x.IsArchive).Column("is_archive");

			References(x => x.PhoneType).Column("type_id").Not.LazyLoad();
			References(x => x.DeliveryPoint).Column("delivery_point_id");
			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.Employee).Column("employee_id");
			References(x => x.RoboAtsCounterpartyName).Column("roboats_counterparty_name_id");
			References(x => x.RoboAtsCounterpartyPatronymic).Column("roboats_counterparty_patronymic_id");
		}
	}
}

