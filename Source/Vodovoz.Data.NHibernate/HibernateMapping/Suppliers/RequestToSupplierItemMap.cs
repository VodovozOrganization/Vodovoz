using FluentNHibernate.Mapping;
using Vodovoz.Domain.Suppliers;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Suppliers
{
	public class RequestToSupplierItemMap : ClassMap<RequestToSupplierItem>
	{
		public RequestToSupplierItemMap()
		{
			Table("requests_to_suppliers_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Quantity).Column("quantity");
			Map(x => x.Transfered).Column("transfered");

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.RequestToSupplier).Column("request_to_supplier_id");
			References(x => x.TransferedFromItem).Column("transfered_from_item_id");
		}
	}
}
