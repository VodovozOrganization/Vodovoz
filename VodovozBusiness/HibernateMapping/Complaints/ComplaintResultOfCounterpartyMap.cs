using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.HibernateMapping.Complaints
{
	public class ComplaintResultOfCounterpartyMap : SubclassMap<ComplaintResultOfCounterparty>
	{
		public ComplaintResultOfCounterpartyMap()
		{
			DiscriminatorValue("Counterparty");
		}
	}
}
