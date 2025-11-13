using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class CarLoadDocumentItemMap : ClassMap<CarLoadDocumentItem>
	{
		public CarLoadDocumentItemMap()
		{
			Table("store_car_load_document_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Amount).Column("amount");
			Map(x => x.ExpireDatePercent).Column("item_expiration_date_percent");
			Map(x => x.OwnType).Column("own_type");
			Map(x => x.OrderId).Column("order_id");
			Map(x => x.IsIndividualSetForOrder).Column("is_individual_set_for_order");

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.Equipment).Column("equipment_id");
			References(x => x.Document).Column("car_load_document_id");
			References(x => x.GoodsAccountingOperation).Column("warehouse_movement_operation_id").Cascade.All();
			References(x => x.EmployeeNomenclatureMovementOperation).Column("employee_nomenclature_movement_operation_id").Cascade.All();
			References(x => x.DeliveryFreeBalanceOperation).Column("delivery_free_balance_operation_id").Cascade.All();

			HasMany(x => x.TrueMarkCodes).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("car_load_document_item_id");
		}
	}
}
