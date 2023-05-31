using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.IncomingInvoices;

namespace Vodovoz.HibernateMapping.Documents.IncomingInvoices
{
	public class InventoryInstanceIncomingInvoiceItemMap : SubclassMap<InventoryInstanceIncomingInvoiceItem>
	{
		public InventoryInstanceIncomingInvoiceItemMap()
		{
			DiscriminatorValue(nameof(AccountingType.Instance));

			References(x => x.InventoryNomenclatureInstance).Column("nomenclature_instance_id");
		}
	}
}
