using FluentNHibernate.Mapping;
using Vodovoz.Domain.Suppliers;

namespace Vodovoz.HibernateMapping.Suppliers
{
	public class RequestToSupplierMap : ClassMap<RequestToSupplier>
	{
		public RequestToSupplierMap()
		{
			Table("requests_to_suppliers");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.SuppliersOrdering).Column("ordering").CustomType<SupplierOrderingTypeStringType>();
			Map(x => x.Comment).Column("comment");
			Map(x => x.CreatingDate).Column("creating_date").ReadOnly();

			References(x => x.Creator).Column("author_id");

			HasManyToMany(x => x.RequestingNomenclatures)
				.Table("nomenclatures_to_requests_to_suppliers")
					.ParentKeyColumn("request_to_supplier_id")
					.ChildKeyColumn("nomenclature_id")
					.LazyLoad();
		}
	}
}