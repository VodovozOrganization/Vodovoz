using QSOrmProject;
using Vodovoz.Domain;
using NHibernate.Criterion;

namespace Vodovoz.Repository
{
	public static class CounterpartyRepository
	{
		public static QueryOver<Counterparty> ActiveClientsQuery ()
		{
			return QueryOver.Of<Counterparty> ()
				.Where (c => c.CounterpartyType == CounterpartyType.customer);
		}
	}
}

