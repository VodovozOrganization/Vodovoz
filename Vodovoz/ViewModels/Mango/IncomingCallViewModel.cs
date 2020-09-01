using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.Navigation;
using Vodovoz.Domain.Client;
using QS.ViewModels.Dialog;
using Vodovoz.Infrastructure.Mango;
using QS.DomainModel.UoW;

namespace Vodovoz.ViewModels.Mango
{
	public class IncomingCallViewModel : WindowDialogViewModelBase
	{
		public readonly MangoManager MangoManager;

		private List<CounterpartyOrderViewModel> counterpartyOrdersModels = null;
		public List<CounterpartyOrderViewModel> CounterpartyOrdersModels {
			get => counterpartyOrdersModels;
			private set {
				counterpartyOrdersModels = value;
			}
		}
		private Counterparty currentCounterparty { get; set; }

		public IncomingCallViewModel(IUnitOfWorkFactory UoWFactory, 
			INavigationManager navigation,
			ITdiCompatibilityNavigation tdinavigation,
			MangoManager manager) : base(navigation)
		{
			this.MangoManager = manager ?? throw new ArgumentNullException(nameof(manager));
			manager.PropertyChanged += Manager_PropertyChanged;
			SetTitle();

			if(manager.Clients != null) {
				counterpartyOrdersModels = new List<CounterpartyOrderViewModel>();
				foreach(Counterparty client in manager.Clients) {
					CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(client, UoWFactory, tdinavigation);
					CounterpartyOrdersModels.Add(model);
				}
				currentCounterparty = CounterpartyOrdersModels.First().Client;
			}
			IsModal = false;
			WindowPosition = WindowGravity.RightBottom;
		}

		#region Действия View

		public void DeclineCall()
		{
			//FIXME Реализовать команду
		}

		#endregion

		void Manager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(MangoManager.StageDuration))
				SetTitle();
		}

		private void SetTitle()
		{
			Title = String.Format("Входящий звонок {0:mm\\:ss}", MangoManager.StageDuration);
		}

	}
}
