using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
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

			Map(x => x.DriverCallType).Column("driver_call_type");
			Map(x => x.UndeliveryStatus).Column("status");
			Map(x => x.OldOrderStatus).Column("undelivered_order_status");
			Map(x => x.OrderTransferType).Column("transfer_type");

			References(x => x.OldOrder).Column("old_order_id");
			References(x => x.NewOrder).Column("new_order_id");
			References(x => x.InProcessAtDepartment).Column("in_process_at");
			References(x => x.EmployeeRegistrator).Column("registered_by_employee_id");
			References(x => x.Author).Column("author_employee_id");
			References(x => x.LastEditor).Column("editor_employee_id");
			References(x => x.ProblemSource).Column("undelivery_problem_source_id");
			References(x => x.UndeliveryTransferAbsenceReason).Column("undelivery_transfer_absence_reason_id");
			References(x => x.UndeliveryDetalization).Column("undelivery_detalization_id");

			HasMany(x => x.Fines).Inverse().KeyColumn("undelivery_id");
			HasMany(x => x.GuiltyInUndelivery).Cascade.AllDeleteOrphan().Inverse().KeyColumn("undelivery_id");
			HasMany(x => x.ResultComments).Cascade.All().Inverse().KeyColumn("undelivered_order_id");
			HasMany(x => x.UndeliveryDiscussions).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("undelivery_id");
		}
	}
}
