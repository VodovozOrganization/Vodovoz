using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class DocumentEdoTaskMap : SubclassMap<DocumentEdoTask>
	{
		public DocumentEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.CustomerDocument));

			Extends(typeof(CustomerEdoTask));
				
			Map(x => x.FromOrganization)
				.Column("from_organization_id");

			Map(x => x.ToClient)
				.Column("to_client_id");

			Map(x => x.DocumentType)
				.Column("document_type");
		}
	}
}
