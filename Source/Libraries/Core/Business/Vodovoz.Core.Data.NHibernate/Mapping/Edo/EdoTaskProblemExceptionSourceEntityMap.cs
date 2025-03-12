using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoTaskProblemExceptionSourceEntityMap : SubclassMap<EdoTaskProblemExceptionSourceEntity>
	{
		public EdoTaskProblemExceptionSourceEntityMap()
		{
			DiscriminatorValue(nameof(EdoTaskProblemType.Exception));
		}
	}
}
