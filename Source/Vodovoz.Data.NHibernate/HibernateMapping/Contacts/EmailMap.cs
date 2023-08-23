using FluentNHibernate.Mapping;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Contacts
{
	public class EmailMap : ClassMap<Email>
	{
		public EmailMap()
		{
			Table("emails");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Address).Column("address").Not.Nullable();

			References(x => x.EmailType).Column("type_id");

			References(x => x.Counterparty).Column("counterparty_id");
		}
	}
}

