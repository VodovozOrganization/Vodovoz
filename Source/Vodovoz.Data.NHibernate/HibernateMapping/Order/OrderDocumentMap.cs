using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OrderDocumentMap : ClassMap<OrderDocument>
	{
		public OrderDocumentMap()
		{
			Table("order_documents");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("type");
			References(x => x.Order).Column("order_id");
			References(x => x.AttachedToOrder).Column("attached_to_order_id");
			References(x => x.DocumentOrganizationCounter).Column("document_organization_counter_id");
			//	.Cascade.SaveUpdate();
		}
	}

	public class OrderM2ProxyMap : SubclassMap<OrderM2Proxy>
	{
		public OrderM2ProxyMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.M2Proxy));
			References(x => x.M2Proxy).Column("m2proxy_id").Cascade.SaveUpdate();
		}
	}

	public class OrderAgreementMap : SubclassMap<OrderAgreement>
	{
		public OrderAgreementMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.AdditionalAgreement));
			References(x => x.AdditionalAgreement).Column("agreement_id");
		}
	}

	public class OrderContractMap : SubclassMap<OrderContract>
	{
		public OrderContractMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.Contract));
			References(x => x.Contract).Column("contract_id");
		}
	}

	public class BillDocumentMap : SubclassMap<BillDocument>
	{
		public BillDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.Bill));

			Map(x => x.HideSignature).Column("hide_signature");
		}
	}

	public class SpecialBillDocumentMap : SubclassMap<SpecialBillDocument>
	{
		public SpecialBillDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.SpecialBill));

			Map(x => x.HideSignature).Column("hide_signature");
		}
	}

	public class DoneWorkDocumentMap : SubclassMap<DoneWorkDocument>
	{
		public DoneWorkDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.DoneWorkReport));
		}
	}

	public class EquipmentTransferDocumentMap : SubclassMap<EquipmentTransferDocument>
	{
		public EquipmentTransferDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.EquipmentTransfer));
		}
	}

	public class EquipmentReturnDocumentMap : SubclassMap<EquipmentReturnDocument>
	{
		public EquipmentReturnDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.EquipmentReturn));
		}
	}


	public class InvoiceBarterDocumentMap : SubclassMap<InvoiceBarterDocument>
	{
		public InvoiceBarterDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.InvoiceBarter));
		}
	}

	public class InvoiceContractDocMap : SubclassMap<InvoiceContractDoc>
	{
		public InvoiceContractDocMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.InvoiceContractDoc));

			Map(x => x.WithoutAdvertising).Column("without_advertising");
			Map(x => x.HideSignature).Column("hide_signature");
		}
	}

	public class InvoiceDocumentMap : SubclassMap<InvoiceDocument>
	{
		public InvoiceDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.Invoice));

			Map(x => x.WithoutAdvertising).Column("without_advertising");
			Map(x => x.HideSignature).Column("hide_signature");
		}
	}

	public class UPDDocumentMap : SubclassMap<UPDDocument>
	{
		public UPDDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.UPD));
		}
	}

	public class SpecialUPDDocumentMap : SubclassMap<SpecialUPDDocument>
	{
		public SpecialUPDDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.SpecialUPD));
		}
	}

	public class DriverTicketDocumentMap : SubclassMap<DriverTicketDocument>
	{
		public DriverTicketDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.DriverTicket));
		}
	}

	public class Torg12DocumentMap : SubclassMap<Torg12Document>
	{
		public Torg12DocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.Torg12));
		}
	}

	public class ShetFacturaDocumentMap : SubclassMap<ShetFacturaDocument>
	{
		public ShetFacturaDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.ShetFactura));
		}
	}

	public class NomenclatureCertificateDocumentMap : SubclassMap<NomenclatureCertificateDocument>
	{
		public NomenclatureCertificateDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.ProductCertificate));
			References(x => x.Certificate).Column("certificate_id");
		}
	}

	public class TransportInvoiceDocumentMap : SubclassMap<TransportInvoiceDocument>
	{
		public TransportInvoiceDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.TransportInvoice));
		}
	}

	public class Torg2DocumentMap : SubclassMap<Torg2Document>
	{
		public Torg2DocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.Torg2));
		}
	}

	public class AssemblyListDocumentMap : SubclassMap<AssemblyListDocument>
	{
		public AssemblyListDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.AssemblyList));
		}
	}

	public class LetterOfDebtDocumentMap : SubclassMap<LetterOfDebtDocument>
	{
		public LetterOfDebtDocumentMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.LetterOfDebt));
			Map(x => x.HideSignature).Column("hide_signature");
		}
	}
}
