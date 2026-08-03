using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoTaskProblemRoutineStateMap : ClassMap<EdoTaskProblemRoutineState>
	{
		public EdoTaskProblemRoutineStateMap()
		{
			Table("edo_task_problem_routine_states");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			References(x => x.Problem)
				.Column("edo_task_problem_id")
				.Unique()
				.Not.Nullable();

			Map(x => x.RetryCount)
				.Column("retry_count");

			Map(x => x.LastRetryTime)
				.Column("last_retry_time");
		}
	}
}
