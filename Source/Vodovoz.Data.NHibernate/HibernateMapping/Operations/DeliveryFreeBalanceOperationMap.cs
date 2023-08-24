using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
{
	public class DeliveryFreeBalanceOperationMap : ClassMap<DeliveryFreeBalanceOperation>
	{

		public DeliveryFreeBalanceOperationMap()
		{
			Table("delivery_free_balance_operations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.OperationTime).Column("operation_time").ReadOnly();
			Map(x => x.Amount).Column("amount").Not.Nullable();

			References(x => x.RouteList).Column("route_list_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
