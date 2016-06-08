using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HMap
{
	public class CounterpartyStatusMap : ClassMap<CounterpartyStatus>
	{
		public CounterpartyStatusMap ()
		{
			Table ("counterparty_status");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
		}
	}
}