using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
	public class UndeliveredOrderMap : ClassMap<UndeliveredOrder>
	{
		public UndeliveredOrderMap()
		{
			Table("undelivered_orders");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DispatcherCallTime).Column("dispatcher_call_time");
			Map(x => x.DriverCallNr).Column("driver_call_nr");
			Map(x => x.DriverCallTime).Column("driver_call_time");
			Map(x => x.Reason).Column("reason");
			Map(x => x.TimeOfCreation).Column("creation_date");
			Map(x => x.LastEditedTime).Column("last_edited_time");

			Map(x => x.GuiltySide).Column("guilty_is").CustomType<UndeliveredOrderGuiltySideStringType>();
			Map(x => x.DriverCallType).Column("driver_call_type").CustomType<DriverCallTypeStringType>();
			Map(x => x.UndeliveryStatus).Column("status").CustomType<UndeliveredOrderUndeliveryStatusStringType>();
			Map(x => x.OldOrderStatus).Column("undelivered_order_status").CustomType<OrderStatusStringType>();

			References(x => x.OldOrder).Column("undelivered_order_id");
			References(x => x.NewOrder).Column("new_order_id");
			References(x => x.GuiltyDepartment).Column("guilty_department_id");
			References(x => x.EmployeeRegistrator).Column("registered_by_employee_id");
			References(x => x.Author).Column("author_employee_id");
			References(x => x.LastEditor).Column("editor_employee_id");

			HasMany(x => x.Fines).Inverse().KeyColumn("undelivered_order_id");
		}
	}
}
