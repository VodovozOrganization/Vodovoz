using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.HibernateMapping
{
	public class OrderDocumentMap : ClassMap<OrderDocument>
	{
		public OrderDocumentMap ()
		{
			Table ("order_documents");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			DiscriminateSubClassesOnColumn ("type");
			References (x => x.Order).Column ("order_id");
			References (x => x.AttachedToOrder).Column ("attached_to_order_id");
		}
	}

	public class OrderAgreementMap : SubclassMap<OrderAgreement>
	{
		public OrderAgreementMap ()
		{
			DiscriminatorValue ("AdditionalAgreement");
			References (x => x.AdditionalAgreement).Column ("agreement_id");
		}
	}

	public class OrderContractMap : SubclassMap<OrderContract>
	{
		public OrderContractMap ()
		{
			DiscriminatorValue ("Contract");
			References (x => x.Contract).Column ("contract_id");
		}
	}

	public class BillDocumentMap : SubclassMap<BillDocument>
	{
		public BillDocumentMap()
		{
			DiscriminatorValue ("Bill");
		}
	}

	public class CoolerWarrantyDocumentMap:SubclassMap<CoolerWarrantyDocument>
	{
		public CoolerWarrantyDocumentMap()
		{
			DiscriminatorValue ("CoolerWarranty");
			Map(x => x.WarrantyNumber).Column("warranty_number");
			References(x => x.Contract).Column("contract_id");
			References(x => x.AdditionalAgreement).Column("agreement_id");
		}
	}

	public class DoneWorkDocumentMap:SubclassMap<DoneWorkDocument>
	{
		public DoneWorkDocumentMap()
		{
			DiscriminatorValue ("DoneWorkReport");
			References (x => x.ServiceClaim).Column ("service_claim_id");
		}
	}

	public class EquipmentTransferDocumentMap:SubclassMap<EquipmentTransferDocument>
	{
		public EquipmentTransferDocumentMap()
		{
			DiscriminatorValue ("EquipmentTransfer");
			References (x => x.ServiceClaim).Column ("service_claim_id");
		}
	}

	public class InvoiceBarterDocumentMap:SubclassMap<InvoiceBarterDocument>
	{
		public InvoiceBarterDocumentMap()
		{
			DiscriminatorValue ("InvoiceBarter");
		}
	}

	public class InvoiceDocumentMap:SubclassMap<InvoiceDocument>
	{
		public InvoiceDocumentMap()
		{
			DiscriminatorValue ("Invoice");

			Map(x => x.WithoutAdvertising).Column("without_advertising");
		}
	}

	public class PumpWarrantyDocumentMap:SubclassMap<PumpWarrantyDocument>
	{
		public PumpWarrantyDocumentMap()
		{
			DiscriminatorValue ("PumpWarranty");
			Map(x => x.WarrantyNumber).Column("warranty_number");
			References(x => x.Contract).Column("contract_id");
			References(x => x.AdditionalAgreement).Column("agreement_id");
		}
	}

	public class UPDDocumentMap:SubclassMap<UPDDocument>
	{
		public UPDDocumentMap()
		{
			DiscriminatorValue ("UPD");
		}
	}

	public class DriverTicketDocumentMap:SubclassMap<DriverTicketDocument>
	{
		public DriverTicketDocumentMap()
		{
			DiscriminatorValue ("DriverTicket");
		}
	}

	public class Torg12DocumentMap:SubclassMap<Torg12Document>
	{
		public Torg12DocumentMap()
		{
			DiscriminatorValue("Torg12");
		}
	}

	public class ShetFacturaDocumentMap:SubclassMap<ShetFacturaDocument>
	{
		public ShetFacturaDocumentMap()
		{
			DiscriminatorValue("ShetFactura");
		}
	}
}