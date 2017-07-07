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
			DiscriminatorValue ("order_agreement");
			References (x => x.AdditionalAgreement).Column ("agreement_id");
		}
	}

	public class OrderContractMap : SubclassMap<OrderContract>
	{
		public OrderContractMap ()
		{
			DiscriminatorValue ("order_contract");
			References (x => x.Contract).Column ("contract_id");
		}
	}

	public class BillDocumentMap : SubclassMap<BillDocument>
	{
		public BillDocumentMap()
		{
			DiscriminatorValue ("bill_document");
		}
	}

	public class CoolerWarrantyDocumentMap:SubclassMap<CoolerWarrantyDocument>
	{
		public CoolerWarrantyDocumentMap()
		{
			DiscriminatorValue ("cooler_warranty");
		}
	}

	public class DoneWorkDocumentMap:SubclassMap<DoneWorkDocument>
	{
		public DoneWorkDocumentMap()
		{
			DiscriminatorValue ("done_work");
			References (x => x.ServiceClaim).Column ("service_claim_id");
		}
	}

	public class EquipmentTransferDocumentMap:SubclassMap<EquipmentTransferDocument>
	{
		public EquipmentTransferDocumentMap()
		{
			DiscriminatorValue ("equipment_transfer");
			References (x => x.ServiceClaim).Column ("service_claim_id");
		}
	}

	public class InvoiceBarterDocumentMap:SubclassMap<InvoiceBarterDocument>
	{
		public InvoiceBarterDocumentMap()
		{
			DiscriminatorValue ("invoice_barter");
		}
	}

	public class InvoiceDocumentMap:SubclassMap<InvoiceDocument>
	{
		public InvoiceDocumentMap()
		{
			DiscriminatorValue ("invoice");
		}
	}

	public class PumpWarrantyDocumentMap:SubclassMap<PumpWarrantyDocument>
	{
		public PumpWarrantyDocumentMap()
		{
			DiscriminatorValue ("pump_warranty");
		}
	}

	public class UPDDocumentMap:SubclassMap<UPDDocument>
	{
		public UPDDocumentMap()
		{
			DiscriminatorValue ("upd");
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