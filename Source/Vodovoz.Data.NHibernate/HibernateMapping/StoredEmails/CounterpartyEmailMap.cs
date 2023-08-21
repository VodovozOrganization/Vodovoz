﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.HibernateMapping.StoredEmails
{
	public class CounterpartyEmailMap : ClassMap<CounterpartyEmail>
	{
		public CounterpartyEmailMap()
		{
			Table("counterparty_emails");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("type");

			Map(x => x.Type).Column("type").CustomType<CounterpartyEmailStringType>().ReadOnly();

			References(x => x.StoredEmail).Column("stored_email_id");
			References(x => x.Counterparty).Column("counterparty_id");
		}

		public class OrderDocumentEmailMap : SubclassMap<OrderDocumentEmail>
		{
			public OrderDocumentEmailMap()
			{
				DiscriminatorValue(nameof(CounterpartyEmailType.OrderDocument));
				References(x => x.OrderDocument).Column("order_document_id");
			}
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

		public class BulkEmailMap : SubclassMap<BulkEmail>
		{
			public BulkEmailMap()
			{
				DiscriminatorValue(nameof(CounterpartyEmailType.Bulk));
			}
		}
	}
}
