using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class ValidationEdoTaskProblemMap : SubclassMap<ValidationEdoTaskProblem>
	{
		public ValidationEdoTaskProblemMap()
		{
			DiscriminatorValue(nameof(EdoTaskProblemType.Validation));
		}
	}
}
