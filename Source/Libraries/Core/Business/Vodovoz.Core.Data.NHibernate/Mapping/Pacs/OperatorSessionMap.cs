using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Pacs
{
	public class OperatorSessionMap : ClassMap<OperatorSession>
	{
		public OperatorSessionMap()
		{
			Table("pacs_sessions");

			Id(x => x.Id).Column("id")
                .GeneratedBy.Assigned();
			Map(x => x.Started).Column("started");
			Map(x => x.Ended).Column("ended");
			Map(x => x.OperatorId).Column("operator_id");
		}
	}
}
