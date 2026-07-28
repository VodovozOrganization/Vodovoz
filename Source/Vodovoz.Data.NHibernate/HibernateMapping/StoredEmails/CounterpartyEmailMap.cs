using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.StoredEmails;
using Vodovoz.Domain.StoredEmails;
using VodovozBusiness.Domain.StoredEmails;

namespace Vodovoz.Data.NHibernate.HibernateMapping.StoredEmails
{
	public class CounterpartyEmailMap : ClassMap<CounterpartyEmail>
	{
		public CounterpartyEmailMap()
		{
			Table("counterparty_emails");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("type");

			Map(x => x.Type).Column("type").ReadOnly();
			Map(x => x.OrganizationId).Column("organization_id");

			References(x => x.StoredEmail).Column("stored_email_id");
			References(x => x.Counterparty).Column("counterparty_id");
		}

		public class OrderWithoutShipmentForDebtEmailMap : SubclassMap<OrderWithoutShipmentForDebtEmail>
		{
			public OrderWithoutShipmentForDebtEmailMap()
			{
				DiscriminatorValue(nameof(CounterpartyEmailType.OrderWithoutShipmentForDebt));
				References(x => x.OrderWithoutShipmentForDebt).Column("bill_ws_for_debt_id");
			}
		}

		public class OrderWithoutShipmentForPaymentEmailMap : SubclassMap<OrderWithoutShipmentForPaymentEmail>
		{
			public OrderWithoutShipmentForPaymentEmailMap()
			{
				DiscriminatorValue(nameof(CounterpartyEmailType.OrderWithoutShipmentForPayment));
				References(x => x.OrderWithoutShipmentForPayment).Column("bill_ws_for_payment_id");
			}
		}

		public class OrderWithoutShipmentForAdvancePaymentEmailMap : SubclassMap<OrderWithoutShipmentForAdvancePaymentEmail>
		{
			public OrderWithoutShipmentForAdvancePaymentEmailMap()
			{
				DiscriminatorValue(nameof(CounterpartyEmailType.OrderWithoutShipmentForAdvancePayment));
				References(x => x.OrderWithoutShipmentForAdvancePayment).Column("bill_ws_for_advance_payment_id");
			}
		}

		public class ClosingDeliveriesEmailMap : SubclassMap<ClosingDeliveriesEmail>
		{
			public ClosingDeliveriesEmailMap()
			{
				DiscriminatorValue(nameof(CounterpartyEmailType.ClosingDeliveries));
				References(x => x.OrderWithoutShipmentForDebt).Column("bill_ws_for_debt_id");
			}
		}
	}
}
