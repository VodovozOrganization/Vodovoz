using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OrderDepositItemMap : ClassMap<OrderDepositItem>
	{
		public OrderDepositItemMap()
		{
			Table("order_deposit_items");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Deposit).Column("deposit_sum");
			Map(x => x.Count).Column("count");
			Map(x => x.ActualCount).Column("actual_count");
			Map(x => x.DepositType).Column("deposit_type");

			References(x => x.EquipmentNomenclature).Column("equip_nomenclature_id");
			References(x => x.Order).Column("order_id");
			References(x => x.DepositOperation).Column("deposit_operation_id");
		}
	}
}
