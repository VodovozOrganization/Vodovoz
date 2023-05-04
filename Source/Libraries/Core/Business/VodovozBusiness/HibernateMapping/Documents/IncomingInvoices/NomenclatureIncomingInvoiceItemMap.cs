using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.IncomingInvoices;

namespace Vodovoz.HibernateMapping.Documents.IncomingInvoices
{
	public class NomenclatureIncomingInvoiceItemMap : SubclassMap<NomenclatureIncomingInvoiceItem>
	{
		public NomenclatureIncomingInvoiceItemMap()
		{
			DiscriminatorValue(nameof(AccountingType.Bulk));
		}
	}
}
