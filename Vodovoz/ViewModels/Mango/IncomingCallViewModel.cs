using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.Infrastructure.Mango;

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

		public readonly bool IsTransfer;
		public string OnLine { get;private  set; }

		private Counterparty currentCounterparty { get; set; }

		public IncomingCallViewModel(
			IUnitOfWorkFactory UoWFactory, 
			INavigationManager navigation,
			ITdiCompatibilityNavigation tdinavigation,
			MangoManager manager) : base(navigation)
		{
			this.MangoManager = manager ?? throw new ArgumentNullException(nameof(manager));

			if(manager.IsTransfer && manager.PrimaryCaller != null) {
				IsTransfer = manager.IsTransfer;
				if(manager.PrimaryCaller != null) {
					if(manager.Employee != null)
						OnLine = manager.Employee.Name;
					else OnLine = manager.PrimaryCaller.Number;
				}
				if(manager.Clients != null) {
					counterpartyOrdersModels = new List<CounterpartyOrderViewModel>();
					foreach(Counterparty client in manager.Clients) {
						CounterpartyOrderViewModel model = new CounterpartyOrderViewModel(client, UoWFactory, tdinavigation);
						CounterpartyOrdersModels.Add(model);
					}
					currentCounterparty = CounterpartyOrdersModels.First().Client;
				}
			}
			IsModal = false;
			WindowPosition = WindowGravity.RightBottom;
			Title = "Входящий звонок";
		}

		#region Действия View

		public void DeclineCall()
		{
			MangoManager.HangUp();
			Close(false, CloseSource.Self);
		}

		#endregion
	}
}
