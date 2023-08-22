using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;

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
			Map(x => x.OwnType).Column("own_type").CustomType<OwnTypesStringType>();

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.Equipment).Column("equipment_id");
			References(x => x.Document).Column("car_load_document_id");
			References(x => x.GoodsAccountingOperation).Column("warehouse_movement_operation_id").Cascade.All();
			References(x => x.EmployeeNomenclatureMovementOperation).Column("employee_nomenclature_movement_operation_id").Cascade.All();
			References(x => x.DeliveryFreeBalanceOperation).Column("delivery_free_balance_operation_id").Cascade.All();
		}
	}
}
