using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoTaskMap : ClassMap<EdoTask>
	{
		public EdoTaskMap()
		{
			Table("edo_tasks");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.CreationTime)
				.Column("creation_time")
				.ReadOnly();

			Map(x => x.Status)
				.Column("status");

			Map(x => x.StartTime)
				.Column("start_time");

			Map(x => x.EndTime)
				.Column("end_time");

			HasMany(x => x.Problems)
				.KeyColumn("edo_task_id")
				.Cascade.AllDeleteOrphan()
				.Inverse();
		}
	}

	public class OutgoingEdoDocumentMap : ClassMap<OutgoingEdoDocument>
	{
		public OutgoingEdoDocumentMap()
		{
			Table("outgoing_edo_documents");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.CreationTime)
				.Column("creation_time")
				.ReadOnly();

			Map(x => x.Type)
				.Column("type")
				.ReadOnly();

			DiscriminateSubClassesOnColumn("type");

			Map(x => x.EdoType)
				.Column("edo_type");

			Map(x => x.DocumentType)
				.Column("document_type");

			Map(x => x.Status)
				.Column("status");

			Map(x => x.SendTime)
				.Column("send_time");

			Map(x => x.AcceptTime)
				.Column("accept_time");
		}
	}

	public class TransferEdoDocumentMap : SubclassMap<TransferEdoDocument>
	{
		public TransferEdoDocumentMap()
		{
			DiscriminatorValue(nameof(OutgoingEdoDocumentType.Transfer));

			Map(x => x.TransferTaskId)
				.Column("transfer_task_id");
		}
	}

	public class CustomerEdoDocumentMap : SubclassMap<CustomerEdoDocument>
	{
		public CustomerEdoDocumentMap()
		{
			DiscriminatorValue(nameof(OutgoingEdoDocumentType.Customer));

			Map(x => x.DocumentTaskId)
				.Column("document_task_id");
		}
	}
}
