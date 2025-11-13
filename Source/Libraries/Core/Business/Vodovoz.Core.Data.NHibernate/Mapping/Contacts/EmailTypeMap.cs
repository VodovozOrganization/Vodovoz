using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Contacts;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Contacts
{
	public class EmailTypeMap : ClassMap<EmailTypeEntity>
	{
		public EmailTypeMap()
		{
			Table("email_types");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Name)
				.Column("name");

			Map(x => x.EmailPurpose)
				.Column("purpose");
		}
	}
}
