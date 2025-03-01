using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Contacts;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Contacts
{
	public class EmailMap : ClassMap<EmailEntity>
	{
		public EmailMap()
		{
			Table("emails");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Address)
				.Column("address")
				.Not.Nullable();

			References(x => x.EmailType)
				.Column("type_id");
		}
	}
}
