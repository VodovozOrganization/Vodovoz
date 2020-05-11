using FluentNHibernate.Mapping;
using Vodovoz.Domain.BusinessTasks;

namespace Vodovoz.HibernateMapping.BusinessTasks
{
	public class PaymentTaskMap : ClassMap<PaymentTask>
	{
		public PaymentTaskMap()
		{
			Table("payment_tasks");
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreationDate).Column("date_of_task_creation");
			Map(x => x.CompleteDate).Column("date_complete");
			Map(x => x.EndActivePeriod).Column("deadline");
			Map(x => x.TaskState).Column("task_state").CustomType<BusinessTaskStatusStringType>();
			Map(x => x.Source).Column("task_source").CustomType<TaskSourceStringType>();
			Map(x => x.IsTaskComplete).Column("is_task_complete");

			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.AssignedEmployee).Column("employee_id");
			References(x => x.TaskCreator).Column("task_creator_id");

			HasManyToMany(x => x.Comments).Table("document_comments_to_documents")
								.ParentKeyColumn("payment_task_id")
								.ChildKeyColumn("comment_id")
								.LazyLoad();
		}
	}
}
