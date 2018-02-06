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
			DiscriminatorValue (OrderAgreement.OrderDocumentTypeValue);
			References (x => x.AdditionalAgreement).Column ("agreement_id");
		}
	}

	public class OrderContractMap : SubclassMap<OrderContract>
	{
		public OrderContractMap ()
		{
			DiscriminatorValue (OrderContract.OrderDocumentTypeValue);
			References (x => x.Contract).Column ("contract_id");
		}
	}

	public class BillDocumentMap : SubclassMap<BillDocument>
	{
		public BillDocumentMap()
		{
			DiscriminatorValue (BillDocument.OrderDocumentTypeValue);
		}
	}

	public class CoolerWarrantyDocumentMap:SubclassMap<CoolerWarrantyDocument>
	{
		public CoolerWarrantyDocumentMap()
		{
			DiscriminatorValue (CoolerWarrantyDocument.OrderDocumentTypeValue);
			Map(x => x.WarrantyNumber).Column("warranty_number");
			References(x => x.Contract).Column("contract_id");
			References(x => x.AdditionalAgreement).Column("agreement_id");
		}
	}

	public class DoneWorkDocumentMap:SubclassMap<DoneWorkDocument>
	{
		public DoneWorkDocumentMap()
		{
			DiscriminatorValue (DoneWorkDocument.OrderDocumentTypeValue);
			References (x => x.ServiceClaim).Column ("service_claim_id");
		}
	}

	public class EquipmentTransferDocumentMap:SubclassMap<EquipmentTransferDocument>
	{
		public EquipmentTransferDocumentMap()
		{
			DiscriminatorValue (EquipmentTransferDocument.OrderDocumentTypeValue);
			References (x => x.ServiceClaim).Column ("service_claim_id");
		}
	}

	public class InvoiceBarterDocumentMap:SubclassMap<InvoiceBarterDocument>
	{
		public InvoiceBarterDocumentMap()
		{
			DiscriminatorValue (InvoiceBarterDocument.OrderDocumentTypeValue);
		}
	}

	public class InvoiceDocumentMap:SubclassMap<InvoiceDocument>
	{
		public InvoiceDocumentMap()
		{
			DiscriminatorValue (InvoiceDocument.OrderDocumentTypeValue);
		}
	}

	public class PumpWarrantyDocumentMap:SubclassMap<PumpWarrantyDocument>
	{
		public PumpWarrantyDocumentMap()
		{
			DiscriminatorValue (PumpWarrantyDocument.OrderDocumentTypeValue);
			Map(x => x.WarrantyNumber).Column("warranty_number");
			References(x => x.Contract).Column("contract_id");
			References(x => x.AdditionalAgreement).Column("agreement_id");
		}
	}

	public class UPDDocumentMap:SubclassMap<UPDDocument>
	{
		public UPDDocumentMap()
		{
			DiscriminatorValue (UPDDocument.OrderDocumentTypeValue);
		}
	}

	public class DriverTicketDocumentMap:SubclassMap<DriverTicketDocument>
	{
		public DriverTicketDocumentMap()
		{
			DiscriminatorValue (DriverTicketDocument.OrderDocumentTypeValue);
		}
	}

	public class Torg12DocumentMap:SubclassMap<Torg12Document>
	{
		public Torg12DocumentMap()
		{
			DiscriminatorValue(Torg12Document.OrderDocumentTypeValue);
		}
	}

	public class ShetFacturaDocumentMap:SubclassMap<ShetFacturaDocument>
	{
		public ShetFacturaDocumentMap()
		{
			DiscriminatorValue(ShetFacturaDocument.OrderDocumentTypeValue);
		}
	}
}