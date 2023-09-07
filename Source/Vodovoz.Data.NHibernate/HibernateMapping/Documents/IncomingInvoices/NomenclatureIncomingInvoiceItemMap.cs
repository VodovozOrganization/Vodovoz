using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.IncomingInvoices;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.IncomingInvoices
{
	public class NomenclatureIncomingInvoiceItemMap : SubclassMap<NomenclatureIncomingInvoiceItem>
	{
		public NomenclatureIncomingInvoiceItemMap()
		{
			DiscriminatorValue(nameof(AccountingType.Bulk));
		}
	}
}
