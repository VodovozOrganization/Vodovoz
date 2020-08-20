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

namespace Vodovoz.ViewModels.Mango
{
	public class FullInternalCallViewModel : UowDialogViewModelBase
	{
		private ITdiCompatibilityNavigation tdiNavigation;

		private List<CounterpartyOrderViewModel> Models = new List<CounterpartyOrderViewModel>();

		public FullInternalCallViewModel(INavigationManager navigation,ITdiCompatibilityNavigation tdinavigation, IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory, navigation)
		{
			this.NavigationManager = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.tdiNavigation= tdinavigation ?? throw new ArgumentNullException(nameof(navigation));
			Title = "Входящий звонок существующего контрагента";

			Counterparty client = UoW.Session.Query<Counterparty>().Where(c => c.Id == 8).FirstOrDefault();
			CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(client, unitOfWorkFactory, navigation,tdinavigation);
			Models.Add(model);

			 client = UoW.Session.Query<Counterparty>().Where(c => c.Id == 9).FirstOrDefault();
			 model = new CounterpartyOrderViewModel(client, unitOfWorkFactory, navigation, tdinavigation);
			Models.Add(model);
		}

		public IDictionary<string, CounterpartyOrderView> GetWidgetPages() {
			IDictionary<string, CounterpartyOrderView> dict = new Dictionary<string, CounterpartyOrderView>();
			foreach(var m in Models) {
				CounterpartyOrderView view = new CounterpartyOrderView(m);
				dict.Add(m.Client.Name, view);
			}
			return dict;
		}
		#region Взаимодействие с Mango

		//private IList<Counterparty> GetCounterpartiesByPhone(Phone phone)
		//{
		//	return IList<Counterparty>
		//}
		#endregion

		#region Действия View
		public void LoadCounterpartyInformantion(object sender , EventArgs e)
		{
			//Notebook widget = (Notebook)sender;
		}

		#endregion
	}
}
