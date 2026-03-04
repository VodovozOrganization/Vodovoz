using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class ManualEdoRequestMap : SubclassMap<ManualEdoRequest>
	{
		public ManualEdoRequestMap()
		{
			DiscriminatorValue(nameof(EdoRequestType.Manual));
		}
	}
}
