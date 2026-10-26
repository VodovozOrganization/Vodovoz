using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Payments
{
	public class OnlineRecurrentPaymentMap : ClassMap<OnlineRecurrentPayment>
	{
		public OnlineRecurrentPaymentMap()
		{
			Table("online_recurrent_payments");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Source)
				.Column("source")
				.Not.Nullable();

			Map(x => x.ExternalCounterpartyId)
				.Column("external_counterparty_id");
			
			Map(x => x.OnlineOrderTemplateId)
				.Column("online_order_template_id");
			
			Map(x => x.AccountId)
				.Column("account_id")
				.Not.Nullable();
			
			Map(x => x.Token)
				.Column("token")
				.Not.Nullable();
			
			Map(x => x.CardLastFour)
				.Column("card_last_four");
			
			Map(x => x.CardType)
				.Column("card_type");
		}
	}
}
