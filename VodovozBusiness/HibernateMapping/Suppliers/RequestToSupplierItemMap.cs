using FluentNHibernate.Mapping;
using Vodovoz.Domain.Suppliers;

namespace Vodovoz.HibernateMapping.Suppliers
{
	public class RequestToSupplierItemMap : ClassMap<RequestToSupplierItem>
	{
		public RequestToSupplierItemMap()
		{
			Table("requests_to_suppliers_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Quantity).Column("quantity");

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.RequestToSupplier).Column("request_to_supplier_id");
		}
	}
}
