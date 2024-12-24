using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TransferEdoTaskMap : SubclassMap<TransferEdoTask>
	{
		public TransferEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.Transfer));

			References(x => x.DocumentEdoTaskId)
				.Column("document_edo_task_id");

			Map(x => x.FromOrganizationId)
				.Column("from_organization_id");

			Map(x => x.ToOrganizationId)
				.Column("to_organization_id");
		}
	}

	public class TransferEdoRequestMap : ClassMap<TransferEdoRequest>
	{
		public TransferEdoRequestMap()
		{
			Table("edo_transfer_requests");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			References(x => x.DocumentEdoTask)
				.Column("document_edo_task_id")
				.Cascade.All();

			Map(x => x.Time)
				.Column("time")
				.ReadOnly();

			Map(x => x.FromOrganizationId)
				.Column("from_organization_id");

			Map(x => x.ToOrganizationId)
				.Column("to_organization_id");

			HasManyToMany(x => x.TransferedItems)
				.Table("edo_transfered_items")
				.ParentKeyColumn("transfer_edo_request_id")
				.ChildKeyColumn("edo_task_item_id");

			References(x => x.TransferEdoTask)
				.Column("transfer_edo_task_id")
				.Cascade.All();
		}
	}
}
