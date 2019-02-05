using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Counterparty
{
	public class BottleDebtorMap : ClassMap<BottleDebtor>
	{
		public BottleDebtorMap()
		{
			Table("bottle_debtors");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DebtByAdress).Column("debt_by_address");
			Map(x => x.DebtByClient).Column("debt_by_client");
			Map(x => x.DateOfTaskCreation).Column("date_of_task_creation");
			Map(x => x.NextCallDate).Column("next_call_date");
			Map(x => x.TaskState).Column("task_state");
			Map(x => x.Comment).Column("comment");
			Map(x => x.IsTaskComplete).Column("is_task_complete");

			References(x => x.Client).Column("client_id");
			References(x => x.Address).Column("delivery_point_id");
			References(x => x.AssignedEmployee).Column("employee_id");

		}
	}
}
