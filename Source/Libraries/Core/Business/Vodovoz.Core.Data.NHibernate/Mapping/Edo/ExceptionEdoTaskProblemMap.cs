using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class ExceptionEdoTaskProblemMap : SubclassMap<ExceptionEdoTaskProblem>
	{
		public ExceptionEdoTaskProblemMap()
		{
			DiscriminatorValue(nameof(EdoTaskProblemType.Exception));

			Map(x => x.ExceptionMessage)
				.Column("exception_message");
		}
	}
}
