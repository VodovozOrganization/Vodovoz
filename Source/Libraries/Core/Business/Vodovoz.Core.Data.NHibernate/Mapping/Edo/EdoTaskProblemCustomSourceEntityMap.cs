using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoTaskProblemCustomSourceEntityMap : SubclassMap<EdoTaskProblemCustomSourceEntity>
	{
		public EdoTaskProblemCustomSourceEntityMap()
		{
			DiscriminatorValue(nameof(EdoTaskProblemType.Custom));

			Map(x => x.Message)
				.Column("message");
		}
	}
}
