using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping
{
	public class OrderDepositItemMap : ClassMap<OrderDepositItem>
	{
		public OrderDepositItemMap ()
		{
			Table ("order_deposit_items");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.Deposit).Column ("deposit_sum");
			Map (x => x.Count).Column ("count");
			Map (x => x.ActualCount).Column("actual_count");
			Map (x => x.DepositType).Column ("deposit_type").CustomType<DepositTypeStringType> ();
			Map (x => x.PaymentDirection).Column ("payment_type").CustomType<PaymentDirectionStringType> ();

			References (x => x.EquipmentNomenclature).Column("equip_nomenclature_id");
			References (x => x.Order).Column ("order_id");
			References (x => x.DepositOperation).Column ("deposit_operation_id");
			References (x => x.PaidRentItem).Column ("paid_rent_equipment_id");
			References (x => x.FreeRentItem).Column ("free_rent_equipment_id");
		}
	}
}