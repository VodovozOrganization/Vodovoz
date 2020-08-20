using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using QS.Views.Dialog;
using Gtk;
using Vodovoz.Views.Mango;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Mango
{
	public class CounterpartyArguments
	{

	}
	public class CounterpartyOrderViewModel : UowDialogViewModelBase
	{
		private Counterparty client;
		public Counterparty Client {
			get { return client;}
			private set { client = value; }
		}
		private ITdiCompatibilityNavigation tdiNavigation;

		public List<Order> LatestOrder {get;private set;}

		public CounterpartyOrderViewModel(Counterparty client,IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, ITdiCompatibilityNavigation tdinavigation, int count = 5) 
		: base(unitOfWorkFactory, navigation)
		{
			this.client = client;
			this.tdiNavigation = tdinavigation;
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
