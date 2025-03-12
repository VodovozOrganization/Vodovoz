using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoTaskProblemValidatorSourceEntityMap : SubclassMap<EdoTaskProblemValidatorSourceEntity>
	{
		public EdoTaskProblemValidatorSourceEntityMap()
		{
			DiscriminatorValue(nameof(EdoTaskProblemType.Validation));

			Map(x => x.Message)
				.Column("message");
		}
	}
}
