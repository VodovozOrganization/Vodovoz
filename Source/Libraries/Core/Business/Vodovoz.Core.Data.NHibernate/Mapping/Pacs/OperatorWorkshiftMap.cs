using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Pacs
{
	public class OperatorWorkshiftMap : ClassMap<OperatorWorkshift>
	{
		public OperatorWorkshiftMap()
		{
			Table("pacs_operator_workshifts");

			Id(x => x.Id).Column("id")
				.GeneratedBy.Native();
			Map(x => x.OperatorId).Column("operator_id");
			References(x => x.PlannedWorkShift).Column("workshift_id")
				.Not.LazyLoad()
				.Fetch.Join();
			Map(x => x.Started).Column("started");
			Map(x => x.Ended).Column("ended");
			Map(x => x.Reason).Column("reason");
		}
	}
}
