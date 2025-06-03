using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TransferEdoDocumentMap : SubclassMap<TransferEdoDocument>
	{
		public TransferEdoDocumentMap()
		{
			DiscriminatorValue(nameof(OutgoingEdoDocumentType.Transfer));

			Map(x => x.TransferTaskId)
				.Column("transfer_task_id");
		}
	}
}
