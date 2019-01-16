using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic
{
	public class LoadingUnloadingOperationMap : ClassMap<LoadingUnloadingOperation>
	{
		public LoadingUnloadingOperationMap()
		{
			Table("loading_unloading_operations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.RouteList).Column("route_list_id");
			References(x => x.Warehouse).Column("warehouse_id");
			Map(x => x.OperType).Column("operation_type").CustomType<OperationTypeStringType>();
			Map(x => x.IsActive).Column("is_active");
			Map(x => x.IsComplete).Column("is_complete");
		}
	}
}
