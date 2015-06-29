using QSOrmProject;
using Vodovoz.Domain;


namespace Vodovoz.Repository
{
	public class CounterpartyRepository
	{
		public static CounterpartyContract GetCounterpartyContract (IUnitOfWork uow)
		{
			CounterpartyContract contract = null;
			Order order = uow.RootObject as Order;
			return null;
			//return uow.Session.QueryOver<CounterpartyContract> ()
			//	.Where (co => co.Counterparty == order.Client && !co.IsArchive && !co.OnCancellation);
		}


	}
}

