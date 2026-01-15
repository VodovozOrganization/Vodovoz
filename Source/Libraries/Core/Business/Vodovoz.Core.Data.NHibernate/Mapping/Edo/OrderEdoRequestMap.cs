using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class OrderEdoRequestMap : SubclassMap<PrimaryEdoRequest>
	{
		public OrderEdoRequestMap()
		{
			DiscriminatorValue(nameof(EdoRequestType.Primary));
		}
	}
}
