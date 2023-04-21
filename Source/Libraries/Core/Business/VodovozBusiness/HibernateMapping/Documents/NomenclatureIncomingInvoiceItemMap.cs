using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class NomenclatureIncomingInvoiceItemMap : SubclassMap<NomenclatureIncomingInvoiceItem>
	{
		public NomenclatureIncomingInvoiceItemMap()
		{
			DiscriminatorValue(nameof(AccountingType.Bulk));
		}
	}
}
