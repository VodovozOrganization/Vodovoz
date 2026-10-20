using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.StoredEmails;

namespace Vodovoz.Core.Data.NHibernate.Mapping.StoredEmails
{
	public class ChainStoreDelayLetterMap : ClassMap<ChainStoreDelayLetter>
	{
		public ChainStoreDelayLetterMap()
		{
			Table("chain_store_delay_letters");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreationDate).Column("creation_date");
			Map(x => x.LastUpdateDate).Column("last_update_date");
			Map(x => x.Status).Column("status");
			Map(x => x.OrderId).Column("order_id");
			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.EstimatedPaymentDate).Column("estimated_payment_date");
			Map(x => x.CounterpartyEmailId).Column("counterparty_email_id");
			Map(x => x.LastError).Column("last_error");
		}
	}
}
