using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class InventoryInstanceIncomingInvoiceItemMap : SubclassMap<InventoryInstanceIncomingInvoiceItem>
	{
		public InventoryInstanceIncomingInvoiceItemMap()
		{
			DiscriminatorValue(nameof(AccountingType.Instance));

			References(x => x.InventoryNomenclatureInstance).Column("inventory_nomenclature_instance_id");
		}
	}
}
