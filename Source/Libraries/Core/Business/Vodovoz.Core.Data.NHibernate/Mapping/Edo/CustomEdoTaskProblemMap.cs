using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class CustomEdoTaskProblemMap : SubclassMap<CustomEdoTaskProblem>
	{
		public CustomEdoTaskProblemMap()
		{
			DiscriminatorValue(nameof(EdoTaskProblemType.Custom));
		}
	}
}
