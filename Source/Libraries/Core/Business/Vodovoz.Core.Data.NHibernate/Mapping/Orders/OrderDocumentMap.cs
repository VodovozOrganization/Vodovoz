using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders.Documents;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Orders
{
	public class OrderDocumentMap : ClassMap<OrderDocumentEntity>
	{
		public OrderDocumentMap()
		{
			Table("order_documents");
			Not.LazyLoad();

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();
			
			References(x => x.Order)
				.Column("order_id");
			
			References(x => x.AttachedToOrder)
				.Column("attached_to_order_id");
			
			References(x => x.DocumentOrganizationCounter)
				.Column("document_organization_counter_id");
		}
	}

	public class OrderM2ProxyMap : SubclassMap<OrderM2ProxyEntity>
	{
		public OrderM2ProxyMap()
		{
			DiscriminatorValue("M2Proxy");

			References(x => x.M2Proxy)
				.Column("m2proxy_id")
				.Cascade.SaveUpdate();
		}
	}

	public class OrderAgreementMap : SubclassMap<OrderAgreementEntity>
	{
		public OrderAgreementMap()
		{
			DiscriminatorValue("AdditionalAgreement");

			References(x => x.AdditionalAgreement)
				.Column("agreement_id");
		}
	}

	public class OrderContractMap : SubclassMap<OrderContractEntity>
	{
		public OrderContractMap()
		{
			DiscriminatorValue("Contract");

			References(x => x.Contract)
				.Column("contract_id");
		}
	}

	public class BillDocumentMap : SubclassMap<BillDocumentEntity>
	{
		public BillDocumentMap()
		{
			DiscriminatorValue("Bill");

			Map(x => x.HideSignature)
				.Column("hide_signature");
		}
	}

	public class SpecialBillDocumentMap : SubclassMap<SpecialBillDocumentEntity>
	{
		public SpecialBillDocumentMap()
		{
			DiscriminatorValue("SpecialBill");

			Map(x => x.HideSignature)
				.Column("hide_signature");
		}
	}

	public class DoneWorkDocumentMap : SubclassMap<DoneWorkDocumentEntity>
	{
		public DoneWorkDocumentMap()
		{
			DiscriminatorValue("DoneWorkReport");
		}
	}

	public class EquipmentTransferDocumentMap : SubclassMap<EquipmentTransferDocumentEntity>
	{
		public EquipmentTransferDocumentMap()
		{
			DiscriminatorValue("EquipmentTransfer");
		}
	}

	public class EquipmentReturnDocumentMap : SubclassMap<EquipmentReturnDocumentEntity>
	{
		public EquipmentReturnDocumentMap()
		{
			DiscriminatorValue("EquipmentReturn");
		}
	}


	public class InvoiceBarterDocumentMap : SubclassMap<InvoiceBarterDocumentEntity>
	{
		public InvoiceBarterDocumentMap()
		{
			DiscriminatorValue("InvoiceBarter");
		}
	}

	public class InvoiceContractDocMap : SubclassMap<InvoiceContractDocEntity>
	{
		public InvoiceContractDocMap()
		{
			DiscriminatorValue("InvoiceContractDoc");

			Map(x => x.WithoutAdvertising)
				.Column("without_advertising");

			Map(x => x.HideSignature)
				.Column("hide_signature");
		}
	}

	public class InvoiceDocumentMap : SubclassMap<InvoiceDocumentEntity>
	{
		public InvoiceDocumentMap()
		{
			DiscriminatorValue("Invoice");

			Map(x => x.WithoutAdvertising)
				.Column("without_advertising");

			Map(x => x.HideSignature)
				.Column("hide_signature");
		}
	}

	public class UPDDocumentMap : SubclassMap<UPDDocumentEntity>
	{
		public UPDDocumentMap()
		{
			DiscriminatorValue("UPD");
		}
	}

	public class SpecialUPDDocumentMap : SubclassMap<SpecialUPDDocumentEntity>
	{
		public SpecialUPDDocumentMap()
		{
			DiscriminatorValue("SpecialUPD");
		}
	}

	public class DriverTicketDocumentMap : SubclassMap<DriverTicketDocumentEntity>
	{
		public DriverTicketDocumentMap()
		{
			DiscriminatorValue("DriverTicket");
		}
	}

	public class Torg12DocumentMap : SubclassMap<Torg12DocumentEntity>
	{
		public Torg12DocumentMap()
		{
			DiscriminatorValue("Torg12");
		}
	}

	public class ShetFacturaDocumentMap : SubclassMap<ShetFacturaDocumentEntity>
	{
		public ShetFacturaDocumentMap()
		{
			DiscriminatorValue("ShetFactura");
		}
	}

	public class NomenclatureCertificateDocumentMap : SubclassMap<NomenclatureCertificateDocumentEntity>
	{
		public NomenclatureCertificateDocumentMap()
		{
			DiscriminatorValue("ProductCertificate");

			References(x => x.Certificate)
				.Column("certificate_id");
		}
	}

	public class TransportInvoiceDocumentMap : SubclassMap<TransportInvoiceDocumentEntity>
	{
		public TransportInvoiceDocumentMap()
		{
			DiscriminatorValue("TransportInvoice");
		}
	}

	public class Torg2DocumentMap : SubclassMap<Torg2DocumentEntity>
	{
		public Torg2DocumentMap()
		{
			DiscriminatorValue("Torg2");
		}
	}

	public class AssemblyListDocumentMap : SubclassMap<AssemblyListDocumentEntity>
	{
		public AssemblyListDocumentMap()
		{
			DiscriminatorValue("AssemblyList");
		}
	}

	public class LetterOfDebtDocumentMap : SubclassMap<LetterOfDebtDocumentEntity>
	{
		public LetterOfDebtDocumentMap()
		{
			DiscriminatorValue("LetterOfDebt");
			Map(x => x.HideSignature)
				.Column("hide_signature");
		}
	}
}
