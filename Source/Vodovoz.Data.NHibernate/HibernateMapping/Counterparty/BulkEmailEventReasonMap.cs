using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class BulkEmailEventReasonMap : ClassMap<BulkEmailEventReason>
	{
		public BulkEmailEventReasonMap()
		{
			Table("bulk_email_event_reasons");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.HideForUnsubscribePage).Column("hide_for_unsubscribe_page");
		}
	}
}
