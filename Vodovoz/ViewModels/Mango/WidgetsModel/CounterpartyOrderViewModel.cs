using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.ViewModels.Mango
{
	public class CounterpartyArguments
	{

	}
	public class CounterpartyOrderViewModel : ViewModelBase
	{
		private Counterparty client;
		public Counterparty Client {
			get { return client;}
			private set { client = value; }
		}
		private ITdiCompatibilityNavigation tdiNavigation;
		private IUnitOfWork UoW;

		public List<Order> LatestOrder {get;private set;}

		public CounterpartyOrderViewModel(Counterparty client,
			IUnitOfWorkFactory unitOfWorkFactory,
			ITdiCompatibilityNavigation tdinavigation, 
			int count = 5) 
		: base()
		{
			this.client = client;
			this.tdiNavigation = tdinavigation;
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			OrderSingletonRepository orderRepos = OrderSingletonRepository.GetInstance();
			LatestOrder = orderRepos.GetLatestOrdersForCounterparty(UoW,client,count).ToList();
		}
		public void OpenMoreInformationAboutCounterparty()
		{
			var page = tdiNavigation.OpenTdiTab<CounterpartyDlg,int>(null,client.Id, OpenPageOptions.IgnoreHash);
			var tab = page.TdiTab as CounterpartyDlg;
			//tab.Entity
		}
		public void OpenMoreInformationAboutOrder(int id)
		{
			var page = tdiNavigation.OpenTdiTab<OrderDlg,int>(null, id,OpenPageOptions.IgnoreHash);
		}

	}
}
