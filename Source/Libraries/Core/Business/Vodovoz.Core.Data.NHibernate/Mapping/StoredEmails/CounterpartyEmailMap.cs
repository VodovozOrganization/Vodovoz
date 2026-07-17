using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.StoredEmails;

namespace Vodovoz.Core.Data.NHibernate.Mapping.StoredEmails
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

		public class BillDocumentEmailMap : SubclassMap<BillDocumentEmail>
		{
			public BillDocumentEmailMap()
			{
				DiscriminatorValue(nameof(CounterpartyEmailType.BillDocument));
				References(x => x.OrderDocument).Column("order_document_id");
			}
		}

		public class UpdDocumentEmailMap : SubclassMap<UpdDocumentEmail>
		{
			public UpdDocumentEmailMap()
			{
				DiscriminatorValue(nameof(CounterpartyEmailType.UpdDocument));
				References(x => x.OrderDocument).Column("order_document_id");
			}
		}


		public class EquipmentTransferDocumentEmailMap : SubclassMap<EquipmentTransferDocumentEmail>
		{
			public EquipmentTransferDocumentEmailMap()
			{
				DiscriminatorValue(nameof(CounterpartyEmailType.EquipmentTransfer));
				References(x => x.OrderDocument).Column("order_document_id");
			}
		}

		public class BulkEmailMap : SubclassMap<BulkEmail>
		{
			public BulkEmailMap()
			{
				DiscriminatorValue(nameof(CounterpartyEmailType.Bulk));

				References(x => x.OrderDocument).Column("order_document_id");
			}
		}

		public class LetterOfClaimEmailMap : SubclassMap<LetterOfClaimEmail>
		{
			public LetterOfClaimEmailMap()
			{
				DiscriminatorValue(nameof(CounterpartyEmailType.LetterOfClaim));
			}
		}

		public class GeneralBillDocumentEmailMap : SubclassMap<GeneralBillDocumentEmail>
		{
			public GeneralBillDocumentEmailMap()
			{
				DiscriminatorValue(nameof(CounterpartyEmailType.GeneralBillDocument));
			}
		}

		public class InformationLetterEmailMap : SubclassMap<InformationLetterEmail>
		{
			public InformationLetterEmailMap()
			{
				DiscriminatorValue(nameof(CounterpartyEmailType.InformationLetter));
			}
		}
	}
}
