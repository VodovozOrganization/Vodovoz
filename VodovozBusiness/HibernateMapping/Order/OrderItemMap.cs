using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain;

namespace Vodovoz.HMap
{
	public class OrderItemMap : ClassMap<OrderItem>
	{
		public OrderItemMap ()
		{
			Table ("order_items");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.Price)		.Column ("price");
			Map (x => x.Count)		.Column ("count");
			Map (x => x.ActualCount).Column ("actual_count");
			Map (x => x.IncludeNDS)	.Column ("include_nds");
			Map (x => x.Discount)	.Column ("discount");

			References (x => x.Order)						 .Column ("order_id");
			References (x => x.AdditionalAgreement)			 .Column ("additional_agreement_id");
			References (x => x.Equipment)					 .Column ("equipment_id");
			References (x => x.Nomenclature)				 .Column ("nomenclature_id");
			References (x => x.CounterpartyMovementOperation).Column ("counterparty_movement_operation_id").Cascade.All();
		}
	}
}