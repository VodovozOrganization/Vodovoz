using FluentNHibernate.Mapping;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.HibernateMapping.StoredEmails
{
	public class OrderDocumentEmailMap : ClassMap<OrderDocumentEmail>
	{
		public OrderDocumentEmailMap()
		{
			Table("order_document_emails");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.DocumentType).Column("document_type");
			References(x => x.Order).Column("order_id");
			References(x => x.StoredEmail).Column("stored_email_id");
		}
	}
}
