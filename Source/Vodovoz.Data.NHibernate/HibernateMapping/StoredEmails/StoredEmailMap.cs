using FluentNHibernate.Mapping;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.Data.NHibernate.HibernateMapping.StoredEmails
{
	public class StoredEmailMap : ClassMap<StoredEmail>
	{
		public StoredEmailMap()
		{
			Table("stored_emails");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.ExternalId).Column("external_id");
			Map(x => x.SendDate).Column("send_date");
			Map(x => x.State).Column("state");
			Map(x => x.StateChangeDate).Column("state_change_date");
			Map(x => x.Description).Column("description");
			Map(x => x.RecipientAddress).Column("recipient_address");
			Map(x => x.ManualSending).Column("manual_sending");
			Map(x => x.Subject).Column("subject");
			Map(x => x.Guid).Column("guid");
			References(x => x.Author).Column("author_id");
		}
	}
}
