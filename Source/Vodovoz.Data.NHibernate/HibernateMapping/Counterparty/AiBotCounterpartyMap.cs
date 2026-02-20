using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	/// <summary>
	/// Маппинг класса <see cref="AiBotCounterparty"/>
	/// </summary>
	public class AiBotCounterpartyMap : SubclassMap<AiBotCounterparty>
	{
		public AiBotCounterpartyMap()
		{
			DiscriminatorValue(nameof(CounterpartyFrom.AiBot));
		}
	}
}
