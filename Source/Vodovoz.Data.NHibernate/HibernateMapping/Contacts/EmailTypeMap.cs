using FluentNHibernate.Mapping;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Contacts
{
	public class EmailTypeMap : ClassMap<EmailType>
	{
		public EmailTypeMap()
		{
			Table("email_types");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.EmailPurpose).Column("purpose");
		}
	}
}
