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
			Map(x => x.Status).Column("status").CustomType<RequestStatusStringType>();
			Map(x => x.WithDelayOnly).Column("with_delay_only");

			References(x => x.Creator).Column("author_id");

			HasMany(x => x.RequestingNomenclatureItems).Cascade.AllDeleteOrphan().LazyLoad().Inverse()
				.KeyColumn("request_to_supplier_id");
		}
	}
}